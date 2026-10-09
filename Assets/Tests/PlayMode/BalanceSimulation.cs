using System;
using System.Collections;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static PlayModeHelpers;

// Measures balance: the BotPlayer plays seeded games (fresh profile, Soldier, no meta upgrades)
// at a fixed 1/30 s step, as fast as the machine allows, and one JSON line per game goes to
// Temp/Balance/results.jsonl. Explicit: run on demand, not with the normal suite.
public class BalanceSimulation
{
    private const int Runs = 10;
    private const float MaxGameTime = 720f; // 12 minutes
    private const float Step = 1f / 30f;

    [Serializable]
    private class RunRecord
    {
        public int seed;
        public float time;
        public bool died;
        public int kills;
        public int level;
        public int bossesSpawned;
        public int bossesKilled;
        public float hp120 = -1f;
        public float hp300 = -1f;
        public float hp600 = -1f;
        public float distanceMoved;
        public string weapons;
    }

    private readonly PrefsSnapshot m_Prefs = new PrefsSnapshot();
    private int m_TargetFrameRate;
    private int m_VSync;

    [SetUp]
    public void SetUp()
    {
        m_Prefs.Capture();
        m_TargetFrameRate = Application.targetFrameRate;
        m_VSync = QualitySettings.vSyncCount;
        Application.targetFrameRate = -1; // run frames as fast as possible
        QualitySettings.vSyncCount = 0;
    }

    [TearDown]
    public void TearDown()
    {
        Time.captureDeltaTime = 0f;
        Time.timeScale = 1f;
        Application.targetFrameRate = m_TargetFrameRate;
        QualitySettings.vSyncCount = m_VSync;
        m_Prefs.Restore();
    }

    [UnityTest, Explicit("Long-running balance measurement; run on demand"), Timeout(7200000)]
    public IEnumerator RunBotGames()
    {
        string folder = Path.Combine(Application.dataPath, "..", "Temp", "Balance");
        Directory.CreateDirectory(folder);
        string file = Path.Combine(folder, "results.jsonl");
        File.WriteAllText(file, "");
        Type botType = FindType("BotPlayer");
        Assert.IsNotNull(botType, "BotPlayer type not found");

        for (int seed = 1; seed <= Runs; seed++)
        {
            PrefsSnapshot.ClearAll();
            UnityEngine.Random.InitState(seed);
            Time.captureDeltaTime = 0f;
            Time.timeScale = 1f;
            yield return LoadScene("GameScene");

            Component player = FindByTypeName("PlayerManager");
            Component game = FindByTypeName("GameManager");
            player.gameObject.AddComponent(botType);
            yield return null; // let the bot read the camera before rendering is switched off
            StopRendering();
            Time.captureDeltaTime = Step;

            var record = new RunRecord { seed = seed };
            Vector3 lastPosition = player.transform.position;
            while (!IsGameOver(game))
            {
                float time = (float)GetProperty(game, "GameTime");
                if (time >= MaxGameTime) break;
                float health = (float)GetProperty(player, "HealthFraction");
                if (record.hp120 < 0f && time >= 120f) record.hp120 = health;
                if (record.hp300 < 0f && time >= 300f) record.hp300 = health;
                if (record.hp600 < 0f && time >= 600f) record.hp600 = health;
                record.distanceMoved += Vector3.Distance(player.transform.position, lastPosition);
                lastPosition = player.transform.position;
                yield return null;
            }

            record.died = IsGameOver(game);
            record.time = (float)GetProperty(game, "GameTime");
            record.kills = (int)GetProperty(game, "EnemyKilled");
            record.level = (int)GetProperty(FindByTypeName("ExperienceManager"), "CurrentLevel");
            Component spawner = FindByTypeName("EnemySpawner");
            record.bossesSpawned = (int)GetField(spawner, "m_BossLevel");
            record.bossesKilled = (int)GetProperty(spawner, "BossesKilled");
            record.weapons = DescribeWeapons(FindByTypeName("WeaponSystem"));

            string line = JsonUtility.ToJson(record);
            File.AppendAllText(file, line + "\n");
            Debug.Log("[Balance] " + line);
            Time.captureDeltaTime = 0f;
        }
    }

    // Nobody watches the simulation: skip drawing the world and the UI (about 2-3x faster).
    // Only rendering is turned off; camera transforms keep updating, so game logic is unchanged.
    // The next scene load brings everything back.
    private static void StopRendering()
    {
        foreach (Camera camera in UnityEngine.Object.FindObjectsByType<Camera>())
            camera.enabled = false;
        foreach (Canvas canvas in UnityEngine.Object.FindObjectsByType<Canvas>())
            canvas.enabled = false;
    }

    private static string DescribeWeapons(Component weaponSystem)
    {
        var builder = new StringBuilder();
        foreach (object weapon in (IEnumerable)GetProperty(weaponSystem, "Weapons"))
        {
            object data = GetField(weapon, "data");
            if (builder.Length > 0) builder.Append(", ");
            builder.Append(GetField(data, "weaponName")).Append(" Lv").Append(GetField(weapon, "level"));
        }
        return builder.ToString();
    }

    private static Type FindType(string name)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type type = assembly.GetType(name);
            if (type != null) return type;
        }
        return null;
    }
}
