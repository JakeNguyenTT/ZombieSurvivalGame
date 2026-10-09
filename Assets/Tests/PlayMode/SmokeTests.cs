using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using static PlayModeHelpers;
using Object = UnityEngine.Object;

// Plays through menu -> game -> boss -> game over -> menu. Any error or exception logged
// along the way fails the test.
public class SmokeTests
{
    private readonly PrefsSnapshot m_Prefs = new PrefsSnapshot();

    [SetUp]
    public void SnapshotPrefs() => m_Prefs.Capture();

    [TearDown]
    public void RestorePrefs()
    {
        Time.timeScale = 1f;
        m_Prefs.Restore();
    }

    [UnityTest, Timeout(180000)]
    public IEnumerator FullRun_HasNoErrors()
    {
        yield return LoadScene("MenuScene");
        yield return WaitRealtime(1f);
        yield return Screenshot("01_menu");

        Component menuMeta = FindByTypeName("MenuMeta");
        Assert.IsNotNull(menuMeta, "MenuMeta was not created");
        foreach (var panel in new[] { "BuildShop", "BuildCharacters", "BuildSettings" })
        {
            Invoke(menuMeta, panel);
            yield return null;
            yield return Screenshot("02_" + panel);
            Invoke(menuMeta, "ClosePanel");
            yield return null;
        }

        yield return LoadScene("GameScene");
        yield return WaitRealtime(1f);
        Component game = FindByTypeName("GameManager");
        Assert.IsNotNull(game, "GameManager missing");

        Assert.IsNotNull(FindByTypeName("EnemyNavigation"), "Enemy navigation was not created");
        Assert.IsNotNull(FindByTypeName("ScreenEffects"), "Screen effects were not created");

        GameObject props = GameObject.Find("ArenaProps");
        Assert.IsNotNull(props, "Arena props were not spawned");
        Assert.That(props.transform.childCount, Is.GreaterThan(20), "Too few arena props placed");

        // The test player never moves, so keep them alive: a long post-hit invulnerability window
        Component player = FindByTypeName("PlayerManager");
        player.GetType().GetField("m_InvisibleTimer", AnyInstance).SetValue(player, 9999f);

        // The player stands still, so grant experience directly to exercise level-ups
        Component experience = FindByTypeName("ExperienceManager");
        Invoke(experience, "AddExperience", 150f);
        Assert.IsFalse((bool)GetProperty(game, "IsPlaying"), "Level-up should pause the game");

        // ~20 s of game time, picking upgrades whenever a level-up pauses the game
        Time.timeScale = 5f;
        bool rerolled = false;
        int upgradesPicked = 0;
        float end = Time.realtimeSinceStartup + 4f;
        while (Time.realtimeSinceStartup < end && !IsGameOver(game))
        {
            if (!(bool)GetProperty(game, "IsPlaying"))
            {
                Component upgradePanel = FindByTypeName("UpgradePanel");
                if (upgradePanel != null && upgradePanel.gameObject.activeInHierarchy)
                {
                    if (!rerolled)
                    {
                        yield return Screenshot("03_levelup");
                        Invoke(upgradePanel, "OnButtonReroll");
                        rerolled = true;
                    }
                    PickFirstUpgrade();
                    upgradesPicked++;
                    Time.timeScale = 5f;
                    if (upgradesPicked < 3) Invoke(experience, "AddExperience", 150f);
                }
            }
            yield return null;
        }
        Assert.That(upgradesPicked, Is.GreaterThanOrEqualTo(1), "No upgrade was picked");
        yield return Screenshot("04_gameplay");

        // Every weapon card to max level, then every evolution, then let them fight a while
        Assert.IsFalse(IsGameOver(game), "Player died despite invulnerability");
        {
            Component upgradeManager = FindByTypeName("UpgradeManager");
            var upgrades = (IList)upgradeManager.GetType().GetField("m_AvailableUpgrades", AnyInstance).GetValue(upgradeManager);
            foreach (object upgrade in upgrades)
                if (UpgradeTypeName(upgrade) == "AddWeapon")
                    for (int i = 0; i < 5; i++) Invoke(upgradeManager, "ApplyUpgrade", upgrade);
            foreach (object upgrade in upgrades)
                if (UpgradeTypeName(upgrade) == "Evolve")
                    Invoke(upgradeManager, "ApplyUpgrade", upgrade);

            var weapons = (ICollection)GetProperty(FindByTypeName("WeaponSystem"), "Weapons");
            Assert.That(weapons.Count, Is.GreaterThanOrEqualTo(5), "Weapon cards did not add weapons");
            Time.timeScale = 3f;
            yield return WaitRealtime(2f);
            yield return Screenshot("04b_weapons");
        }

        Invoke(game, "SkipToBoss");
        Time.timeScale = 1f;
        yield return WaitRealtime(1f);
        Assert.IsNotNull(GetProperty(FindByTypeName("EnemySpawner"), "ActiveBoss"), "Boss did not spawn");
        yield return Screenshot("05_boss");

        Invoke(game, "GameOver");
        yield return null;
        yield return null;
        yield return Screenshot("06_gameover");
        Assert.IsTrue(IsGameOver(game), "GameOver did not end the run");
        Assert.AreEqual(0f, Time.timeScale, "Game over should pause the game");

        Time.timeScale = 1f;
        yield return LoadScene("MenuScene");
        yield return WaitRealtime(0.5f);
    }

