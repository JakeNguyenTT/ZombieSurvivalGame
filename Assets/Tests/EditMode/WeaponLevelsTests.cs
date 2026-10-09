using NUnit.Framework;

public class WeaponLevelsTests
{
    [Test]
    public void DamageMultiplier_IsOneAtLevelOne_AndAddsTwentyPercentPerLevel()
    {
        Assert.AreEqual(1f, WeaponLevels.DamageMultiplier(1), 1e-4f);
        Assert.AreEqual(1.2f, WeaponLevels.DamageMultiplier(2), 1e-4f);
        Assert.AreEqual(1.8f, WeaponLevels.DamageMultiplier(5), 1e-4f);
    }

    [Test]
    public void DamageMultiplier_ClampsOutOfRangeLevels()
    {
        Assert.AreEqual(1f, WeaponLevels.DamageMultiplier(0), 1e-4f);
        Assert.AreEqual(1.8f, WeaponLevels.DamageMultiplier(9), 1e-4f);
    }

    [TestCase(1, 0)]
    [TestCase(2, 0)]
    [TestCase(3, 1)]
    [TestCase(4, 1)]
    [TestCase(5, 2)]
    public void ExtraCount_StepsAtLevelsThreeAndFive(int level, int expected)
    {
        Assert.AreEqual(expected, WeaponLevels.ExtraCount(level));
    }

    [Test]
    public void RadiusMultiplier_AddsFifteenPercentPerLevel()
    {
        Assert.AreEqual(1f, WeaponLevels.RadiusMultiplier(1), 1e-4f);
        Assert.AreEqual(1.6f, WeaponLevels.RadiusMultiplier(5), 1e-4f);
    }
}
