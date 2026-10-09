using UnityEngine;

public static class AreaDamage
{
    // Damages every active enemy within `radius` (flat distance, enemy size included).
    // Returns how many were hit.
    public static int Apply(Vector3 center, float radius, float damage)
    {
        var enemies = EnemySpawner.Instance.ActiveEnemies;
        int hits = 0;
        // Backwards: a kill removes that enemy from the list, which only shifts later entries
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            if (i >= enemies.Count) continue;
            EnemyBehavior enemy = enemies[i];
            Vector3 offset = enemy.transform.position - center;
            offset.y = 0;
            float reach = radius + 0.5f * enemy.transform.localScale.x;
            if (offset.sqrMagnitude > reach * reach) continue;
            enemy.TakeDamage(damage, offset);
            hits++;
        }
        return hits;
    }
}
