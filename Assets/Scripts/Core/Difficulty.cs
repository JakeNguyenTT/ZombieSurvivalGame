using UnityEngine;

// Difficulty curve as a function of elapsed run time (seconds).
// Tuned with the balance bot (BalanceSimulation): see docs/superpowers/plans/2026-10-09-round-4.md.
public static class Difficulty
{
    public const float MinSpawnInterval = 0.25f;
    public const float FirstBossTime = 120f;
    public const float BossInterval = 150f;
    public const float BossHealthPerLevel = 6f; // times the base type's health

    // 1.2 s at the start, 0.6 s at 4:00, 0.4 s at 8:00, floor 0.25 s
    public static float SpawnInterval(float time) => Mathf.Max(MinSpawnInterval, 1.2f / (1f + time / 240f));

    // Enemies spawned per spawn tick: one more every four minutes
    public static int SpawnBatch(float time) => 1 + Mathf.FloorToInt(time / 240f);

    public static float HealthMultiplier(float time) => 1f + 0.10f * time / 60f;

    public static float DamageMultiplier(float time) => 1f + 0.03f * time / 60f;

    // 0 before the first boss, then 1, 2, ... for each boss that should have spawned by now
    public static int BossLevelAt(float time)
    {
        if (time < FirstBossTime) return 0;
        return 1 + Mathf.FloorToInt((time - FirstBossTime) / BossInterval);
    }

    public static float BossHealth(float baseHealth, int bossLevel, float healthMultiplier) =>
        baseHealth * BossHealthPerLevel * bossLevel * healthMultiplier;

    // Contact damage: 1.5x base at level 1, +0.5x per level after
    public static float BossDamage(float baseDamage, int bossLevel) => baseDamage * (1f + 0.5f * bossLevel);
}
