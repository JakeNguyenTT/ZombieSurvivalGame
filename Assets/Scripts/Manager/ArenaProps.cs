using System.Collections.Generic;
using UnityEngine;

// Scatters rocks/trees (obstacles) and bushes (decoration) over the arena floor.
public static class ArenaProps
{
    private const string ResourcePath = "ArenaProps";
    private const string GroundName = "Ground";
    private const string ObstacleTag = "Obstacle";
    private const int AttemptsPerProp = 20;

    private static readonly List<ArenaObstacle> s_Obstacles = new List<ArenaObstacle>();

    // Solid props placed this run, as flat circles (used for enemy pathing and spawn checks)
    public static IReadOnlyList<ArenaObstacle> Obstacles => s_Obstacles;

    public static void Spawn(Vector3 playerStart)
    {
        s_Obstacles.Clear();
        var set = Resources.Load<ArenaPropSet>(ResourcePath);
        GameObject ground = GameObject.Find(GroundName);
        if (set == null || ground == null || !TryGetBounds(ground, out Bounds bounds)) return;

        var root = new GameObject("ArenaProps").transform;
        Place(set, set.solidProps, set.solidCount, true, bounds, playerStart, root);
        Place(set, set.decorProps, set.decorCount, false, bounds, playerStart, root);
    }

    // True when a circle of `radius` at `position` doesn't overlap any solid prop
    public static bool IsClear(Vector3 position, float radius)
    {
        var point = new Vector2(position.x, position.z);
        foreach (ArenaObstacle obstacle in s_Obstacles)
        {
            float reach = radius + obstacle.Radius;
            if ((obstacle.Position - point).sqrMagnitude < reach * reach) return false;
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
                if (solid)
                    s_Obstacles.Add(new ArenaObstacle(new Vector2(position.x, position.z), FootprintRadius(prop)));
                break;
            }
        }
    }

    // Half the larger horizontal size of what actually blocks: colliders (a tree's trunk, not
    // its canopy), falling back to renderers
    private static float FootprintRadius(GameObject prop)
    {
        Physics.SyncTransforms(); // collider bounds of a just-instantiated prop
        if (TryEncapsulate(prop.GetComponentsInChildren<Collider>(), c => c.bounds, out Bounds bounds) ||
            TryEncapsulate(prop.GetComponentsInChildren<Renderer>(), r => r.bounds, out bounds))
            return Mathf.Max(bounds.extents.x, bounds.extents.z);
        return 0.5f;
    }

    private static bool TryEncapsulate<T>(T[] parts, System.Func<T, Bounds> getBounds, out Bounds bounds)
    {
        bounds = default;
        bool any = false;
        foreach (T part in parts)
        {
            Bounds partBounds = getBounds(part);
            if (partBounds.size == Vector3.zero) continue;
            if (any) bounds.Encapsulate(partBounds);
            else bounds = partBounds;
            any = true;
        }
        return any;
    }

    private static bool TryGetBounds(GameObject ground, out Bounds bounds)
    {
        if (ground.TryGetComponent(out Collider collider)) { bounds = collider.bounds; return true; }
        if (ground.TryGetComponent(out Renderer renderer)) { bounds = renderer.bounds; return true; }
        bounds = default;
        return false;
    }
}

public readonly struct ArenaObstacle
{
    public readonly Vector2 Position; // world XZ
    public readonly float Radius;

    public ArenaObstacle(Vector2 position, float radius)
    {
        Position = position;
        Radius = radius;
    }
}
