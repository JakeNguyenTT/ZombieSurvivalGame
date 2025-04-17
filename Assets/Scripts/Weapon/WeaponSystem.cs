using UnityEngine;
using System.Collections.Generic;

public class WeaponSystem : MonoBehaviour
{
    [SerializeField] private ProjectilePool m_ProjectilePool;
    [Header("Read Only")]
    [SerializeField] private List<WeaponInstance> m_ActiveWeapons = new List<WeaponInstance>();
    [SerializeField] private Transform m_PlayerTransform;

    public void Initialize(WeaponData startingWeapon, Transform playerTransform)
    {
        m_PlayerTransform = playerTransform;
        AddWeapon(startingWeapon, m_PlayerTransform.GetComponent<PlayerManager>().GunMuzzle);
    }

    public void Tick(float deltaTime)
    {
        foreach (var weapon in m_ActiveWeapons)
        {
            weapon.timer -= deltaTime;
            if (weapon.timer <= 0)
            {
                FireWeapon(weapon);
                weapon.timer = weapon.data.fireRate;
            }
        }
    }

    public void AddWeapon(WeaponData weaponData)
    {
        AddWeapon(weaponData, m_PlayerTransform);
    }

    public void AddWeapon(WeaponData weaponData, Transform muzzle)
    {
        WeaponInstance weapon = new WeaponInstance();
        weapon.Initialize(weaponData, muzzle);
        m_ActiveWeapons.Add(weapon);
    }

    private void FireWeapon(WeaponInstance weapon)
    {
        Vector3 position = weapon.muzzle.position;
        switch (weapon.data.firingType)
        {
            case FiringType.Single:
                FireSingle(position, weapon);
                break;
            case FiringType.Spread:
                FireSpread(position, weapon);
                break;
        }
    }

    public void FireMainWeapon(Vector3 position)
    {
        Debug.Log("FireMainWeapon");
        FireSingle(position, m_ActiveWeapons[0]);
    }

    private void FireSingle(Vector3 position, WeaponInstance weapon)
    {
        Vector3 direction = m_PlayerTransform.forward;
        Projectile proj = m_ProjectilePool.GetProjectile(weapon.data.projectilePrefab);
        proj.Initialize(position, direction, weapon);
        // AudioManager.Instance.PlaySFX(AudioID.Shoot, position);
    }

    private void FireSpread(Vector3 position, WeaponInstance weapon)
    {
        int count = 3;
        float angleStep = 30f;
        float startAngle = -angleStep * (count - 1) / 2;

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + i * angleStep;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * m_PlayerTransform.forward;
            Projectile proj = m_ProjectilePool.GetProjectile(weapon.data.projectilePrefab);
            proj.Initialize(position, direction, weapon);
        }
    }

    public void ApplyUpgrade(UpgradeData upgrade)
    {
        m_ActiveWeapons[0].ApplyUpgrade(upgrade);
    }
}
