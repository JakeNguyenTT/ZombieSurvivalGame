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

    private bool m_LayoutFixed;

    // The card prefab was laid out for one short line; weapon and evolution text is longer
    private void FixLayout()
    {
        if (m_LayoutFixed) return;
        m_LayoutFixed = true;

        m_NameText.enableAutoSizing = true;
        m_NameText.fontSizeMin = 32;
        m_NameText.fontSizeMax = 70;
        m_NameText.rectTransform.sizeDelta = new Vector2(-40, 70); // side padding; anchors stretch

        m_DescriptionText.textWrappingMode = TextWrappingModes.Normal;
        m_DescriptionText.enableAutoSizing = true;
        m_DescriptionText.fontSizeMin = 26;
        m_DescriptionText.fontSizeMax = 46;
        m_DescriptionText.rectTransform.sizeDelta = new Vector2(-40, 300); // grows down from the top pivot

        TMP_Text buttonLabel = m_UpgradeButton.GetComponentInChildren<TMP_Text>(true);
        if (buttonLabel != null) buttonLabel.text = "Choose";
    }

    public void Setup(UpgradeData upgrade)
    {
        FixLayout();
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
