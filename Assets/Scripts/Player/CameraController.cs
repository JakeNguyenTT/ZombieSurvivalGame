using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    // Screen shake state; the follow position is kept separately so shake never drifts the camera
    private Vector3 m_BasePosition;
    private float m_ShakeStrength;
    private float m_ShakeDuration;
    private float m_ShakeTimer;

    // Player reference and initial offset (used for initialization)
    [SerializeField] private Transform m_Player;
    [SerializeField] private Vector3 m_Offset = new Vector3(0, 10, -10);

    // Speed and smoothing controls
    [SerializeField] private float m_YawSpeed = 2f;         // Mouse horizontal sensitivity
    [SerializeField] private float m_PitchSpeed = 2f;       // Mouse vertical sensitivity
    [SerializeField] private float m_RotationSpeed = 5f;    // Rotation smoothing speed
    [SerializeField] private float m_FollowSpeed = 5f;      // Position smoothing speed

    // Camera positioning and look-at adjustments
    [SerializeField] private float m_LookHeight = 1f;       // Height offset for look-at point

    [Header("Read Only")]
    // Internal variables for rotation and distance
    [SerializeField] private float m_Yaw;           // Current horizontal angle
    [SerializeField] private float m_Pitch;         // Current vertical angle
    [SerializeField] private float m_TargetYaw;     // Target horizontal angle
    [SerializeField] private float m_TargetPitch;   // Target vertical angle
    [SerializeField] private float m_Distance;      // Distance from player

    void Start()
    {
        // Initialize yaw, pitch, and distance based on the initial offset
        Vector3 dir = (-m_Offset).normalized; // Direction from camera to player
        m_Yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        m_Pitch = -Mathf.Asin(dir.y) * Mathf.Rad2Deg;
        m_TargetYaw = m_Yaw;
        m_TargetPitch = m_Pitch;
        m_Distance = m_Offset.magnitude;

        // Set initial position to match the original behavior
        transform.position = m_Player.position + m_Offset;
        m_BasePosition = transform.position;
    }

    void Awake()
    {
        Instance = this;
    }

    // A stronger shake replaces a weaker one; a weaker one never cuts a stronger one short
    public void Shake(float strength, float duration)
    {
        float currentStrength = m_ShakeTimer > 0 ? m_ShakeStrength * (m_ShakeTimer / m_ShakeDuration) : 0f;
        if (strength < currentStrength) return;
        m_ShakeStrength = strength;
        m_ShakeDuration = duration;
        m_ShakeTimer = duration;
    }

    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            // Update target rotation based on mouse input
            m_TargetYaw += Input.GetAxis("Mouse X") * m_YawSpeed;
            m_TargetPitch -= Input.GetAxis("Mouse Y") * m_PitchSpeed;

            // Clamp pitch to prevent camera flipping (e.g., -80° to 80°)
            m_TargetPitch = Mathf.Clamp(m_TargetPitch, -80f, 80f);
        }
    }

    void LateUpdate()
    {
        // Smoothly interpolate current yaw and pitch toward targets
        m_Yaw = Mathf.Lerp(m_Yaw, m_TargetYaw, m_RotationSpeed * Time.deltaTime);
        m_Pitch = Mathf.Lerp(m_Pitch, m_TargetPitch, m_RotationSpeed * Time.deltaTime);

        // Calculate camera rotation and direction
        Quaternion rotation = Quaternion.Euler(m_Pitch, m_Yaw, 0);
        Vector3 direction = rotation * Vector3.forward;

        // Compute desired position: player position offset by distance in the direction
        Vector3 desiredPosition = m_Player.position - direction * m_Distance;

        // Smoothly move camera to the desired position
        m_BasePosition = Vector3.Lerp(m_BasePosition, desiredPosition, m_FollowSpeed * Time.deltaTime);
        transform.position = m_BasePosition;

        // Look at a point above the player for better framing
        transform.LookAt(m_Player.position + Vector3.up * m_LookHeight);

        if (m_ShakeTimer > 0)
        {
            m_ShakeTimer -= Time.deltaTime;
            float falloff = Mathf.Clamp01(m_ShakeTimer / m_ShakeDuration);
            transform.position = m_BasePosition + Random.insideUnitSphere * m_ShakeStrength * falloff;
        }
    }
}