using System.Collections.Generic;
using UnityEngine;

// Scatters rocks/trees (obstacles) and bushes (decoration) over the arena floor.
public static class ArenaProps
{
    private const string ResourcePath = "ArenaProps";
    private const string GroundName = "Ground";
    private const string ObstacleTag = "Obstacle";
    private const int AttemptsPerProp = 20;

    private static readonly List<Vector3> s_SolidPositions = new List<Vector3>();

    public static void Spawn(Vector3 playerStart)
    {
        s_SolidPositions.Clear();
        var set = Resources.Load<ArenaPropSet>(ResourcePath);
        GameObject ground = GameObject.Find(GroundName);
        if (set == null || ground == null || !TryGetBounds(ground, out Bounds bounds)) return;

        var root = new GameObject("ArenaProps").transform;
        Place(set, set.solidProps, set.solidCount, true, bounds, playerStart, root);
        Place(set, set.decorProps, set.decorCount, false, bounds, playerStart, root);
    }

    // True when a spot is clear of solid props (used to keep enemy spawns out of rocks)
    public static bool IsClear(Vector3 position, float radius)
    {
        foreach (Vector3 solid in s_SolidPositions)
        {
            Vector3 offset = solid - position;
            offset.y = 0;
            if (offset.sqrMagnitude < radius * radius) return false;
        }
        return true;
    }

    private static void Place(ArenaPropSet set, GameObject[] prefabs, int count, bool solid, Bounds bounds,
        Vector3 playerStart, Transform root)
    {
        if (prefabs == null || prefabs.Length == 0) return;
        float margin = set.edgeMargin;
        for (int i = 0; i < count; i++)
        {
            for (int attempt = 0; attempt < AttemptsPerProp; attempt++)
            {
                // Around the start position (the floor is far bigger than where fights happen)
                Vector2 offset = Random.insideUnitCircle * set.areaRadius;
                if (offset.magnitude < set.clearRadius) continue;
                var position = new Vector3(playerStart.x + offset.x, bounds.max.y, playerStart.z + offset.y);
                if (position.x < bounds.min.x + margin || position.x > bounds.max.x - margin ||
                    position.z < bounds.min.z + margin || position.z > bounds.max.z - margin) continue;
                if (solid && !IsClear(position, set.minSpacing)) continue;

                GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
                GameObject prop = Object.Instantiate(prefab, position, Quaternion.Euler(0, Random.Range(0f, 360f), 0), root);
                foreach (Collider collider in prop.GetComponentsInChildren<Collider>())
                {
                    // Bullets stop on objects tagged Obstacle; decoration doesn't block anything
                    if (solid) collider.gameObject.tag = ObstacleTag;
                    else collider.enabled = false;
                }
                if (solid) s_SolidPositions.Add(position);
                break;
            }
        }
    }

    private static bool TryGetBounds(GameObject ground, out Bounds bounds)
    {
        if (ground.TryGetComponent(out Collider collider)) { bounds = collider.bounds; return true; }
        if (ground.TryGetComponent(out Renderer renderer)) { bounds = renderer.bounds; return true; }
        bounds = default;
        return false;
    }
}
