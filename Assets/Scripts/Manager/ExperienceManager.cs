using UnityEngine;
using System;

public class ExperienceManager : MonoBehaviour
{
    public static ExperienceManager Instance { get; private set; }

    [SerializeField] private float[] expThresholds;
    private float currentExp;
    private int currentLevel;

    public event Action<UpgradeData[]> OnLevelUp;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddExperience(float amount)
    {
        currentExp += amount;
        UIManager.Instance.UpdateExperience(currentExp / expThresholds[currentLevel]);
        if (currentExp >= expThresholds[currentLevel])
            LevelUp();
    }

    private void LevelUp()
    {
        currentLevel++;
        currentExp = 0;
        Time.timeScale = 0f; // Pause game
        UpgradeData[] options = UpgradeManager.Instance.GetUpgradeOptions(3);
        OnLevelUp?.Invoke(options);
    }
}