    // Regression: gems used to be checked against 0.1 units before moving, so a player walking at
    // 30 fps (0.17 units per frame) dragged gems along forever without collecting them
    [UnityTest, Timeout(60000)]
    public IEnumerator WalkingPlayer_CollectsGems_At30Fps()
    {
        yield return LoadScene("GameScene");
        Component player = FindByTypeName("PlayerManager");
        Component experience = FindByTypeName("ExperienceManager");
        player.GetType().GetField("m_InvisibleTimer", AnyInstance).SetValue(player, 9999f);
        Invoke(FindByTypeName("EnemySpawner"), "StopSpawning");

        Time.captureDeltaTime = 1f / 30f;
        try
        {
            Vector3 walk = Vector3.right * 5f / 30f; // 5 units/s
            Invoke(FindByTypeName("ExpSpawner"), "SpawnExp", player.transform.position + Vector3.right * 1.5f);
            float before = ExperienceOf(experience);
            for (int frame = 0; frame < 60; frame++)
            {
                player.transform.position += walk;
                yield return null;
            }
            Assert.That(ExperienceOf(experience), Is.GreaterThan(before), "Gem was not collected while walking");
        }
        finally
        {
            Time.captureDeltaTime = 0f;
        }
    }

    private static float ExperienceOf(Component experience) =>
        (float)GetField(experience, "m_CurrentExp") + 1000f * (int)GetProperty(experience, "CurrentLevel");

    private static string UpgradeTypeName(object upgrade) =>
        upgrade.GetType().GetField("type").GetValue(upgrade).ToString();

    private static void PickFirstUpgrade()
    {
        Component upgradeManager = FindByTypeName("UpgradeManager");
        Component uiManager = FindByTypeName("UIManager");
        var options = (Array)Invoke(upgradeManager, "GetUpgradeOptions", 1);
        Assert.That(options.Length, Is.GreaterThan(0), "No upgrade options offered");
        Invoke(uiManager, "SelectUpgrade", options.GetValue(0));
    }

    // Only when run from the editor's Test Runner window: in batch mode WaitForEndOfFrame never
    // resumes, which would hang the test
    private static IEnumerator Screenshot(string name)
    {
        if (Application.isBatchMode) yield break;
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) yield break;
        yield return new WaitForEndOfFrame();
        Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture();
        if (texture == null) yield break;
        string folder = Path.Combine(Application.dataPath, "..", "Temp", "Screenshots");
        Directory.CreateDirectory(folder);
        File.WriteAllBytes(Path.Combine(folder, name + ".png"), texture.EncodeToPNG());
        Object.Destroy(texture);
    }
}
