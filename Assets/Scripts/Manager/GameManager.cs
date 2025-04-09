using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private PlayerController m_Player;
    [SerializeField] private EnemySpawner m_EnemySpawner;
    [SerializeField] private UIManager m_UIManager;
    [SerializeField] private AudioManager m_AudioManager;

    private float gameTime;
    private bool isPlaying;

    public event Action OnGameOver;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        m_EnemySpawner.Initialize();
        m_UIManager.Initialize();
        m_AudioManager.PlayMusic("BackgroundMusic");
        StartGame();
    }

    void Update()
    {
        if (!isPlaying) return;
        gameTime += Time.deltaTime;
        m_UIManager.UpdateTime(gameTime);
    }

    public void StartGame()
    {
        isPlaying = true;
        m_EnemySpawner.StartSpawning();
    }

    public void GameOver()
    {
        // Implement game over logic (e.g., show UI, stop spawning)
        Time.timeScale = 0;
        m_EnemySpawner.StopSpawning();
        OnGameOver?.Invoke();
        m_UIManager.ShowGameOver(gameTime);
    }

    public Vector3 GetPlayerPosition() => m_Player.transform.position;
}
