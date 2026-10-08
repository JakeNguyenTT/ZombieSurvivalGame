public static class RunRewards
{
    public const int CoinsPerBoss = 25;

    public static int Coins(int kills, float seconds, int bosses)
    {
        return kills / 2 + (int)(seconds / 10f) + bosses * CoinsPerBoss;
    }
}
