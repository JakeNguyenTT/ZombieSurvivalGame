using System.Collections.Generic;
using UnityEngine;

// Spitter shot. Built from a primitive at runtime and pooled; hits are distance checks
// against the player so no physics layers or prefabs are needed.
public class EnemyProjectile : MonoBehaviour
{
    private const float HitRadius = 0.6f;
    private const float Lifetime = 4f;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    private static readonly Queue<EnemyProjectile> s_Pool = new Queue<EnemyProjectile>();
    private static Transform s_Root;

    private Vector3 m_Velocity;
    private float m_Damage;
    private float m_Life;
    private Renderer m_Renderer;
    private MaterialPropertyBlock m_PropertyBlock;

    public static void Fire(Vector3 position, Vector3 direction, float speed, float damage, Color color)
    {
        if (s_Root == null)
        {
            // Previous scene's projectiles were destroyed with it
            s_Pool.Clear();
            s_Root = new GameObject("EnemyProjectiles").transform;
        }

        EnemyProjectile projectile = s_Pool.Count > 0 ? s_Pool.Dequeue() : Create();
        projectile.transform.position = position;
        projectile.m_Velocity = direction.normalized * speed;
        projectile.m_Damage = damage;
        projectile.m_Life = Lifetime;
        projectile.m_PropertyBlock.SetColor(BaseColorId, color);
        projectile.m_Renderer.SetPropertyBlock(projectile.m_PropertyBlock);
        projectile.gameObject.SetActive(true);
    }

    private static EnemyProjectile Create()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "EnemyProjectile";
        Destroy(go.GetComponent<Collider>());
        go.transform.SetParent(s_Root);
        go.transform.localScale = Vector3.one * 0.4f;
        var projectile = go.AddComponent<EnemyProjectile>();
        projectile.m_Renderer = go.GetComponent<Renderer>();
        projectile.m_PropertyBlock = new MaterialPropertyBlock();
        return projectile;
    }

    void Update()
    {
        transform.position += m_Velocity * Time.deltaTime;
        m_Life -= Time.deltaTime;

        Vector3 toPlayer = GameManager.Instance.GetPlayerPosition() - transform.position;
        toPlayer.y = 0;
        if (toPlayer.sqrMagnitude < HitRadius * HitRadius)
        {
            PlayerManager.Instance.TakeDamage(m_Damage);
            Return();
        }
        else if (m_Life <= 0)
        {
            Return();
        }
    }

    private void Return()
    {
        gameObject.SetActive(false);
        s_Pool.Enqueue(this);
    }
}
