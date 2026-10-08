using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private ParticleSystem m_HitEffect;

    private float m_Speed;
    private float m_Range;
    private Vector3 m_StartPosition;
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
        m_StartPosition = position;
        m_Speed = weapon.projectileSpeed;
        m_Range = weapon.range;
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

        // Return to pool once it has travelled past the weapon's range
        if ((transform.position - m_StartPosition).sqrMagnitude > m_Range * m_Range)
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
        // Physics callbacks can still arrive after this projectile was returned in the same step
        if (!m_IsActive) return;
        if (other.CompareTag("Enemy") && other.TryGetComponent(out EnemyBehavior enemy))
        {
            enemy.TakeDamage(m_Damage);
            PlayHitEffect();
            m_CurrentPenetrations++;
            if (m_CurrentPenetrations > m_MaxPenetrations)
                ReturnToPool();
        }
    }

    private void CheckObstacle(GameObject other)
    {
        if (!m_IsActive) return;
        if (other.CompareTag("Obstacle"))
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
        if (!m_IsActive) return;
        m_IsActive = false;
        gameObject.SetActive(false);
        ProjectilePool.Instance.ReturnProjectile(this);
    }
}