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
        m_NameText.text = upgrade.name;
        m_DescriptionText.text = upgrade.description;
        m_UpgradeButton.onClick.RemoveAllListeners();
        m_UpgradeButton.onClick.AddListener(() => OnButtonUpgrade(upgrade));
    }

    public void OnButtonUpgrade(UpgradeData upgrade)
    {
        UIManager.Instance.SelectUpgrade(upgrade);
    }
}
