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
    public event Action<float> OnGameOver;
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
        m_EnemySpawner.Initialize();
        m_UIManager.Initialize();
        // m_AudioManager.PlayMusic("BackgroundMusic");
        StartGame();
    }

    void Update()
    {
        if (!m_IsPlaying) return;
        m_GameTime += Time.deltaTime;
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
        // Implement game over logic (e.g., show UI, stop spawning)
        Time.timeScale = 0;
        m_EnemySpawner.StopSpawning();
        OnGameOver?.Invoke(m_GameTime);
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
        m_EnemyKilled = 20;
    }

    public Vector3 GetPlayerPosition() => m_Player.transform.position;
}
