using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Game/WeaponData")]
public class WeaponData : ScriptableObject
{
    public FiringType firingType;
    public Projectile projectilePrefab;
    public float fireRate = 0.5f;
    public float damage = 10f;
}

public enum FiringType { Single, Spread }