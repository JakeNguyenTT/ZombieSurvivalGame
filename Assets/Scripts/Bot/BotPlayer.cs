using StarterAssets;
using UnityEngine;

// Plays the game for balance testing, through the same input path as a human: kites away from
// zombies (circling rather than backing into a corner), collects gems when it's safe, steers
// clear of rocks and trees, and picks upgrades with BotUpgradePolicy.
// Add it to the player object; it is never part of a normal game.
public class BotPlayer : MonoBehaviour
{
    private const float ThreatRadius = 7f;
    private const float GemRadius = 30f; // kills happen 10-25 units out (rifle range 50)
    private const float ObstacleRadius = 3f;
    private const float LeashRadius = 60f;
    private const float OrbitStrength = 0.35f;
    private const float OrbitFlipInterval = 8f;

    private StarterAssetsInputs m_Input;
    private PlayerManager m_Player;
    private Transform m_Camera;
    private Vector3 m_Start;
    private float m_OrbitSign = 1f;
    private float m_OrbitTimer = OrbitFlipInterval;
    private UpgradeData[] m_PendingOptions;

    void Start()
    {
        m_Input = GetComponentInParent<StarterAssetsInputs>();
        m_Player = PlayerManager.Instance;
        m_Camera = Camera.main != null ? Camera.main.transform : null;
        m_Start = m_Player.transform.position;
        ExperienceManager.Instance.OnLevelUp += options => m_PendingOptions = options;
    }

    void Update()
    {
        if (m_PendingOptions != null)
        {
            UpgradeData choice = BotUpgradePolicy.Choose(m_PendingOptions, m_Player.HealthFraction);
            m_PendingOptions = null;
            if (choice != null) UIManager.Instance.SelectUpgrade(choice);
        }

        if (!GameManager.Instance.IsPlaying)
        {
            m_Input.MoveInput(Vector2.zero);
            return;
        }

        m_OrbitTimer -= Time.deltaTime;
        if (m_OrbitTimer <= 0f)
        {
            m_OrbitTimer = OrbitFlipInterval;
            if (Random.value < 0.5f) m_OrbitSign = -m_OrbitSign;
        }

        m_Input.MoveInput(ToInput(DesiredDirection()));
    }

    private Vector3 DesiredDirection()
    {
        Vector3 position = m_Player.transform.position;
        Vector3 threat = Vector3.zero;
        foreach (EnemyBehavior enemy in EnemySpawner.Instance.ActiveEnemies)
        {
            Vector3 away = position - enemy.transform.position;
            away.y = 0f;
            float distance = away.magnitude - 0.5f * enemy.transform.localScale.x;
            if (distance > ThreatRadius) continue;
            distance = Mathf.Max(distance, 0.3f);
            float weight = enemy.IsBoss ? 1.5f : 1f;
            threat += away.normalized * 4f * weight / (distance * distance);
        }

        Vector3 move = threat;
        if (threat.sqrMagnitude > 0.0001f)
            move += Vector3.Cross(Vector3.up, threat.normalized) * m_OrbitSign * OrbitStrength * threat.magnitude;

        // Gems pull at a steady strength: worth grabbing unless a zombie is within ~3 units
        ExperienceGem gem = NearestGem(position);
        if (gem != null)
        {
            Vector3 toGem = gem.transform.position - position;
            toGem.y = 0f;
            move += toGem.normalized * 0.5f;
        }

        foreach (ArenaObstacle obstacle in ArenaProps.Obstacles)
        {
            var away = new Vector3(position.x - obstacle.Position.x, 0f, position.z - obstacle.Position.y);
            float distance = away.magnitude - obstacle.Radius;
            if (distance > ObstacleRadius) continue;
            move += away.normalized * (1f / Mathf.Max(distance, 0.2f));
        }

        Vector3 fromStart = position - m_Start;
        fromStart.y = 0f;
        if (fromStart.magnitude > LeashRadius)
            move -= fromStart.normalized * (fromStart.magnitude - LeashRadius) * 0.1f;

        return move;
    }

    private static ExperienceGem NearestGem(Vector3 position)
    {
        ExperienceGem nearest = null;
        float best = GemRadius * GemRadius;
        foreach (ExperienceGem gem in ExperienceGem.Active)
        {
            float sqr = (gem.transform.position - position).sqrMagnitude;
            if (sqr < best)
            {
                best = sqr;
                nearest = gem;
            }
        }
        return nearest;
    }

    // The controller moves relative to the camera's facing
    private Vector2 ToInput(Vector3 worldDirection)
    {
        if (worldDirection.sqrMagnitude < 0.0001f) return Vector2.zero;
        float worldAngle = Mathf.Atan2(worldDirection.x, worldDirection.z) * Mathf.Rad2Deg;
        float cameraYaw = m_Camera != null ? m_Camera.eulerAngles.y : 0f;
        float inputAngle = (worldAngle - cameraYaw) * Mathf.Deg2Rad;
        return new Vector2(Mathf.Sin(inputAngle), Mathf.Cos(inputAngle));
    }
}
