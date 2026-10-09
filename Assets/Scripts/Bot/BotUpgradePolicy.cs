// How the balance bot ranks level-up choices. Deliberately simple and greedy: it should play
// like a reasonable first-time player, not an optimal one.
public static class BotUpgradePolicy
{
    public static UpgradeData Choose(UpgradeData[] options, float healthFraction)
    {
        UpgradeData best = null;
        float bestScore = float.MinValue;
        foreach (UpgradeData option in options)
        {
            if (option == null) continue;
            float score = Score(option, healthFraction);
            if (score > bestScore)
            {
                bestScore = score;
                best = option;
            }
        }
        return best;
    }

    private static float Score(UpgradeData upgrade, float healthFraction)
    {
        switch (upgrade.type)
        {
            case UpgradeType.Evolve: return 100f;
            case UpgradeType.Heal: return healthFraction < 0.5f ? 90f : 5f;
            case UpgradeType.AddWeapon: return 80f;
            case UpgradeType.Damage: return 60f;
            case UpgradeType.FireRate: return 55f;
            case UpgradeType.MaxHealth: return 50f;
            case UpgradeType.Penetration: return 40f;
            case UpgradeType.Magnet: return 35f;
            case UpgradeType.Speed: return 30f;
            case UpgradeType.ProjectileSpeed: return 20f;
            default: return 10f;
        }
    }
}
