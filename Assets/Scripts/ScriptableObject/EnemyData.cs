using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Game/EnemyData")]
public class EnemyData : ScriptableObject
{
    public EnemyBehavior prefab;
    public float speed = 2f;
    public float health = 100f;
    public float damage = 5f;
    public AudioClip hurtSound;
    public AudioClip deathSound;
}