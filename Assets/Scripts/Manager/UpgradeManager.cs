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
    private readonly Dictionary<UpgradeType, int> m_PickCounts = new Dictionary<UpgradeType, int>();
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

    private int PickCount(UpgradeType type) => m_PickCounts.TryGetValue(type, out int count) ? count : 0;

    private bool IsAvailable(UpgradeData upgrade)
    {
        switch (upgrade.type)
        {
            case UpgradeType.Heal:
                return !m_Player.IsFullHealth;
            case UpgradeType.AddWeapon:
            {
                // New weapon, or a level-up for one we own
                if (upgrade.weaponData == null) return false;
                WeaponInstance owned = m_WeaponSystem.GetWeapon(upgrade.weaponData);
                if (owned != null) return !owned.IsMaxLevel;
                return !m_WeaponSystem.HasEvolutionOf(upgrade.weaponData);
            }
            case UpgradeType.Evolve:
            {
                // Base weapon maxed out and its paired passive picked at least once
                WeaponData evolved = upgrade.weaponData;
                if (evolved == null || evolved.evolvesFrom == null) return false;
                WeaponInstance baseWeapon = m_WeaponSystem.GetWeapon(evolved.evolvesFrom);
                return baseWeapon != null && baseWeapon.IsMaxLevel && PickCount(evolved.requiredPassive) > 0;
            }
            default:
                return true;
        }
    }

    // Card text, which for weapons depends on what the player already owns
    public (string title, string description) Describe(UpgradeData upgrade)
    {
        switch (upgrade.type)
        {
            case UpgradeType.AddWeapon when upgrade.weaponData != null:
            {
                WeaponInstance owned = m_WeaponSystem.GetWeapon(upgrade.weaponData);
                if (owned == null) return (upgrade.weaponData.weaponName, "New! " + upgrade.description);
                return ($"{upgrade.weaponData.weaponName} Lv {owned.level + 1}", LevelUpText(owned));
            }
            case UpgradeType.Evolve when upgrade.weaponData != null:
                return ($"Evolve: {upgrade.weaponData.weaponName}", upgrade.description);
            default:
                return (upgrade.name, upgrade.description);
        }
    }

    private static string LevelUpText(WeaponInstance weapon)
    {
        int next = weapon.level + 1;
        string text = "+20% damage";
        FiringType type = weapon.data.firingType;
        if ((type == FiringType.Orbit || type == FiringType.Homing) &&
            WeaponLevels.ExtraCount(next) > WeaponLevels.ExtraCount(weapon.level))
            text += type == FiringType.Orbit ? ", +1 blade" : ", +1 missile";
        if (type == FiringType.Aura) text += ", +15% radius";
        return text;
    }

    public void ApplyUpgrade(UpgradeData upgrade)
    {
        Debug.Log($"Applying upgrade: <color=green>{upgrade.name} + {upgrade.value}</color>");
        m_PickCounts[upgrade.type] = PickCount(upgrade.type) + 1;
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
                if (upgrade.weaponData == null)
                    Debug.LogWarning($"Upgrade {upgrade.name} has no weaponData");
                else if (m_WeaponSystem.HasWeapon(upgrade.weaponData))
                    m_WeaponSystem.LevelUpWeapon(upgrade.weaponData);
                else
                    m_WeaponSystem.AddWeapon(upgrade.weaponData);
                break;
            case UpgradeType.Evolve:
                if (upgrade.weaponData != null)
                    m_WeaponSystem.Evolve(upgrade.weaponData);
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
