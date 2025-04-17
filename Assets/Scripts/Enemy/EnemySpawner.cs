using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }
    [SerializeField] private List<EnemyData> m_EnemyTypes;
    private Queue<EnemyBehavior> m_EnemyPool = new Queue<EnemyBehavior>();

    [SerializeField] private float m_SpawnRange = 25f;

    [SerializeField] private int m_InitialPoolSize = 10000;
    [Header("Read Only")]
    [SerializeField] private int m_CurrentActiveEnemies = 0;

    private float m_SpawnRate = 1f;
    private float m_SpawnTimer;
    private bool m_IsBossSpawned = false;

    void Awake()
    {
        Instance = this;
    }

    // Call this to populate the pool initially (e.g., from GameManager)
    public void InitializePool(EnemyBehavior[] enemies)
    {
        foreach (var enemy in enemies)
            m_EnemyPool.Enqueue(enemy);
    }

    public void Initialize()
    {
        PreloadEnemies();
    }

    public void StartSpawning()
    {
        StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        StopAllCoroutines();
    }

    private void PreloadEnemies()
    {
        foreach (var enemyType in m_EnemyTypes)
        {
            for (int i = 0; i < m_InitialPoolSize / m_EnemyTypes.Count; i++)
            {
                EnemyBehavior enemy = Instantiate(enemyType.prefab, Vector3.zero, Quaternion.identity);
                enemy.gameObject.SetActive(false);
                m_EnemyPool.Enqueue(enemy);
            }
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            m_SpawnTimer -= Time.deltaTime;
            if (m_SpawnTimer <= 0)
            {
                SpawnEnemy();
                m_SpawnRate = Mathf.Max(0.1f, m_SpawnRate * 0.99f); // Increase difficulty
                m_SpawnTimer = m_SpawnRate;
            }
            yield return null;
        }
    }

    public void SpawnEnemy()
    {
        if (m_EnemyPool.Count == 0)
            PreloadEnemies();

        EnemyBehavior enemy = m_EnemyPool.Dequeue();
        Vector2 circle = Random.insideUnitCircle.normalized * m_SpawnRange;
        Vector3 spawnPos = new Vector3(circle.x, 0, circle.y) + GameManager.Instance.GetPlayerPosition();
        var enemyType = m_EnemyTypes[Random.Range(0, m_EnemyTypes.Count)];
        // if time more than 30 seconds, increase enemy health, scale with time
        var enemyEnhancement = new EnemyEnhancement();
        var enemyKilled = GameManager.Instance.EnemyKilled;
        if (enemyKilled > 30)
        {
            enemyEnhancement.health = enemyType.health + (enemyKilled - 40);
            enemyEnhancement.damage = enemyType.damage + (enemyKilled - 40) * 0.1f;
        }
        // if time more than 60 seconds, spawn enemy boss, bigger, more health, slower speed, more damage
        if (enemyKilled > 30 && !m_IsBossSpawned)
        {
            enemy.InitializeBoss(spawnPos, enemyType, enemyEnhancement, 1);
            m_IsBossSpawned = true;
        }
        else
        {
            enemy.Initialize(spawnPos, enemyType, enemyEnhancement);
        }
        enemy.gameObject.SetActive(true);
        m_CurrentActiveEnemies++;
    }

    public void ReturnEnemy(EnemyBehavior enemy)
    {
        enemy.gameObject.SetActive(false);
        m_EnemyPool.Enqueue(enemy);
        m_CurrentActiveEnemies--;
        GameManager.Instance.EnemyKilled++;
    }
}

[System.Serializable]
public class EnemyEnhancement
{
    public float health;
    public float speed;
    public float damage;
}

