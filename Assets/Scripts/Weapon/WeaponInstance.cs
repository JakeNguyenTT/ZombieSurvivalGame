using UnityEngine;

[System.Serializable]
public class WeaponInstance
{
    public WeaponData data;
    public int level = 1;
    public float fireRate;
    public float damage;
    public float range;
    public float projectileSpeed;
    public int ammoCapacity;
    public int currentAmmo;
    public int count;
    public float radius;
    public int penetrationCount;
    public Transform muzzle;
    public float timer;

    // Initialize with base stats from WeaponData
    public void Initialize(WeaponData weaponData, Transform muzzle)
    {
        data = weaponData;
        level = 1;
        fireRate = weaponData.fireRate;
        damage = weaponData.damage;
        range = weaponData.range;
        projectileSpeed = weaponData.projectileSpeed;
        ammoCapacity = weaponData.maxAmmo;
        currentAmmo = ammoCapacity;
        penetrationCount = weaponData.pierceAll ? int.MaxValue / 2 : weaponData.penetration;
        timer = fireRate;
        this.muzzle = muzzle;
        RefreshLevelStats();
    }

    public bool IsMaxLevel => level >= WeaponLevels.MaxLevel;

    public void LevelUp()
    {
        if (IsMaxLevel) return;
        float before = WeaponLevels.DamageMultiplier(level);
        level++;
        damage *= WeaponLevels.DamageMultiplier(level) / before;
        RefreshLevelStats();
    }

    // Swap to the evolved weapon, carrying over everything gained from upgrades
    public void EvolveInto(WeaponData evolved)
    {
        float damageRatio = damage / data.damage;
        float fireRateRatio = fireRate / data.fireRate;
        float speedRatio = projectileSpeed / data.projectileSpeed;
        int extraPenetration = data.pierceAll ? 0 : penetrationCount - data.penetration;

        data = evolved;
        damage = evolved.damage * damageRatio;
        fireRate = evolved.fireRate * fireRateRatio;
        projectileSpeed = evolved.projectileSpeed * speedRatio;
        range = evolved.range;
        penetrationCount = evolved.pierceAll ? int.MaxValue / 2 : evolved.penetration + extraPenetration;
        RefreshLevelStats();
    }

    private void RefreshLevelStats()
    {
        bool countGrows = data.firingType == FiringType.Orbit || data.firingType == FiringType.Homing;
        count = data.count + (countGrows ? WeaponLevels.ExtraCount(level) : 0);
        radius = data.radius * (data.firingType == FiringType.Aura ? WeaponLevels.RadiusMultiplier(level) : 1f);
    }

    // Apply an upgrade to this weapon instance
    public void ApplyUpgrade(UpgradeData upgrade)
    {
        switch (upgrade.type)
        {
            case UpgradeType.Damage:
                damage += upgrade.value;
                break;
            case UpgradeType.FireRate:
                fireRate = Mathf.Max(0.05f, fireRate * (1f - upgrade.value)); // Reduce fire rate (faster shooting)
                break;
            case UpgradeType.MaxAmmo:
                ammoCapacity = Mathf.FloorToInt(ammoCapacity * (1f + upgrade.value));
                currentAmmo = Mathf.Min(currentAmmo + Mathf.FloorToInt(upgrade.value * data.maxAmmo), ammoCapacity);
                break;
            case UpgradeType.Penetration:
                penetrationCount += Mathf.FloorToInt(upgrade.value);
                break;
            case UpgradeType.ProjectileSpeed:
                projectileSpeed *= 1f + upgrade.value;
                break;
        }
    }
}
