using UnityEngine;
using System.Collections.Generic;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }
    [SerializeField] private PlayerManager m_Player;
    [SerializeField] private WeaponSystem m_WeaponSystem;
    [SerializeField] private List<UpgradeData> m_AvailableUpgrades;
    [SerializeField] private int m_RerollsPerRun = 3;
    private readonly List<float> m_Weights = new List<float>();
    private int m_LastOptionCount = 3;

    public int RerollsLeft { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        RerollsLeft = m_RerollsPerRun;
    }

    // Weighted pick of distinct upgrades that make sense right now
    public UpgradeData[] GetUpgradeOptions(int count)
    {
        m_LastOptionCount = count;
        m_Weights.Clear();
        foreach (var upgrade in m_AvailableUpgrades)
            m_Weights.Add(upgrade != null && IsAvailable(upgrade) ? upgrade.weight : 0f);

        List<int> picked = WeightedRandom.PickIndices(m_Weights, count, () => Random.value);
        var result = new UpgradeData[picked.Count];
        for (int i = 0; i < picked.Count; i++)
            result[i] = m_AvailableUpgrades[picked[i]];
        return result;
    }

    public bool TryReroll(out UpgradeData[] options)
    {
        options = null;
        if (RerollsLeft <= 0) return false;
        RerollsLeft--;
        options = GetUpgradeOptions(m_LastOptionCount);
        return true;
    }

    private bool IsAvailable(UpgradeData upgrade)
    {
        switch (upgrade.type)
        {
            case UpgradeType.Heal:
                return !m_Player.IsFullHealth;
            case UpgradeType.AddWeapon:
                return upgrade.weaponData != null && !m_WeaponSystem.HasWeapon(upgrade.weaponData);
            default:
                return true;
        }
    }

    public void ApplyUpgrade(UpgradeData upgrade)
    {
        Debug.Log($"Applying upgrade: <color=green>{upgrade.name} + {upgrade.value}</color>");
        switch (upgrade.type)
        {
            case UpgradeType.Heal:
                m_Player.Heal(upgrade.value);
                break;
            case UpgradeType.MaxHealth:
                m_Player.IncreaseMaxHealth(upgrade.value);
                break;
            case UpgradeType.Speed:
                m_Player.IncreaseSpeed(upgrade.value);
                break;
            case UpgradeType.Magnet:
                m_Player.IncreasePickupRadius(upgrade.value);
                break;
            case UpgradeType.AddWeapon:
                if (upgrade.weaponData != null)
                    m_WeaponSystem.AddWeapon(upgrade.weaponData);
                else
                    Debug.LogWarning($"Upgrade {upgrade.name} has no weaponData");
                break;
            case UpgradeType.Penetration:
            case UpgradeType.Damage:
            case UpgradeType.FireRate:
            case UpgradeType.ProjectileSpeed:
            case UpgradeType.MaxAmmo:
                m_WeaponSystem.ApplyUpgrade(upgrade);
                break;
        }
    }
}
