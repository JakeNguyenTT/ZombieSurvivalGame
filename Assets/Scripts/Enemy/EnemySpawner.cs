using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<EnemyData> m_EnemyTypes;
    private Queue<EnemyBehavior> m_EnemyPool = new Queue<EnemyBehavior>();

    [SerializeField] private int m_InitialPoolSize = 10000;
    private float m_SpawnRate = 1f;
    private float m_SpawnTimer;

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
        if (m_EnemyPool.Count == 0) return;
        EnemyBehavior enemy = m_EnemyPool.Dequeue();
        Vector2 circle = Random.insideUnitCircle.normalized * 15f;
        Vector3 spawnPos = new Vector3(circle.x, 0, circle.y) + GameManager.Instance.GetPlayerPosition();
        enemy.Initialize(spawnPos, m_EnemyTypes[Random.Range(0, m_EnemyTypes.Count)]);
        enemy.gameObject.SetActive(true);
    }

    public void ReturnEnemy(EnemyBehavior enemy)
    {
        enemy.gameObject.SetActive(false);
        m_EnemyPool.Enqueue(enemy);
    }
}