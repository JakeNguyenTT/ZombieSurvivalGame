using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    [SerializeField] private PercentBar m_HealthBar;
    [SerializeField] private PercentBar m_ExpBar;
    [SerializeField] private TextMeshProUGUI m_LevelText;
    [SerializeField] private TextMeshProUGUI m_TimeText;
    [SerializeField] private TextMeshProUGUI m_EnemyKilledText;

    [Header("Panels")]
    [SerializeField] private UpgradePanel m_UpgradePanel;
    [SerializeField] private PausePanel m_PausePanel;
    [SerializeField] private GameOverPanel m_GameOverPanel;
    [SerializeField] private Image m_FadeBackground;
    [SerializeField] private TextMeshProUGUI m_GameOverTimeText;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        m_FadeBackground.gameObject.SetActive(false);
        m_UpgradePanel.gameObject.SetActive(false);
        m_PausePanel.gameObject.SetActive(false);
        m_GameOverPanel.gameObject.SetActive(false);
    }

    public void Initialize()
    {
        ExperienceManager.Instance.OnLevelUp += ShowUpgradeOptions;
        GameManager.Instance.OnGameOver += ShowGameOver;
    }

    public void UpdateHealth(float health, float maxHealth) => m_HealthBar.SetValue(health, maxHealth);
    public void UpdateExperience(float value, float maxValue) => m_ExpBar.SetValue(value, maxValue, true);
    public void UpdateLevel(int value) => m_LevelText.text = $"{value}";
    public void UpdateTime(float time) => m_TimeText.text = $"Time: {FormatTime(time)}";
    public void UpdateEnemyKilled(int value) => m_EnemyKilledText.text = $"Killed: {value}";

    private void ShowUpgradeOptions(UpgradeData[] options)
    {
        m_UpgradePanel.gameObject.SetActive(true);
        m_UpgradePanel.Initialize(options);
    }

    public void SelectUpgrade(UpgradeData upgrade)
    {
        UpgradeManager.Instance.ApplyUpgrade(upgrade);
        m_UpgradePanel.gameObject.SetActive(false);
        GameManager.Instance.ResumeGame();
    }

    private void ShowGameOver(float time)
    {
        m_FadeBackground.gameObject.SetActive(true);
        m_GameOverPanel.gameObject.SetActive(true);
        m_GameOverTimeText.text = $" {FormatTime(time)}";
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return $"{minutes:00}:{seconds:00}";
    }

    public void OnButtonPause()
    {
        m_FadeBackground.gameObject.SetActive(true);
        m_PausePanel.gameObject.SetActive(true);
        GameManager.Instance.PauseGame();
    }

    public void HideFadeBackground()
    {
        m_FadeBackground.gameObject.SetActive(false);
    }
}