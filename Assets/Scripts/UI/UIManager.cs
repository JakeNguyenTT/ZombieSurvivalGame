using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider expBar;
    [SerializeField] private Text levelText;
    [SerializeField] private Text timeText;
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Button[] upgradeButtons;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text gameOverTimeText;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Initialize()
    {
        ExperienceManager.Instance.OnLevelUp += ShowUpgradeOptions;
        GameManager.Instance.OnGameOver += () => gameOverPanel.SetActive(true);
    }

    public void UpdateHealth(float value) => healthBar.value = value;
    public void UpdateExperience(float value) => expBar.value = value;
    public void UpdateTime(float time) => timeText.text = $"Time: {time:F1}s";

    private void ShowUpgradeOptions(UpgradeData[] options)
    {
        upgradePanel.SetActive(true);
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            if (i < options.Length)
            {
                upgradeButtons[i].gameObject.SetActive(true);
                upgradeButtons[i].GetComponentInChildren<Text>().text = options[i].description;
                int index = i;
                upgradeButtons[i].onClick.RemoveAllListeners();
                upgradeButtons[i].onClick.AddListener(() => SelectUpgrade(options[index]));
            }
            else
            {
                upgradeButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void SelectUpgrade(UpgradeData upgrade)
    {
        UpgradeManager.Instance.ApplyUpgrade(upgrade);
        upgradePanel.SetActive(false);
    }

    public void ShowGameOver(float time)
    {
        gameOverTimeText.text = $"Survived: {time:F1}s";
        gameOverPanel.SetActive(true);
    }
}