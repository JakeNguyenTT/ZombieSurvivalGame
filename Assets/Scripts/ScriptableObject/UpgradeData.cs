using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeData", menuName = "Game/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    public UpgradeType type;
    public float value;
    public string description;
    public WeaponData weaponData;
}

public enum UpgradeType
{
    Heal,
    AddWeapon,
    Penetration,
    MaxHealth,
    Speed,
    Damage,
    FireRate,
    ProjectileSpeed,
    MaxAmmo,
}