using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Game/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string weaponName = "Weapon";
    public Transform gunPrefab;
    public FiringType firingType;
    public Projectile projectilePrefab;
    public float fireRate = 0.5f;
    public float damage = 10f;
    public float range = 50f;
    public int maxAmmo = 10;
    public float projectileSpeed = 10f;
    public int penetration = 0;
}

public enum FiringType { Single, Spread, Automatic }