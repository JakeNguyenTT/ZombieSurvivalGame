using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float health;
    [SerializeField] private float damage;
    [SerializeField] private ParticleSystem deathEffect;

    private float updateInterval = 0.1f;
    private float updateTimer;

    public void Initialize(Vector3 position, EnemyData data)
    {
        transform.position = position;
        speed = data.speed;
        health = data.health;
        damage = data.damage;
        updateTimer = Random.Range(0f, updateInterval);
        gameObject.SetActive(true);
    }

    void Update()
    {
        updateTimer -= Time.deltaTime;
        if (updateTimer <= 0)
        {
            MoveTowardsPlayer();
            updateTimer = updateInterval;
        }
    }

    private void MoveTowardsPlayer()
    {
        Vector3 direction = (GameManager.Instance.GetPlayerPosition() - transform.position);
        direction.y = 0; // Keep movement in XZ plane
        direction.Normalize();
        transform.position += direction * speed * updateInterval;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerManager>().TakeDamage(damage);
            Die();
        }
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0) Die();
    }

    private void Die()
    {
        ParticleSystem effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
        effect.Play();
        Destroy(effect.gameObject, effect.main.duration);
        gameObject.SetActive(false);
    }
}