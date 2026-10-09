// How a weapon grows as it levels up from 1 to MaxLevel.
public static class WeaponLevels
{
    public const int MaxLevel = 5;

    // +20% damage per level after the first
    public static float DamageMultiplier(int level) => 1f + 0.2f * (Clamp(level) - 1);

    // One extra blade/missile at level 3 and another at level 5
    public static int ExtraCount(int level) => (level >= 3 ? 1 : 0) + (level >= 5 ? 1 : 0);

    // +15% area per level after the first (aura)
    public static float RadiusMultiplier(int level) => 1f + 0.15f * (Clamp(level) - 1);

    private static int Clamp(int level) => level < 1 ? 1 : (level > MaxLevel ? MaxLevel : level);
}
