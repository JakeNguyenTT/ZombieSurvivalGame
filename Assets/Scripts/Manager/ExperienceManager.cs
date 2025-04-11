using UnityEngine;
using System;

public class ExperienceManager : MonoBehaviour
{
    public static ExperienceManager Instance { get; private set; }

    [SerializeField] private float m_ExpThreshold = 100f;
    [Header("Read Only")]
    [SerializeField] private float m_CurrentExp;
    [SerializeField] private int m_CurrentLevel;

    public event Action<UpgradeData[]> OnLevelUp;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        m_CurrentExp = 0;
        m_CurrentLevel = 1;
        UIManager.Instance.UpdateExperience(m_CurrentExp / m_ExpThreshold);
        UIManager.Instance.UpdateLevel(m_CurrentLevel);
    }

    public void AddExperience(float amount)
    {
        m_CurrentExp += amount;
        if (m_CurrentExp >= m_ExpThreshold)
            LevelUp();
        else
            UIManager.Instance.UpdateExperience(m_CurrentExp / m_ExpThreshold);
    }

    private void LevelUp()
    {
        m_CurrentLevel++;
        m_CurrentExp = 0;
        UIManager.Instance.UpdateExperience(m_CurrentExp / m_ExpThreshold);
        UIManager.Instance.UpdateLevel(m_CurrentLevel);
        // GameManager.Instance.PauseGame();
        // UpgradeData[] options = UpgradeManager.Instance.GetUpgradeOptions(3);
        // OnLevelUp?.Invoke(options);
    }
}