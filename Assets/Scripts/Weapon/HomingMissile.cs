using System.Collections.Generic;
using UnityEngine;

// Seeks the nearest enemy and explodes on contact (or when its fuel runs out), hurting
// everything in the weapon's radius. Built from a primitive at runtime and pooled.
public class HomingMissile : MonoBehaviour
{
    private const float TurnRateDegrees = 360f;
    private const float Lifetime = 4f;
    private const float HitDistance = 0.8f;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    private static readonly Queue<HomingMissile> s_Pool = new Queue<HomingMissile>();
    private static Transform s_Root;

    private WeaponInstance m_Weapon;
    private EnemyBehavior m_Target;
    private Vector3 m_Velocity;
    private float m_Life;
    private Renderer m_Renderer;
    private MaterialPropertyBlock m_PropertyBlock;

    public static void Launch(Vector3 position, Vector3 direction, WeaponInstance weapon)
    {
        if (s_Root == null)
        {
            // The previous scene's missiles were destroyed with it
            s_Pool.Clear();
            s_Root = new GameObject("HomingMissiles").transform;
        }

        HomingMissile missile = s_Pool.Count > 0 ? s_Pool.Dequeue() : Create();
        missile.m_Weapon = weapon;
        missile.m_Target = null;
        missile.m_Life = Lifetime;
        missile.m_Velocity = direction.normalized * weapon.projectileSpeed;
        missile.transform.SetPositionAndRotation(position, Quaternion.LookRotation(direction));
        missile.m_PropertyBlock.SetColor(BaseColorId, weapon.data.color);
        missile.m_Renderer.SetPropertyBlock(missile.m_PropertyBlock);
        missile.gameObject.SetActive(true);
    }

    private static HomingMissile Create()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = "HomingMissile";
        Destroy(go.GetComponent<Collider>());
        go.transform.SetParent(s_Root);
        go.transform.localScale = new Vector3(0.18f, 0.3f, 0.18f);
        var missile = go.AddComponent<HomingMissile>();
        missile.m_Renderer = go.GetComponent<Renderer>();
        missile.m_PropertyBlock = new MaterialPropertyBlock();
        return missile;
    }

    void Update()
    {
        if (Time.deltaTime <= 0f) return; // paused
        if (m_Target == null || !m_Target.IsAlive || !m_Target.gameObject.activeInHierarchy)
            m_Target = EnemySpawner.Instance.GetClosestEnemy(transform.position);

        if (m_Target != null)
        {
            Vector3 toTarget = m_Target.transform.position + Vector3.up - transform.position;
            Vector3 flat = toTarget;
            flat.y = 0;
            if (flat.magnitude < HitDistance + 0.3f * m_Target.transform.localScale.x)
            {
                Explode();
                return;
            }
            Vector3 desired = toTarget.normalized * m_Weapon.projectileSpeed;
            m_Velocity = Vector3.RotateTowards(m_Velocity, desired, TurnRateDegrees * Mathf.Deg2Rad * Time.deltaTime, 0f);
        }

        transform.position += m_Velocity * Time.deltaTime;
        if (m_Velocity.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(m_Velocity) * Quaternion.Euler(90, 0, 0); // capsule points along Y

        m_Life -= Time.deltaTime;
        if (m_Life <= 0f) Explode();
    }

    private void Explode()
    {
        AreaDamage.Apply(transform.position, m_Weapon.radius, m_Weapon.damage);
        EffectPool.Play(m_Weapon.data.hitEffect, transform.position);
        gameObject.SetActive(false);
        s_Pool.Enqueue(this);
    }
}
