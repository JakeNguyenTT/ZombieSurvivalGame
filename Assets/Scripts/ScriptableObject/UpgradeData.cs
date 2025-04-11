using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeData", menuName = "Game/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    public UpgradeType type;
    public float value;
    public WeaponData weaponData; // For AddWeapon type
    public string description;
}

public enum UpgradeType { IncreaseSpeed, AddWeapon, Heal }