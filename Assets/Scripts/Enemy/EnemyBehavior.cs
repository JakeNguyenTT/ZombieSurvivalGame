using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    [SerializeField] private float m_Speed = 2f;
    [SerializeField] private float m_Health = 100f;
    [SerializeField] private float m_Damage = 10f;
    [SerializeField] private ParticleSystem m_DeathEffect;

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
    }

    public void InitializeBoss(Vector3 position, EnemyData data, EnemyEnhancement enhancement, int bossLevel = 1)
    {
        // boss is 10 times bigger
        Initialize(position, data, enhancement);
        m_Speed = data.speed - 1;
        m_Health = data.health * 10 * bossLevel;
        m_Damage = data.damage * 5 * bossLevel;
        transform.localScale = Vector3.one * 5;
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
        direction.Normalize();
        // transform.position += direction * m_Speed * m_UpdateInterval;
        transform.position += direction * m_Speed * Time.deltaTime;
        transform.LookAt(GameManager.Instance.GetPlayerPosition());
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
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
        Debug.Log($"{gameObject.name} take damage: {amount}");
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
        ParticleSystem effect = Instantiate(m_DeathEffect, transform.position, Quaternion.identity);
        effect.Play();
        Destroy(effect.gameObject, effect.main.duration);
        gameObject.SetActive(false);
        EnemySpawner.Instance.ReturnEnemy(this);
    }
}