using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeData", menuName = "Game/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    public UpgradeType type;
    public float value;
    public string description;
    public WeaponData weaponData;
    [Min(0)] public float weight = 1f; // relative chance of being offered on level-up
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
    Magnet,
}