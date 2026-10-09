using System.Collections.Generic;
using UnityEngine;

// Blades circling the player; each blade hurts enemies it touches, at most once per
// hitInterval per enemy. Reads count/radius/damage live from the weapon, so level-ups
// and evolution apply immediately.
public class OrbitWeapon : MonoBehaviour
{
    private const float BladeHitRadius = 0.7f;
    private const float Height = 1f;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    private WeaponInstance m_Weapon;
    private Transform m_Player;
    private readonly List<Transform> m_Blades = new List<Transform>();
    private readonly Dictionary<EnemyBehavior, float> m_NextHitTime = new Dictionary<EnemyBehavior, float>();
    private MaterialPropertyBlock m_PropertyBlock;
    private float m_Angle;
    private Color? m_AppliedColor;

    public void Init(WeaponInstance weapon, Transform player)
    {
        m_Weapon = weapon;
        m_Player = player;
        m_PropertyBlock = new MaterialPropertyBlock();
    }

    void Update()
    {
        if (Time.deltaTime <= 0f) return; // paused
        SyncBlades();

        Vector3 center = m_Player.position + Vector3.up * Height;
        transform.position = center;
        m_Angle = (m_Angle + m_Weapon.data.rotationSpeed * Time.deltaTime) % 360f;
        for (int i = 0; i < m_Blades.Count; i++)
        {
            float angle = m_Angle + 360f * i / m_Blades.Count;
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            m_Blades[i].SetPositionAndRotation(center + rotation * Vector3.forward * m_Weapon.radius, rotation);
        }
        HitEnemies(center);
    }

    private void HitEnemies(Vector3 center)
    {
        var enemies = EnemySpawner.Instance.ActiveEnemies;
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            if (i >= enemies.Count) continue;
            EnemyBehavior enemy = enemies[i];
            if (m_NextHitTime.TryGetValue(enemy, out float next) && Time.time < next) continue;

            Vector3 enemyPos = enemy.transform.position;
            float reach = BladeHitRadius + 0.3f * enemy.transform.localScale.x;
            foreach (Transform blade in m_Blades)
            {
                Vector3 offset = enemyPos - blade.position;
                offset.y = 0;
                if (offset.sqrMagnitude > reach * reach) continue;
                m_NextHitTime[enemy] = Time.time + m_Weapon.data.hitInterval;
                Vector3 outward = enemyPos - center;
                enemy.TakeDamage(m_Weapon.damage, outward);
                break;
            }
        }
    }

    private void SyncBlades()
    {
        while (m_Blades.Count < m_Weapon.count)
        {
            GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blade.name = "Blade";
            Destroy(blade.GetComponent<Collider>());
            blade.transform.SetParent(transform, false);
            blade.transform.localScale = new Vector3(0.12f, 0.12f, 0.9f);
            m_Blades.Add(blade.transform);
            m_AppliedColor = null; // tint the new blade too
        }
        // New blades, or evolution changed the color
        if (m_AppliedColor != m_Weapon.data.color)
        {
            m_AppliedColor = m_Weapon.data.color;
            m_PropertyBlock.SetColor(BaseColorId, m_Weapon.data.color);
            foreach (Transform blade in m_Blades)
                blade.GetComponent<Renderer>().SetPropertyBlock(m_PropertyBlock);
        }
    }
}
