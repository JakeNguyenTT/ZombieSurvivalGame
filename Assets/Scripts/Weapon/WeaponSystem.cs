using UnityEngine;
using System.Collections.Generic;

public class WeaponSystem : MonoBehaviour
{
    [SerializeField] private ProjectilePool m_ProjectilePool;
    [Header("Read Only")]
    [SerializeField] private List<WeaponInstance> m_ActiveWeapons = new List<WeaponInstance>();
    [SerializeField] private Transform m_PlayerTransform;
    private Transform m_Muzzle;

    // Permanent damage bonus applied to every weapon when it is added
    public float DamageMultiplier { get; set; } = 1f;

    public IReadOnlyList<WeaponInstance> Weapons => m_ActiveWeapons;

    public void Initialize(WeaponData startingWeapon, Transform playerTransform)
    {
        m_PlayerTransform = playerTransform;
        m_Muzzle = m_PlayerTransform.GetComponent<PlayerManager>().GunMuzzle;
        AddWeapon(startingWeapon, m_Muzzle);
    }

    public void Tick(float deltaTime)
    {
        foreach (var weapon in m_ActiveWeapons)
        {
            // Orbit and aura weapons run continuously from their own components
            if (IsContinuous(weapon.data)) continue;
            weapon.timer -= deltaTime;
            if (weapon.timer <= 0)
            {
                FireWeapon(weapon);
                weapon.timer = weapon.fireRate;
            }
        }
    }

    private static bool IsContinuous(WeaponData data) =>
        data.firingType == FiringType.Orbit || data.firingType == FiringType.Aura;

    public void AddWeapon(WeaponData weaponData)
    {
        AddWeapon(weaponData, m_Muzzle != null ? m_Muzzle : m_PlayerTransform);
    }

    public void AddWeapon(WeaponData weaponData, Transform muzzle)
    {
        WeaponInstance weapon = new WeaponInstance();
        weapon.Initialize(weaponData, muzzle);
        weapon.damage *= DamageMultiplier;
        m_ActiveWeapons.Add(weapon);

        if (weaponData.firingType == FiringType.Orbit)
            new GameObject(weaponData.weaponName).AddComponent<OrbitWeapon>().Init(weapon, m_PlayerTransform);
        else if (weaponData.firingType == FiringType.Aura)
            new GameObject(weaponData.weaponName).AddComponent<AuraWeapon>().Init(weapon, m_PlayerTransform);
    }

    public WeaponInstance GetWeapon(WeaponData weaponData) => m_ActiveWeapons.Find(w => w.data == weaponData);

    public bool HasWeapon(WeaponData weaponData) => GetWeapon(weaponData) != null;

    // True once `weaponData` has been evolved into something else
    public bool HasEvolutionOf(WeaponData weaponData) => m_ActiveWeapons.Exists(w => w.data.evolvesFrom == weaponData);

    public void LevelUpWeapon(WeaponData weaponData) => GetWeapon(weaponData)?.LevelUp();

    public void Evolve(WeaponData evolved)
    {
        WeaponInstance weapon = GetWeapon(evolved.evolvesFrom);
        if (weapon != null) weapon.EvolveInto(evolved);
    }

    private void FireWeapon(WeaponInstance weapon)
    {
        Vector3 position = weapon.muzzle.position;
        switch (weapon.data.firingType)
        {
            case FiringType.Single:
            case FiringType.Automatic:
                FireSingle(position, weapon);
                break;
            case FiringType.Spread:
                FireSpread(position, weapon);
                break;
            case FiringType.Homing:
                FireHoming(position, weapon);
                break;
        }
    }

    public void FireMainWeapon(Vector3 position)
    {
        FireSingle(position, m_ActiveWeapons[0]);
    }

    private void FireSingle(Vector3 position, WeaponInstance weapon)
    {
        Vector3 direction = m_PlayerTransform.forward;
        Projectile proj = m_ProjectilePool.GetProjectile(weapon.data.projectilePrefab);
        proj.Initialize(position, direction, weapon);
        AudioManager.Instance.PlaySFX(weapon.data.shootSound, position, 0.5f);
    }

    // Pellets fanned evenly across 60 degrees
    private void FireSpread(Vector3 position, WeaponInstance weapon)
    {
        int count = Mathf.Max(1, weapon.count);
        float angleStep = count > 1 ? 60f / (count - 1) : 0f;
        float startAngle = -angleStep * (count - 1) / 2;

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + i * angleStep;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * m_PlayerTransform.forward;
            Projectile proj = m_ProjectilePool.GetProjectile(weapon.data.projectilePrefab);
            proj.Initialize(position, direction, weapon);
        }
        AudioManager.Instance.PlaySFX(weapon.data.shootSound, position, 0.5f);
    }

    // Missiles leave sideways and upward, then curve onto their targets
    private void FireHoming(Vector3 position, WeaponInstance weapon)
    {
        int count = Mathf.Max(1, weapon.count);
        for (int i = 0; i < count; i++)
        {
            float side = count > 1 ? Mathf.Lerp(-60f, 60f, i / (float)(count - 1)) : 0f;
            Vector3 direction = Quaternion.Euler(-30f, side, 0) * m_PlayerTransform.forward;
            HomingMissile.Launch(position, direction, weapon);
        }
        AudioManager.Instance.PlaySFX(weapon.data.shootSound, position, 0.5f);
    }

    // Weapon stat upgrades apply to every owned weapon
    public void ApplyUpgrade(UpgradeData upgrade)
    {
        foreach (var weapon in m_ActiveWeapons)
            weapon.ApplyUpgrade(upgrade);
    }
}
