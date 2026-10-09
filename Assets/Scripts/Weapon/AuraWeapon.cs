using UnityEngine;

// Damage ring around the player: every hitInterval it hurts all enemies inside the radius.
// Drawn as a LineRenderer circle; reads radius/damage live from the weapon.
public class AuraWeapon : MonoBehaviour
{
    private const int Segments = 48;
    private const float Height = 0.15f;

    private WeaponInstance m_Weapon;
    private Transform m_Player;
    private LineRenderer m_Ring;
    private float m_TickTimer;
    private float m_DrawnRadius = -1f;
    private float m_Pulse;

    public void Init(WeaponInstance weapon, Transform player)
    {
        m_Weapon = weapon;
        m_Player = player;

        m_Ring = gameObject.AddComponent<LineRenderer>();
        m_Ring.useWorldSpace = false;
        m_Ring.loop = true;
        m_Ring.positionCount = Segments;
        m_Ring.widthMultiplier = 0.08f;
        m_Ring.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        m_Ring.receiveShadows = false;
        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null) m_Ring.material = new Material(shader);
    }

    void Update()
    {
        transform.position = m_Player.position + Vector3.up * Height;
        if (!Mathf.Approximately(m_DrawnRadius, m_Weapon.radius)) DrawRing(m_Weapon.radius);
        if (Time.deltaTime <= 0f) return; // paused

        // Brighten briefly on each tick
        m_Pulse = Mathf.Max(0f, m_Pulse - Time.deltaTime * 4f);
        Color color = m_Weapon.data.color;
        color.a = 0.45f + 0.5f * m_Pulse;
        m_Ring.startColor = m_Ring.endColor = color;

        m_TickTimer -= Time.deltaTime;
        if (m_TickTimer > 0f) return;
        m_TickTimer = m_Weapon.data.hitInterval;
        m_Pulse = 1f;

        int hits = AreaDamage.Apply(m_Player.position, m_Weapon.radius, m_Weapon.damage);
        if (hits > 0 && m_Weapon.data.healPerTick > 0f)
            PlayerManager.Instance.Heal(m_Weapon.data.healPerTick);
    }

    private void DrawRing(float radius)
    {
        m_DrawnRadius = radius;
        for (int i = 0; i < Segments; i++)
        {
            float angle = 2f * Mathf.PI * i / Segments;
            m_Ring.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
        }
    }
}
