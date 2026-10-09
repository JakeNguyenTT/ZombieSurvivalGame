using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private PlayerManager m_Player;
    [SerializeField] private EnemySpawner m_EnemySpawner;
    [SerializeField] private UIManager m_UIManager;
    [SerializeField] private AudioManager m_AudioManager;
    public event Action<RunResult> OnGameOver;
    private bool m_IsGameOver;
    private int m_EnemyKilled;
    private float m_GameTime;
    private bool m_IsPlaying;
    public bool IsPlaying => m_IsPlaying;
    public float GameTime => m_GameTime;
    public int EnemyKilled
    {
        get => m_EnemyKilled;
        set
        {
            m_EnemyKilled = value;
        }
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        ArenaProps.Spawn(GetPlayerPosition());
        gameObject.AddComponent<EnemyNavigation>();
        m_EnemySpawner.Initialize();
        m_UIManager.Initialize();
        // m_AudioManager.PlayMusic("BackgroundMusic");
        StartGame();
    }

    void Update()
    {
        if (!m_IsPlaying) return;
        int previousSecond = (int)m_GameTime;
        m_GameTime += Time.deltaTime;
        // Only rebuild the time string when the displayed second changes
        if ((int)m_GameTime != previousSecond)
            m_UIManager.UpdateTime(m_GameTime);
    }

    public void StartGame()
    {
        m_IsPlaying = true;
        m_EnemySpawner.StartSpawning();
        m_EnemyKilled = 0;
        Time.timeScale = 1;
    }

    public void GameOver()
    {
        if (m_IsGameOver) return;
        m_IsGameOver = true;
        m_IsPlaying = false;
        Time.timeScale = 0;
        m_EnemySpawner.StopSpawning();
        OnGameOver?.Invoke(RecordRun());
    }

    // Awards coins and updates personal bests
    private RunResult RecordRun()
    {
        int bosses = m_EnemySpawner.BossesKilled;
        var result = new RunResult
        {
            Time = m_GameTime,
            Kills = m_EnemyKilled,
            Level = ExperienceManager.Instance.CurrentLevel,
            Bosses = bosses,
            Coins = RunRewards.Coins(m_EnemyKilled, m_GameTime, bosses),
            NewBestTime = m_GameTime > SaveData.BestTime,
            NewBestKills = m_EnemyKilled > SaveData.BestKills,
        };
        if (result.NewBestTime) SaveData.BestTime = m_GameTime;
        if (result.NewBestKills) SaveData.BestKills = m_EnemyKilled;
        SaveData.Coins += result.Coins;
        return result;
    }

    public void PauseGame()
    {
        m_IsPlaying = false;
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        m_IsPlaying = true;
        Time.timeScale = 1;
    }

    public void SpawnEnemies(int count)
    {
        for (int i = 0; i < count; i++)
        {
            m_EnemySpawner.SpawnEnemy();
        }
    }

    public void KillEnemy()
    {
        m_EnemyKilled++;
        m_UIManager.UpdateEnemyKilled(m_EnemyKilled);
    }

    public void SkipToBoss()
    {
        m_EnemySpawner.SpawnBossNow();
    }

    public Vector3 GetPlayerPosition() => m_Player.transform.position;
}
