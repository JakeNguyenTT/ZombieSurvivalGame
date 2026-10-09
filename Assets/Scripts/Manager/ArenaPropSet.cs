using UnityEngine;

// Props scattered over the arena at the start of each run (loaded from Resources/ArenaProps).
[CreateAssetMenu(fileName = "ArenaProps", menuName = "Game/ArenaPropSet")]
public class ArenaPropSet : ScriptableObject
{
    [Tooltip("Block movement and bullets")]
    public GameObject[] solidProps;
    [Tooltip("Decoration only; their colliders are turned off")]
    public GameObject[] decorProps;
    public int solidCount = 80;
    public int decorCount = 100;
    [Tooltip("Props are placed within this distance of the player's start position")]
    public float areaRadius = 100f;
    [Tooltip("Kept empty around the player's start position")]
    public float clearRadius = 6f;
    [Tooltip("Minimum distance between solid props")]
    public float minSpacing = 4f;
    [Tooltip("Kept empty along the arena edge")]
    public float edgeMargin = 2f;
}
