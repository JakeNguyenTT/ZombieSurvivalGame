using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePanel : MonoBehaviour
{
    [SerializeField] private UpgradeItem m_UpgradeItemPrefab;
    [SerializeField] private Transform m_UpgradeItemContainer;
    private List<UpgradeItem> m_UpgradeItems = new List<UpgradeItem>();
    private int m_PoolSize = 3;
    private Button m_RerollButton;

    // Built at runtime from an existing button so it matches the scene's style
    public void CreateRerollButton(Button template)
    {
        m_RerollButton = RuntimeUI.CloneButton(template, transform, "Reroll", OnButtonReroll);
        m_RerollButton.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
        var rect = (RectTransform)m_RerollButton.transform;
        RuntimeUI.Place(rect, new Vector2(0.5f, 0f), new Vector2(0, 90), new Vector2(320, 90));
        UpdateRerollButton();
    }

    private void OnButtonReroll()
    {
        if (UpgradeManager.Instance.TryReroll(out var options))
            Initialize(options);
    }

    private void UpdateRerollButton()
    {
        if (m_RerollButton == null) return;
        int rerolls = UpgradeManager.Instance.RerollsLeft;
        RuntimeUI.SetLabel(m_RerollButton, $"Reroll ({rerolls})");
        m_RerollButton.gameObject.SetActive(rerolls > 0);
    }

    private void Awake()
    {
        if (m_UpgradeItemContainer == null)
        {
            m_UpgradeItemContainer = transform;
        }
        m_UpgradeItemPrefab.gameObject.SetActive(false);
        InitPool();
    }

    private void InitPool()
    {
        m_UpgradeItems = new List<UpgradeItem>();
        for (int i = 0; i < m_PoolSize; i++)
        {
            var upgradeItem = Instantiate(m_UpgradeItemPrefab, m_UpgradeItemContainer);
            upgradeItem.gameObject.SetActive(false);
            m_UpgradeItems.Add(upgradeItem);
        }
    }

    public void Initialize(UpgradeData[] upgrades)
    {
        Clear();
        foreach (var upgrade in upgrades)
        {
            var upgradeItem = GetUpgradeItem();
            upgradeItem.Setup(upgrade);
            upgradeItem.gameObject.SetActive(true);
        }
        UpdateRerollButton();
    }

    private UpgradeItem GetUpgradeItem()
    {
        foreach (var upgradeItem in m_UpgradeItems)
        {
            if (!upgradeItem.gameObject.activeSelf)
            {
                return upgradeItem;
            }
        }
        return null;
    }

    private void Clear()
    {
        foreach (var upgradeItem in m_UpgradeItems)
        {
            upgradeItem.gameObject.SetActive(false);
        }
    }
}
