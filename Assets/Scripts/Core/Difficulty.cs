using UnityEngine;

// Difficulty curve as a function of elapsed run time (seconds).
public static class Difficulty
{
    public const float MinSpawnInterval = 0.15f;
    public const float FirstBossTime = 90f;
    public const float BossInterval = 120f;

    public static float SpawnInterval(float time) => Mathf.Max(MinSpawnInterval, 1f / (1f + time / 60f));

    // Enemies spawned per spawn tick: one more every two minutes
    public static int SpawnBatch(float time) => 1 + Mathf.FloorToInt(time / 120f);

    public static float HealthMultiplier(float time) => 1f + 0.25f * time / 60f;

    public static float DamageMultiplier(float time) => 1f + 0.1f * time / 60f;

    // 0 before the first boss, then 1, 2, ... for each boss that should have spawned by now
    public static int BossLevelAt(float time)
    {
        if (time < FirstBossTime) return 0;
        return 1 + Mathf.FloorToInt((time - FirstBossTime) / BossInterval);
    }
}
