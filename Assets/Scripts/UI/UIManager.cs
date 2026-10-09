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
    private TextMeshProUGUI m_RunStatsText;

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

    // Container for UI built from code; sits under the panels so they stay on top
    public RectTransform RuntimeHUD { get; private set; }
    public TMP_FontAsset HudFont => m_LevelText.font;

    public void Initialize()
    {
        ExperienceManager.Instance.OnLevelUp += ShowUpgradeOptions;
        GameManager.Instance.OnGameOver += ShowGameOver;
        CreateRuntimeHUD();
    }

    private void CreateRuntimeHUD()
    {
        Transform panelParent = m_FadeBackground.transform.parent;
        RuntimeHUD = RuntimeUI.CreateRect(panelParent, "RuntimeHUD");
        RuntimeUI.Stretch(RuntimeHUD);
        RuntimeHUD.SetSiblingIndex(m_FadeBackground.transform.GetSiblingIndex());
        RuntimeHUD.gameObject.AddComponent<DamageNumbers>().Init(RuntimeHUD, HudFont);
        RuntimeHUD.gameObject.AddComponent<BossIndicator>().Init(RuntimeHUD, HudFont);

        Button buttonTemplate = m_PausePanel.GetComponentInChildren<Button>(true);
        if (buttonTemplate != null)
            m_UpgradePanel.CreateRerollButton(buttonTemplate);
    }

    public void UpdateHealth(float health, float maxHealth)
    {
        m_HealthBar.SetValue(health, maxHealth);
        if (ScreenEffects.Instance != null) ScreenEffects.Instance.SetHealth(health, maxHealth);
    }
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

    private void ShowGameOver(RunResult result)
    {
        m_FadeBackground.gameObject.SetActive(true);
        m_GameOverPanel.gameObject.SetActive(true);
        m_GameOverTimeText.text = $" {FormatTime(result.Time)}";

        if (m_RunStatsText == null) CreateRunStatsText();
        string best = result.NewBestTime ? "\nNew best time!" : result.NewBestKills ? "\nNew best kills!" : "";
        m_RunStatsText.text =
            $"Kills {result.Kills}   Level {result.Level}   Bosses {result.Bosses}\n" +
            $"+{result.Coins} coins (total {SaveData.Coins}){best}";
    }

    // Copy of the time text (same font and style) centered between the time row and the buttons
    private void CreateRunStatsText()
    {
        m_RunStatsText = Instantiate(m_GameOverTimeText, m_GameOverPanel.transform);
        m_RunStatsText.name = "RunStatsText";
        var layoutElement = m_RunStatsText.GetComponent<LayoutElement>();
        if (layoutElement == null) layoutElement = m_RunStatsText.gameObject.AddComponent<LayoutElement>();
        layoutElement.ignoreLayout = true;
        m_RunStatsText.enableAutoSizing = false;
        m_RunStatsText.fontSize = 34;
        m_RunStatsText.alignment = TextAlignmentOptions.Center;
        m_RunStatsText.overflowMode = TextOverflowModes.Overflow;
        RuntimeUI.Place(m_RunStatsText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 10), new Vector2(760, 140));
    }

    private string FormatTime(float time) => RuntimeUI.FormatTime(time);

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