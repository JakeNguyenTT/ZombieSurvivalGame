using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_NameText;
    [SerializeField] private TextMeshProUGUI m_DescriptionText;
    [SerializeField] private Button m_UpgradeButton;

    public void Setup(UpgradeData upgrade)
    {
        // Weapon cards read differently depending on what is owned (new / level-up / evolve)
        var (title, description) = UpgradeManager.Instance.Describe(upgrade);
        m_NameText.text = title;
        m_DescriptionText.text = description;
        m_UpgradeButton.onClick.RemoveAllListeners();
        m_UpgradeButton.onClick.AddListener(() => OnButtonUpgrade(upgrade));
    }

    public void OnButtonUpgrade(UpgradeData upgrade)
    {
        UIManager.Instance.SelectUpgrade(upgrade);
    }
}
