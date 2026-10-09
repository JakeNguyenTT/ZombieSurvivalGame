using System.Collections.Generic;
using NUnit.Framework;

public class DifficultyTests
{
    [Test]
    public void SpawnInterval_StartsSlow_AndHalvesByMinuteFour()
    {
        Assert.AreEqual(1.2f, Difficulty.SpawnInterval(0f), 1e-4f);
        Assert.AreEqual(0.6f, Difficulty.SpawnInterval(240f), 1e-4f);
    }

    [Test]
    public void BossDamage_GrowsByHalfBasePerLevel()
    {
        Assert.AreEqual(15f, Difficulty.BossDamage(10f, 1), 1e-4f);
        Assert.AreEqual(25f, Difficulty.BossDamage(10f, 3), 1e-4f);
    }

    [Test]
    public void SpawnInterval_IsClampedToMinimum()
    {
        Assert.AreEqual(0.25f, Difficulty.SpawnInterval(3600f), 1e-4f);
    }

    [Test]
    public void SpawnBatch_GrowsEveryFourMinutes()
    {
        Assert.AreEqual(1, Difficulty.SpawnBatch(0f));
        Assert.AreEqual(1, Difficulty.SpawnBatch(239f));
        Assert.AreEqual(2, Difficulty.SpawnBatch(240f));
        Assert.AreEqual(3, Difficulty.SpawnBatch(500f));
    }

    [Test]
    public void Multipliers_StartAtOne_AndGrowLinearly()
    {
        Assert.AreEqual(1f, Difficulty.HealthMultiplier(0f), 1e-4f);
        Assert.AreEqual(1.1f, Difficulty.HealthMultiplier(60f), 1e-4f);
        Assert.AreEqual(1f, Difficulty.DamageMultiplier(0f), 1e-4f);
        Assert.AreEqual(1.03f, Difficulty.DamageMultiplier(60f), 1e-4f);
    }

    [Test]
    public void BossLevel_IsZeroBeforeFirstBoss_ThenCountsIntervals()
    {
        Assert.AreEqual(0, Difficulty.BossLevelAt(0f));
        Assert.AreEqual(0, Difficulty.BossLevelAt(119.9f));
        Assert.AreEqual(1, Difficulty.BossLevelAt(120f));
        Assert.AreEqual(1, Difficulty.BossLevelAt(269.9f));
        Assert.AreEqual(2, Difficulty.BossLevelAt(270f));
    }

    [Test]
    public void BossHealth_ScalesWithLevelAndTime()
    {
        Assert.AreEqual(600f, Difficulty.BossHealth(100f, 1, 1f), 1e-3f);
        Assert.AreEqual(1800f, Difficulty.BossHealth(100f, 2, 1.5f), 1e-3f);
    }
}

public class WeightedRandomTests
{
    [Test]
    public void Pick_UsesCumulativeWeights()
    {
        var weights = new List<float> { 1f, 3f };
        Assert.AreEqual(0, WeightedRandom.Pick(weights, 0f));
        Assert.AreEqual(0, WeightedRandom.Pick(weights, 0.24f));
        Assert.AreEqual(1, WeightedRandom.Pick(weights, 0.26f));
        Assert.AreEqual(1, WeightedRandom.Pick(weights, 0.9999f));
    }

    [Test]
    public void Pick_ReturnsMinusOne_WhenNoWeight()
    {
        Assert.AreEqual(-1, WeightedRandom.Pick(new List<float> { 0f, 0f }, 0.5f));
        Assert.AreEqual(-1, WeightedRandom.Pick(new List<float>(), 0.5f));
    }

    [Test]
    public void Pick_SkipsZeroWeights()
    {
        var weights = new List<float> { 0f, 2f, 0f };
        Assert.AreEqual(1, WeightedRandom.Pick(weights, 0f));
        Assert.AreEqual(1, WeightedRandom.Pick(weights, 0.9999f));
    }

