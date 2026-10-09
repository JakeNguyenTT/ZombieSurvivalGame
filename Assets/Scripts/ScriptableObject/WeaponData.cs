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
    public AudioClip shootSound;

    [Header("Multi / area weapons")]
    public int count = 1;              // pellets (Spread), blades (Orbit) or missiles (Homing)
    public float radius = 2.5f;        // orbit radius, aura radius or missile blast radius
    public float rotationSpeed = 180f; // Orbit: degrees per second
    public float hitInterval = 0.5f;   // Orbit: re-hit delay per enemy. Aura: time between damage ticks
    public bool pierceAll;             // projectiles never stop on enemies
    public float healPerTick;          // Aura: heal the player when a tick hits something
    public Color color = Color.white;  // tint for code-built visuals
    public ParticleSystem hitEffect;   // Homing: explosion

    [Header("Evolution (set on the evolved weapon)")]
    public WeaponData evolvesFrom;
    public UpgradeType requiredPassive;
}

public enum FiringType { Single, Spread, Automatic, Orbit, Homing, Aura }
