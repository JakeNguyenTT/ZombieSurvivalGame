using UnityEngine;

[System.Serializable]
public class WeaponInstance
{
    public WeaponData data;
    public float fireRate;
    public float damage;
    public float range;
    public int ammoCapacity;
    public int currentAmmo;
    public int spreadCount;
    public int penetrationCount;
    public Transform muzzle;
    public float timer;

    // Initialize with base stats from WeaponData
    public void Initialize(WeaponData weaponData, Transform muzzle)
    {
        data = weaponData;
        fireRate = weaponData.fireRate;
        damage = weaponData.damage;
        range = weaponData.range;
        ammoCapacity = weaponData.maxAmmo;
        currentAmmo = ammoCapacity;
        penetrationCount = weaponData.penetration;
        timer = fireRate;
        this.muzzle = muzzle;
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
        }
        Debug.Log($"Upgraded {data.weaponName}: Damage={damage}, FireRate={fireRate}, Ammo={currentAmmo}/{ammoCapacity}, Penetration={penetrationCount}");
    }
}