    [Test]
    public void PickIndices_ReturnsDistinctIndices_AndSkipsZeroWeights()
    {
        var weights = new List<float> { 1f, 0f, 1f, 1f };
        var rolls = new Queue<float>(new[] { 0f, 0f, 0f, 0f });
        List<int> picked = WeightedRandom.PickIndices(weights, 5, () => rolls.Count > 0 ? rolls.Dequeue() : 0f);
        CollectionAssert.AreEquivalent(new[] { 0, 2, 3 }, picked);
    }

    [Test]
    public void PickIndices_RespectsCount()
    {
        var weights = new List<float> { 1f, 1f, 1f, 1f };
        List<int> picked = WeightedRandom.PickIndices(weights, 2, () => 0.5f);
        Assert.AreEqual(2, picked.Count);
        Assert.AreNotEqual(picked[0], picked[1]);
    }
}

public class MetaUpgradesTests
{
    [Test]
    public void Cost_GrowsPerLevel()
    {
        Assert.AreEqual(50, MetaUpgrades.Cost(0));
        Assert.AreEqual(250, MetaUpgrades.Cost(4));
    }

    [Test]
    public void Bonus_IsZeroAtLevelZero_AndScalesWithLevel()
    {
        Assert.AreEqual(0f, MetaUpgrades.Bonus(MetaStat.MaxHealth, 0), 1e-4f);
        Assert.AreEqual(50f, MetaUpgrades.Bonus(MetaStat.MaxHealth, 5), 1e-4f);
        Assert.AreEqual(0.5f, MetaUpgrades.Bonus(MetaStat.Damage, 5), 1e-4f);
        Assert.AreEqual(1.5f, MetaUpgrades.Bonus(MetaStat.MoveSpeed, 5), 1e-4f);
        Assert.AreEqual(2.5f, MetaUpgrades.Bonus(MetaStat.PickupRange, 5), 1e-4f);
    }

    [Test]
    public void EveryStat_HasALabel()
    {
        foreach (MetaStat stat in System.Enum.GetValues(typeof(MetaStat)))
            Assert.IsFalse(string.IsNullOrEmpty(MetaUpgrades.Label(stat)));
    }
}

public class AdaptiveScaleTests
{
    [Test]
    public void SlowFrames_LowerScale_DownToMinimum()
    {
        Assert.AreEqual(0.7f, AdaptiveScale.Next(0.8f, 1f / 30f, 0.8f), 1e-4f);
        Assert.AreEqual(0.6f, AdaptiveScale.Next(0.6f, 1f / 30f, 0.8f), 1e-4f);
    }

    [Test]
    public void FastFrames_RaiseScale_UpToAssetValue()
    {
        Assert.AreEqual(0.8f, AdaptiveScale.Next(0.7f, 1f / 60f, 0.8f), 1e-4f);
        Assert.AreEqual(0.8f, AdaptiveScale.Next(0.8f, 1f / 60f, 0.8f), 1e-4f);
    }

    [Test]
    public void InBetween_KeepsScale()
    {
        Assert.AreEqual(0.7f, AdaptiveScale.Next(0.7f, 1f / 54f, 0.8f), 1e-4f);
    }
}

public class TextUtilTests
{
    [TestCase("ProjectileSpeed", "Projectile Speed")]
    [TestCase("MaxHP", "Max HP")]
    [TestCase("FireRate", "Fire Rate")]
    [TestCase("Heal", "Heal")]
    [TestCase("HPRegen", "HP Regen")]
    [TestCase("", "")]
    public void Nicify_SplitsWords(string input, string expected)
    {
        Assert.AreEqual(expected, TextUtil.Nicify(input));
    }
}

public class RunRewardsTests
{
    [Test]
    public void Coins_CombinesKillsTimeAndBosses()
    {
        Assert.AreEqual(0, RunRewards.Coins(0, 0f, 0));
        Assert.AreEqual(10 + 12 + 25, RunRewards.Coins(21, 125f, 1));
    }
}
