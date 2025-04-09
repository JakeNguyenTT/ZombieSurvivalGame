using UnityEngine;
using System.Collections.Generic;

public class WeaponSystem : MonoBehaviour
{
    [SerializeField] private ProjectilePool m_ProjectilePool;
    private List<WeaponInstance> m_ActiveWeapons = new List<WeaponInstance>();
    private Transform m_PlayerTransform;

    public void Initialize(WeaponData startingWeapon, Transform playerTransform)
    {
        m_PlayerTransform = playerTransform;
        AddWeapon(startingWeapon);
    }

    public void Tick(float deltaTime)
    {
        foreach (var weapon in m_ActiveWeapons)
        {
            weapon.timer -= deltaTime;
            if (weapon.timer <= 0)
            {
                FireWeapon(weapon.data);
                weapon.timer = weapon.data.fireRate;
            }
        }
    }

    public void AddWeapon(WeaponData weaponData)
    {
        m_ActiveWeapons.Add(new WeaponInstance { data = weaponData, timer = weaponData.fireRate });
    }

    private void FireWeapon(WeaponData weapon)
    {
        Vector3 position = m_PlayerTransform.position + Vector3.up * 1f; // Fire from slightly above ground
        switch (weapon.firingType)
        {
            case FiringType.Single:
                FireSingle(position, weapon);
                break;
            case FiringType.Spread:
                FireSpread(position, weapon);
                break;
        }
    }

    private void FireSingle(Vector3 position, WeaponData weapon)
    {
        Vector3 direction = m_PlayerTransform.forward;
        Projectile proj = m_ProjectilePool.GetProjectile(weapon.projectilePrefab);
        proj.Initialize(position, direction, weapon.damage);
        AudioManager.Instance.PlaySFX("Shoot");
    }

    private void FireSpread(Vector3 position, WeaponData weapon)
    {
        int count = 3;
        float angleStep = 30f;
        float startAngle = -angleStep * (count - 1) / 2;

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + i * angleStep;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * m_PlayerTransform.forward;
            Projectile proj = m_ProjectilePool.GetProjectile(weapon.projectilePrefab);
            proj.Initialize(position, direction, weapon.damage);
        }
        AudioManager.Instance.PlaySFX("Shoot");
    }
}

[System.Serializable]
public class WeaponInstance
{
    public WeaponData data;
    public float timer;
}