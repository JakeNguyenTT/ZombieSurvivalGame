using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradePanel : MonoBehaviour
{
    [SerializeField] private UpgradeItem m_UpgradeItemPrefab;
    [SerializeField] private Transform m_UpgradeItemContainer;
    private List<UpgradeItem> m_UpgradeItems = new List<UpgradeItem>();
    private int m_PoolSize = 3;

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
