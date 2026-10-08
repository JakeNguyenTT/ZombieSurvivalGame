using StarterAssets;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    [SerializeField] private CharacterData m_CharacterData;
    [SerializeField] private WeaponSystem m_WeaponSystem;
    [SerializeField] private Camera m_MainCamera;
    [SerializeField] private Transform m_GunTransform;
    [SerializeField] private Transform m_GunMuzzle;
    [SerializeField] private float m_InvisibleTime = 1f;
    [SerializeField] private float m_InvisibleTimer;
    [SerializeField] private float m_RotationSpeed = 90f;
    public Transform GunMuzzle => m_GunMuzzle;
    public bool IsFullHealth => m_Health >= m_MaxHealth;
    private ThirdPersonController m_Controller;
    private float m_Health;
    private float m_MaxHealth;

    void Awake()
    {
        Instance = this;
        m_Controller = GetComponentInParent<ThirdPersonController>();
    }

    void Start()
    {
        m_MaxHealth = m_Health = m_CharacterData.maxHealth;
        m_WeaponSystem.Initialize(m_CharacterData.startingWeapon, transform);
        if (m_MainCamera == null) m_MainCamera = Camera.main;
        UIManager.Instance.UpdateHealth(m_Health, m_MaxHealth);
    }

    void Update()
    {
        LookAtClosestEnemy();
        m_WeaponSystem.Tick(Time.deltaTime);
        if (m_InvisibleTimer > 0)
        {
            m_InvisibleTimer -= Time.deltaTime;
        }
    }
    public void TakeDamage(float amount)
    {
        if (m_InvisibleTimer > 0) return;
        m_Health -= amount;
        UIManager.Instance.UpdateHealth(m_Health, m_MaxHealth);
        if (m_Health <= 0)
        {
            m_Health = 0;
            GameManager.Instance.GameOver();
        }
        else
        {
            m_InvisibleTimer = m_InvisibleTime;
        }
    }

    public void Heal(float amount)
    {
        m_Health = Mathf.Min(m_Health + amount, m_MaxHealth);
        UIManager.Instance.UpdateHealth(m_Health, m_MaxHealth);
    }

    public void IncreaseMaxHealth(float amount)
    {
        m_MaxHealth += amount;
        m_Health += amount;
        UIManager.Instance.UpdateHealth(m_Health, m_MaxHealth);
    }

    public void IncreaseSpeed(float amount)
    {
        if (m_Controller == null) return;
        m_Controller.MoveSpeed += amount;
        m_Controller.SprintSpeed += amount;
    }

    private void LookAtClosestEnemy()
    {
        Vector3 playerPos = transform.position;
        EnemyBehavior closestEnemy = EnemySpawner.Instance.GetClosestEnemy(playerPos);
        if (closestEnemy == null) return; // No enemies

        // Calculate direction to the closest enemy
        Vector3 direction = (closestEnemy.transform.position - playerPos).normalized;
        direction.y = 0; // Lock rotation to Y-axis (horizontal plane)

        if (direction == Vector3.zero) return; // Prevent rotation if direction is zero

        // Calculate the target rotation
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Smoothly rotate towards the enemy
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, m_RotationSpeed * Time.deltaTime);
    }
}