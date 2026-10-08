// Permanent upgrades bought with coins between runs.
public enum MetaStat
{
    MaxHealth,
    Damage,
    MoveSpeed,
    PickupRange,
}

public static class MetaUpgrades
{
    public const int MaxLevel = 5;

    // Price of buying the next level when currently at `level`
    public static int Cost(int level) => 50 * (level + 1);

    public static float Bonus(MetaStat stat, int level)
    {
        switch (stat)
        {
            case MetaStat.MaxHealth: return 10f * level;
            case MetaStat.Damage: return 0.1f * level;      // damage multiplier bonus
            case MetaStat.MoveSpeed: return 0.3f * level;
            case MetaStat.PickupRange: return 0.5f * level;
            default: return 0f;
        }
    }

    public static string Label(MetaStat stat)
    {
        switch (stat)
        {
            case MetaStat.MaxHealth: return "Max Health";
            case MetaStat.Damage: return "Damage";
            case MetaStat.MoveSpeed: return "Move Speed";
            case MetaStat.PickupRange: return "Pickup Range";
            default: return stat.ToString();
        }
    }

    public static string FormatBonus(MetaStat stat, int level)
    {
        float bonus = Bonus(stat, level);
        return stat == MetaStat.Damage ? $"+{bonus * 100f:0}%" : $"+{bonus:0.#}";
    }
}
