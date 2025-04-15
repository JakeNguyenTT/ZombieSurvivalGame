using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private ParticleSystem hitEffect;

    private float damage;
    private Vector3 direction;
    private bool isActive;

    public void Initialize(Vector3 position, Vector3 dir, float dmg)
    {
        transform.position = position;
        transform.rotation = Quaternion.LookRotation(dir);
        direction = dir.normalized;
        damage = dmg;
        isActive = true;
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (!isActive) return;
        transform.position += direction * speed * Time.deltaTime;

        // Return to pool if too far from player
        if (Vector3.Distance(transform.position, GameManager.Instance.GetPlayerPosition()) > 20f)
            ReturnToPool();
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger: " + other.gameObject.name);
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyBehavior>().TakeDamage(damage);
            //PlayHitEffect();
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
            other.gameObject.GetComponent<EnemyBehavior>().TakeDamage(damage);
            //PlayHitEffect();
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
        ParticleSystem effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
        effect.Play();
        Destroy(effect.gameObject, effect.main.duration);
    }

    private void ReturnToPool()
    {
        isActive = false;
        gameObject.SetActive(false);
        ProjectilePool.Instance.ReturnProjectile(this);
    }
}