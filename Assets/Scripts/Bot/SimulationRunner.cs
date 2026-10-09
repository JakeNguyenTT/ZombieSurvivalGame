using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

// Hidden balance-simulation mode; a normal launch never creates it. Started as
//   ZombieSurvival.exe -batchmode -nographics -simulate [-seeds 1-5] [-out results.jsonl] [-maxTime 720]
// it plays BotPlayer games (fresh in-memory profile, Soldier, no shop upgrades) one after another
// at a fixed 1/30 s step as fast as the CPU allows, writes one JSON line per game, then quits.
// Tools/RunSimulation.ps1 starts several of these in parallel and summarizes the results.
public class SimulationRunner : MonoBehaviour
{
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
        public int gemsSpawned;
        public float experience;
        public string weapons;
    }

    private int m_FirstSeed = 1;
    private int m_LastSeed = 5;
    private string m_OutputPath = "simulation-results.jsonl";
    private float m_MaxTime = 720f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        string[] args = Environment.GetCommandLineArgs();
        if (Array.IndexOf(args, "-simulate") < 0) return;
        var runner = new GameObject(nameof(SimulationRunner)).AddComponent<SimulationRunner>();
        runner.ParseArguments(args);
    }

    private void ParseArguments(string[] args)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            string value = args[i + 1];
            switch (args[i])
            {
                case "-seeds":
                    string[] range = value.Split('-');
                    m_FirstSeed = int.Parse(range[0], CultureInfo.InvariantCulture);
                    m_LastSeed = range.Length > 1 ? int.Parse(range[1], CultureInfo.InvariantCulture) : m_FirstSeed;
                    break;
                case "-out":
                    m_OutputPath = value;
                    break;
                case "-maxTime":
                    m_MaxTime = float.Parse(value, CultureInfo.InvariantCulture);
                    break;
            }
        }
    }

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        SaveData.InMemory = true;
        Application.runInBackground = true;
        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = 0;
        File.WriteAllText(m_OutputPath, "");

        for (int seed = m_FirstSeed; seed <= m_LastSeed; seed++)
            yield return RunGame(seed);

        Application.Quit(0);
    }

    private IEnumerator RunGame(int seed)
    {
        Time.captureDeltaTime = 0f;
        Time.timeScale = 1f;
        Random.InitState(seed);
        AsyncOperation load = SceneManager.LoadSceneAsync("GameScene");
        while (!load.isDone) yield return null;
        yield return null;

        PlayerManager player = PlayerManager.Instance;
        player.gameObject.AddComponent<BotPlayer>();
        yield return null;
        Time.captureDeltaTime = Step;

        var record = new RunRecord { seed = seed };
        Vector3 lastPosition = player.transform.position;
        GameManager game = GameManager.Instance;
        while (!game.IsGameOver && game.GameTime < m_MaxTime)
        {
            float time = game.GameTime;
            float health = player.HealthFraction;
            if (record.hp120 < 0f && time >= 120f) record.hp120 = health;
            if (record.hp300 < 0f && time >= 300f) record.hp300 = health;
            if (record.hp600 < 0f && time >= 600f) record.hp600 = health;
            record.distanceMoved += Vector3.Distance(player.transform.position, lastPosition);
            lastPosition = player.transform.position;
            yield return null;
        }

        record.died = game.IsGameOver;
        record.time = game.GameTime;
        record.kills = game.EnemyKilled;
        record.level = ExperienceManager.Instance.CurrentLevel;
        record.experience = ExperienceManager.Instance.TotalCollected;
        record.gemsSpawned = ExpSpawner.Instance.GemsSpawned;
        record.bossesSpawned = EnemySpawner.Instance.BossesSpawned;
        record.bossesKilled = EnemySpawner.Instance.BossesKilled;
        record.weapons = DescribeWeapons(FindAnyObjectByType<WeaponSystem>());

        string line = JsonUtility.ToJson(record);
        File.AppendAllText(m_OutputPath, line + "\n");
        Debug.Log("[Simulation] " + line);
        Time.captureDeltaTime = 0f;
    }

    private static string DescribeWeapons(WeaponSystem weaponSystem)
    {
        var builder = new StringBuilder();
        foreach (WeaponInstance weapon in weaponSystem.Weapons)
        {
            if (builder.Length > 0) builder.Append(", ");
            builder.Append(weapon.data.weaponName).Append(" Lv").Append(weapon.level);
        }
        return builder.ToString();
    }
}
