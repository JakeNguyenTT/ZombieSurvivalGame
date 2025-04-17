using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float m_Speed = 10f;
    [SerializeField] private ParticleSystem m_HitEffect;

    private float m_Damage;
    private Vector3 m_Direction;
    private bool m_IsActive;
    private int m_CurrentPenetrations;
    private int m_MaxPenetrations = 1;

    public void Initialize(Vector3 position, Vector3 dir, WeaponInstance weapon)
    {
        transform.position = position;
        transform.rotation = Quaternion.LookRotation(dir);
        m_Direction = dir.normalized;
        m_Damage = weapon.damage;
        m_MaxPenetrations = weapon.penetrationCount;
        m_IsActive = true;
        m_CurrentPenetrations = 0;
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
        CheckEnemy(other.gameObject);
        CheckObstacle(other.gameObject);
    }

    void OnCollisionEnter(Collision other)
    {
        CheckEnemy(other.gameObject);
        CheckObstacle(other.gameObject);
    }

    private void CheckEnemy(GameObject other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            other.gameObject.GetComponent<EnemyBehavior>().TakeDamage(m_Damage);
            PlayHitEffect();
            m_CurrentPenetrations++;
            if (m_CurrentPenetrations > m_MaxPenetrations)
                ReturnToPool();
        }
    }

    private void CheckObstacle(GameObject other)
    {
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