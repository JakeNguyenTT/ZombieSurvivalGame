using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Game/EnemyData")]
public class EnemyData : ScriptableObject
{
    public EnemyBehavior prefab;
    public float speed = 2f;
    public float health = 20f;
    public float damage = 5f;
}