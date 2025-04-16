using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    [SerializeField] private float m_Speed = 2f;
    [SerializeField] private float m_Health = 100f;
    [SerializeField] private float m_Damage = 10f;
    [SerializeField] private ParticleSystem m_DeathEffect;

    private float m_UpdateInterval = 0.1f;
    private float m_UpdateTimer;

    public void Initialize(Vector3 position, EnemyData data)
    {
        transform.position = position;
        m_Speed = data.speed;
        m_Health = data.health;
        m_Damage = data.damage;
        m_UpdateTimer = Random.Range(0f, m_UpdateInterval);
        gameObject.SetActive(true);
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
            // Die();
        }
    }

    public void TakeDamage(float amount)
    {
        m_Health -= amount;
        if (m_Health <= 0) Die();
    }

    private void Die()
    {
        ParticleSystem effect = Instantiate(m_DeathEffect, transform.position, Quaternion.identity);
        effect.Play();
        Destroy(effect.gameObject, effect.main.duration);
        gameObject.SetActive(false);
    }
}