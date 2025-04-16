using UnityEngine;
using System.Collections.Generic;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }
    [SerializeField] private PlayerManager m_Player;
    [SerializeField] private WeaponSystem m_WeaponSystem;
    [SerializeField] private List<UpgradeData> m_AvailableUpgrades;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public UpgradeData[] GetUpgradeOptions(int count)
    {
        List<UpgradeData> options = new List<UpgradeData>(m_AvailableUpgrades);
        UpgradeData[] result = new UpgradeData[Mathf.Min(count, options.Count)];
        for (int i = 0; i < result.Length; i++)
        {
            int index = Random.Range(0, options.Count);
            result[i] = options[index];
            options.RemoveAt(index);
        }
        return result;
    }

    public void ApplyUpgrade(UpgradeData upgrade)
    {
        switch (upgrade.type)
        {
            case UpgradeType.IncreaseSpeed:
                m_Player.IncreaseSpeed(upgrade.value);
                break;
            case UpgradeType.AddWeapon:
                m_WeaponSystem.AddWeapon(upgrade.weaponData);
                break;
            case UpgradeType.Heal:
                m_Player.Heal(upgrade.value);
                break;
        }
        Time.timeScale = 1f; // Resume game
    }
}