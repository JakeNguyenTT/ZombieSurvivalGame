using UnityEngine;
using System.Collections.Generic;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [SerializeField] private List<UpgradeData> availableUpgrades;
    [SerializeField] private PlayerManager player;
    [SerializeField] private WeaponSystem weaponSystem;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public UpgradeData[] GetUpgradeOptions(int count)
    {
        List<UpgradeData> options = new List<UpgradeData>(availableUpgrades);
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
                player.IncreaseSpeed(upgrade.value);
                break;
            case UpgradeType.AddWeapon:
                weaponSystem.AddWeapon(upgrade.weaponData);
                break;
            case UpgradeType.Heal:
                player.Heal(upgrade.value);
                break;
        }
        Time.timeScale = 1f; // Resume game
    }
}