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

    [Header("Panels")]
    [SerializeField] private GameObject m_UpgradePanel;
    [SerializeField] private GameObject m_PausePanel;
    [SerializeField] private GameObject m_GameOverPanel;
    [SerializeField] private Image m_FadeBackground;
    [SerializeField] private TextMeshProUGUI m_GameOverTimeText;
    [SerializeField] private Button[] m_UpgradeButtons;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        m_FadeBackground.gameObject.SetActive(false);
        m_UpgradePanel.SetActive(false);
        m_PausePanel.SetActive(false);
        m_GameOverPanel.SetActive(false);
    }

    public void Initialize()
    {
        ExperienceManager.Instance.OnLevelUp += ShowUpgradeOptions;
        GameManager.Instance.OnGameOver += () => m_GameOverPanel.SetActive(true);
    }

    public void UpdateHealth(float value) => m_HealthBar.Value = value;
    public void UpdateExperience(float value) => m_ExpBar.Value = value;
    public void UpdateLevel(int value) => m_LevelText.text = $"{value}";
    public void UpdateTime(float time) => m_TimeText.text = $"Time: {FormatTime(time)}";

    private void ShowUpgradeOptions(UpgradeData[] options)
    {
        m_UpgradePanel.SetActive(true);
        for (int i = 0; i < m_UpgradeButtons.Length; i++)
        {
            if (i < options.Length)
            {
                m_UpgradeButtons[i].gameObject.SetActive(true);
                m_UpgradeButtons[i].GetComponentInChildren<Text>().text = options[i].description;
                int index = i;
                m_UpgradeButtons[i].onClick.RemoveAllListeners();
                m_UpgradeButtons[i].onClick.AddListener(() => SelectUpgrade(options[index]));
            }
            else
            {
                m_UpgradeButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void SelectUpgrade(UpgradeData upgrade)
    {
        UpgradeManager.Instance.ApplyUpgrade(upgrade);
        m_UpgradePanel.SetActive(false);
    }

    public void ShowGameOver(float time)
    {
        m_GameOverPanel.SetActive(true);
        m_GameOverTimeText.text = $"Survived: {FormatTime(time)}";
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return $"{minutes:00}:{seconds:00}";
    }

    public void OnButtonPause()
    {
        GameManager.Instance.PauseGame();
        m_FadeBackground.gameObject.SetActive(true);
        m_PausePanel.SetActive(true);
    }
}