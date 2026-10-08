using UnityEngine;

public enum EnemyArchetype
{
    Walker,   // walks straight at the player
    Runner,   // same as walker, tuned fast and fragile
    Tank,     // same as walker, tuned slow and tough
    Spitter,  // keeps distance and shoots
    Exploder, // runs in and explodes
}

[CreateAssetMenu(fileName = "EnemyData", menuName = "Game/EnemyData")]
public class EnemyData : ScriptableObject
{
    public EnemyBehavior prefab;
    public EnemyArchetype archetype = EnemyArchetype.Walker;
    public float speed = 2f;
    public float health = 100f;
    public float damage = 5f;
    public AudioClip hurtSound;
    public AudioClip deathSound;

    [Header("Look")]
    public Color tint = Color.white;
    public float scale = 1f;

    [Header("Spawning")]
    public float spawnWeight = 1f;
    public float unlockTime = 0f; // seconds into the run before this type can spawn
    public int expDrops = 1;

    [Header("Spitter")]
    public float attackRange = 9f;
    public float attackCooldown = 2.5f;
    public float projectileSpeed = 8f;

    [Header("Exploder")]
    public float explodeRadius = 2.5f;
    public float fuseTime = 0.5f;
}
