using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

// Plays through menu -> game -> boss -> game over -> menu. Any error or exception logged
// along the way fails the test. Game types live in Assembly-CSharp, which test assemblies
// can't reference, so they are driven by name through reflection.
public class SmokeTests
{
    private const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private static readonly string[] IntKeys = { "coins", "best_kills", "meta_MaxHealth", "meta_Damage", "meta_MoveSpeed", "meta_PickupRange" };
    private static readonly string[] FloatKeys = { "best_time", "music_volume", "sfx_volume" };
    private static readonly string[] StringKeys = { "character" };

    private readonly Dictionary<string, object> m_SavedPrefs = new Dictionary<string, object>();

    [SetUp]
    public void SnapshotPrefs()
    {
        m_SavedPrefs.Clear();
        foreach (var key in IntKeys) if (PlayerPrefs.HasKey(key)) m_SavedPrefs[key] = PlayerPrefs.GetInt(key);
        foreach (var key in FloatKeys) if (PlayerPrefs.HasKey(key)) m_SavedPrefs[key] = PlayerPrefs.GetFloat(key);
        foreach (var key in StringKeys) if (PlayerPrefs.HasKey(key)) m_SavedPrefs[key] = PlayerPrefs.GetString(key);
    }

    [TearDown]
    public void RestorePrefs()
    {
        Time.timeScale = 1f;
        foreach (var key in IntKeys) PlayerPrefs.DeleteKey(key);
        foreach (var key in FloatKeys) PlayerPrefs.DeleteKey(key);
        foreach (var key in StringKeys) PlayerPrefs.DeleteKey(key);
        foreach (var pair in m_SavedPrefs)
        {
            if (pair.Value is int i) PlayerPrefs.SetInt(pair.Key, i);
            else if (pair.Value is float f) PlayerPrefs.SetFloat(pair.Key, f);
            else PlayerPrefs.SetString(pair.Key, (string)pair.Value);
        }
        PlayerPrefs.Save();
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

        Invoke(game, "SkipToBoss");
        Time.timeScale = 1f;
        yield return WaitRealtime(1f);
        Assert.IsNotNull(GetProperty(FindByTypeName("EnemySpawner"), "ActiveBoss"), "Boss did not spawn");
        yield return Screenshot("05_boss");

        Invoke(game, "GameOver");
        yield return null;
        yield return null;
        yield return Screenshot("06_gameover");
        Assert.AreEqual(0f, Time.timeScale, "Game over should pause the game");

        Time.timeScale = 1f;
        yield return LoadScene("MenuScene");
        yield return WaitRealtime(0.5f);
    }

    private static bool IsGameOver(Component game)
    {
        FieldInfo field = game.GetType().GetField("m_IsGameOver", AnyInstance);
        return field != null && (bool)field.GetValue(game);
    }

    private static void PickFirstUpgrade()
    {
        Component upgradeManager = FindByTypeName("UpgradeManager");
        Component uiManager = FindByTypeName("UIManager");
        var options = (Array)Invoke(upgradeManager, "GetUpgradeOptions", 1);
        Assert.That(options.Length, Is.GreaterThan(0), "No upgrade options offered");
        Invoke(uiManager, "SelectUpgrade", options.GetValue(0));
    }

    private static IEnumerator LoadScene(string name)
    {
        AsyncOperation load = SceneManager.LoadSceneAsync(name);
        while (!load.isDone) yield return null;
        yield return null;
    }

    private static IEnumerator WaitRealtime(float seconds)
    {
        float end = Time.realtimeSinceStartup + seconds;
        while (Time.realtimeSinceStartup < end) yield return null;
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

    private static Component FindByTypeName(string typeName)
    {
        foreach (var behaviour in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include))
            if (behaviour.GetType().Name == typeName)
                return behaviour;
        return null;
    }

    private static object Invoke(Component target, string method, params object[] args)
    {
        MethodInfo info = target.GetType().GetMethod(method, AnyInstance);
        Assert.IsNotNull(info, $"{target.GetType().Name}.{method} not found");
        return info.Invoke(target, args);
    }

    private static object GetProperty(Component target, string property)
    {
        PropertyInfo info = target.GetType().GetProperty(property, AnyInstance);
        Assert.IsNotNull(info, $"{target.GetType().Name}.{property} not found");
        return info.GetValue(target);
    }
}
