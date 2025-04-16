using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float m_Speed = 10f;
    [SerializeField] private ParticleSystem m_HitEffect;

    private float m_Damage;
    private Vector3 m_Direction;
    private bool m_IsActive;

    public void Initialize(Vector3 position, Vector3 dir, float dmg)
    {
        transform.position = position;
        transform.rotation = Quaternion.LookRotation(dir);
        m_Direction = dir.normalized;
        m_Damage = dmg;
        m_IsActive = true;
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (!m_IsActive) return;
        transform.position += m_Direction * m_Speed * Time.deltaTime;

        // Return to pool if too far from player
        if (Vector3.Distance(transform.position, GameManager.Instance.GetPlayerPosition()) > 20f)
            ReturnToPool();
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger: " + other.gameObject.name);
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyBehavior>().TakeDamage(m_Damage);
            PlayHitEffect();
            ReturnToPool();
        }

        if (other.CompareTag("Obstacle"))
        {
            PlayHitEffect();
            ReturnToPool();
        }
    }

    void OnCollisionEnter(Collision other)
    {
        Debug.Log("Collision: " + other.gameObject.name);
        if (other.gameObject.CompareTag("Enemy"))
        {
            other.gameObject.GetComponent<EnemyBehavior>().TakeDamage(m_Damage);
            PlayHitEffect();
            ReturnToPool();
        }

        if (other.gameObject.CompareTag("Obstacle"))
        {
            PlayHitEffect();
            ReturnToPool();
        }
    }

    private void PlayHitEffect()
    {
        ParticleSystem effect = Instantiate(m_HitEffect, transform.position, Quaternion.identity);
        effect.Play();
        Destroy(effect.gameObject, effect.main.duration);
    }

    private void ReturnToPool()
    {
        m_IsActive = false;
        gameObject.SetActive(false);
        ProjectilePool.Instance.ReturnProjectile(this);
    }
}