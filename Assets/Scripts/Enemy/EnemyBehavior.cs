using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    private const float ExploderTriggerDistance = 1.5f;
    private const float FuseBlinkInterval = 0.1f;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    [SerializeField] private float m_Speed = 2f;
    [SerializeField] private float m_Health = 100f;
    [SerializeField] private float m_Damage = 10f;
    [SerializeField] private ParticleSystem m_DeathEffect;
    bool m_IsBoss = false;
    EnemyData m_Data;

    private Renderer[] m_Renderers;
    private MaterialPropertyBlock m_PropertyBlock;
    private float m_AttackTimer;
    private float m_FuseTimer = -1f;

    public EnemyBehavior SourcePrefab { get; set; }
    public EnemyData Data => m_Data;
    public bool IsBoss => m_IsBoss;
    public bool IsAlive => m_Health > 0;

    void Awake()
    {
        m_Renderers = GetComponentsInChildren<Renderer>(true);
        m_PropertyBlock = new MaterialPropertyBlock();
    }

    public void Initialize(Vector3 position, EnemyData data, EnemyEnhancement enhancement)
    {
        transform.position = position;
        m_Speed = data.speed + enhancement.speed;
        m_Health = data.health + enhancement.health;
        m_Damage = data.damage + enhancement.damage;
        gameObject.SetActive(true);
        transform.localScale = Vector3.one * data.scale;
        m_Data = data;
        m_IsBoss = false;
        m_AttackTimer = data.attackCooldown;
        m_FuseTimer = -1f;
        SetColor(data.tint);
    }

    public void InitializeBoss(Vector3 position, EnemyData data, int bossLevel, float healthMultiplier)
    {
        Initialize(position, data, new EnemyEnhancement());
        m_Speed = Mathf.Max(0.5f, data.speed - 1);
        m_Health = data.health * 10 * bossLevel * healthMultiplier;
        m_Damage = data.damage * (2 + bossLevel);
        transform.localScale = Vector3.one * 5;
        m_IsBoss = true;
    }

    void Update()
    {
        Vector3 toPlayer = GameManager.Instance.GetPlayerPosition() - transform.position;
        toPlayer.y = 0; // Keep movement in XZ plane
        float distance = toPlayer.magnitude;
        if (distance < 0.01f) return;
        Vector3 direction = toPlayer / distance;
        transform.rotation = Quaternion.LookRotation(direction);

        switch (m_IsBoss ? EnemyArchetype.Walker : m_Data.archetype)
        {
            case EnemyArchetype.Spitter:
                UpdateSpitter(direction, distance);
                break;
            case EnemyArchetype.Exploder:
                UpdateExploder(direction, distance);
                break;
            default:
                Move(direction);
                break;
        }
    }

    private void Move(Vector3 direction)
    {
        transform.position += direction * m_Speed * Time.deltaTime;
    }

    private void UpdateSpitter(Vector3 direction, float distance)
    {
        if (distance > m_Data.attackRange)
            Move(direction);

        m_AttackTimer -= Time.deltaTime;
        if (m_AttackTimer <= 0 && distance <= m_Data.attackRange * 1.2f)
        {
            Vector3 muzzle = transform.position + Vector3.up * 1.2f + direction * 0.5f;
            EnemyProjectile.Fire(muzzle, direction, m_Data.projectileSpeed, m_Damage, m_Data.tint);
            m_AttackTimer = m_Data.attackCooldown;
        }
    }

    private void UpdateExploder(Vector3 direction, float distance)
    {
        if (m_FuseTimer < 0)
        {
            Move(direction);
            if (distance < ExploderTriggerDistance)
                m_FuseTimer = m_Data.fuseTime;
            return;
        }

        m_FuseTimer -= Time.deltaTime;
        bool blinkOn = Mathf.FloorToInt(m_FuseTimer / FuseBlinkInterval) % 2 == 0;
        SetColor(blinkOn ? Color.white : m_Data.tint);
        if (m_FuseTimer <= 0)
            Explode();
    }

    private void Explode()
    {
        Vector3 toPlayer = GameManager.Instance.GetPlayerPosition() - transform.position;
        toPlayer.y = 0;
        if (toPlayer.magnitude <= m_Data.explodeRadius)
            PlayerManager.Instance.TakeDamage(m_Damage);
        Die();
    }

    void OnTriggerStay(Collider other)
    {
        // Exploders only hurt by exploding
        if (!m_IsBoss && m_Data != null && m_Data.archetype == EnemyArchetype.Exploder) return;
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerManager>().TakeDamage(m_Damage);
        }
    }

    public void TakeDamage(float amount)
    {
        // Already dead (several hits can land in the same physics step)
        if (m_Health <= 0) return;
        m_Health -= amount;
        if (m_Health <= 0) Die();
        else
        {
            AudioManager.Instance.PlaySFX(m_Data.hurtSound, transform.position);
        }
    }

    private void Die()
    {
        m_Health = 0;
        AudioManager.Instance.PlaySFX(m_Data.deathSound, transform.position);
        ExpSpawner.Instance.SpawnExp(transform.position);
        if (m_IsBoss)
            ExpSpawner.Instance.SpawnExpAround(transform.position, 10, 5);
        else if (m_Data.expDrops > 1)
            ExpSpawner.Instance.SpawnExpAround(transform.position, m_Data.expDrops - 1, 1.5f);
        EffectPool.Play(m_DeathEffect, transform.position);
        gameObject.SetActive(false);
        EnemySpawner.Instance.ReturnEnemy(this);
    }

    private void SetColor(Color color)
    {
        foreach (var r in m_Renderers)
        {
            r.GetPropertyBlock(m_PropertyBlock);
            m_PropertyBlock.SetColor(BaseColorId, color);
            r.SetPropertyBlock(m_PropertyBlock);
        }
    }
}
