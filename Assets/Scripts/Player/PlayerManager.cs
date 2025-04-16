using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private CharacterData m_CharacterData;
    [SerializeField] private WeaponSystem m_WeaponSystem;
    [SerializeField] private Camera m_MainCamera;
    [SerializeField] private Transform m_GunTransform;
    [SerializeField] private Transform m_GunMuzzle;
    [SerializeField] private float m_InvisibleTime = 1f;
    [SerializeField] private float m_InvisibleTimer;
    public Transform GunMuzzle => m_GunMuzzle;

    private float m_MoveSpeed;
    private float m_Health;
    private float m_MaxHealth;

    void Start()
    {
        m_MaxHealth = m_Health = m_CharacterData.maxHealth;
        m_WeaponSystem.Initialize(m_CharacterData.startingWeapon, transform);
        if (m_MainCamera == null) m_MainCamera = Camera.main;
        UIManager.Instance.UpdateHealth(m_Health / m_MaxHealth);
    }

    void Update()
    {
        // m_WeaponSystem.Tick(Time.deltaTime);
        if (m_InvisibleTimer > 0)
        {
            m_InvisibleTimer -= Time.deltaTime;
        }
    }

    public void TakeDamage(float amount)
    {
        if (m_InvisibleTimer > 0) return;
        m_Health -= amount;
        UIManager.Instance.UpdateHealth(m_Health / m_MaxHealth);
        if (m_Health <= 0) GameManager.Instance.GameOver();
        m_InvisibleTimer = m_InvisibleTime;
    }

    public void Heal(float amount)
    {
        m_Health = Mathf.Min(m_Health + amount, m_MaxHealth);
        UIManager.Instance.UpdateHealth(m_Health / m_MaxHealth);
    }

    public void IncreaseSpeed(float amount) => m_MoveSpeed += amount;
}