using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    [SerializeField] private float m_Speed = 2f;
    [SerializeField] private float m_Health = 100f;
    [SerializeField] private float m_Damage = 10f;
    [SerializeField] private ParticleSystem m_DeathEffect;
    bool m_IsBoss = false;
    EnemyData m_Data;

    // private float m_UpdateInterval = 0.1f;
    // private float m_UpdateTimer;

    public void Initialize(Vector3 position, EnemyData data, EnemyEnhancement enhancement)
    {
        transform.position = position;
        m_Speed = data.speed + enhancement.speed;
        m_Health = data.health + enhancement.health;
        m_Damage = data.damage + enhancement.damage;
        // m_UpdateTimer = Random.Range(0f, m_UpdateInterval);
        gameObject.SetActive(true);
        transform.localScale = Vector3.one;
        m_Data = data;
        m_IsBoss = false;
    }

    public void InitializeBoss(Vector3 position, EnemyData data, EnemyEnhancement enhancement, int bossLevel = 1)
    {
        // boss is 10 times bigger
        Initialize(position, data, enhancement);
        m_Speed = Mathf.Max(0.5f, data.speed - 1);
        m_Health = data.health * 10 * bossLevel;
        m_Damage = data.damage * 5 * bossLevel;
        transform.localScale = Vector3.one * 5;
        m_IsBoss = true;
    }

    void Update()
    {
        // m_UpdateTimer -= Time.deltaTime;
        // if (m_UpdateTimer <= 0)
        {
            MoveTowardsPlayer();
            // m_UpdateTimer = m_UpdateInterval;
        }
    }

    private void MoveTowardsPlayer()
    {
        Vector3 direction = (GameManager.Instance.GetPlayerPosition() - transform.position);
        direction.y = 0; // Keep movement in XZ plane
        if (direction.sqrMagnitude < 0.0001f) return;
        direction.Normalize();
        // transform.position += direction * m_Speed * m_UpdateInterval;
        transform.position += direction * m_Speed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    void OnTriggerStay(Collider other)
    {
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
        AudioManager.Instance.PlaySFX(m_Data.deathSound, transform.position);
        ExpSpawner.Instance.SpawnExp(transform.position);
        if (m_IsBoss)
        {
            ExpSpawner.Instance.SpawnExpAround(transform.position, 10, 5);
        }
        EffectPool.Play(m_DeathEffect, transform.position);
        gameObject.SetActive(false);
        EnemySpawner.Instance.ReturnEnemy(this);
    }
}