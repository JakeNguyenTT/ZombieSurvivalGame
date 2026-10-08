using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }
    [SerializeField] private List<EnemyData> m_EnemyTypes;
    // One pool per prefab so an instance is always reused with a matching model
    private readonly Dictionary<EnemyBehavior, Queue<EnemyBehavior>> m_Pools = new Dictionary<EnemyBehavior, Queue<EnemyBehavior>>();
    private readonly List<EnemyBehavior> m_ActiveEnemies = new List<EnemyBehavior>();
    private readonly List<float> m_SpawnWeights = new List<float>();

    [SerializeField] private float m_SpawnRange = 25f;

    [SerializeField] private int m_InitialPoolSize = 10000;
    [SerializeField] private int m_PoolGrowSize = 10;
    [Header("Read Only")]
    [SerializeField] private int m_CurrentActiveEnemies = 0;

    private float m_SpawnRate = 1f;
    private float m_SpawnTimer;
    private bool m_IsBossSpawned = false;

    void Awake()
    {
        Instance = this;
    }

    public void Initialize()
    {
        PreloadEnemies(m_InitialPoolSize);
    }

    public void StartSpawning()
    {
        StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        StopAllCoroutines();
    }

    private void PreloadEnemies(int total)
    {
        var prefabs = new List<EnemyBehavior>();
        foreach (var enemyType in m_EnemyTypes)
            if (enemyType.prefab != null && !prefabs.Contains(enemyType.prefab))
                prefabs.Add(enemyType.prefab);
        if (prefabs.Count == 0) return;

        int perPrefab = Mathf.Max(1, total / prefabs.Count);
        foreach (var prefab in prefabs)
            for (int i = 0; i < perPrefab; i++)
                CreateEnemy(prefab);
    }

    private void CreateEnemy(EnemyBehavior prefab)
    {
        EnemyBehavior enemy = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        enemy.SourcePrefab = prefab;
        enemy.gameObject.SetActive(false);
        GetPool(prefab).Enqueue(enemy);
    }

    private Queue<EnemyBehavior> GetPool(EnemyBehavior prefab)
    {
        if (!m_Pools.TryGetValue(prefab, out var pool))
        {
            pool = new Queue<EnemyBehavior>();
            m_Pools.Add(prefab, pool);
        }
        return pool;
    }

    private EnemyBehavior TakeFromPool(EnemyBehavior prefab)
    {
        var pool = GetPool(prefab);
        if (pool.Count == 0)
            for (int i = 0; i < m_PoolGrowSize; i++)
                CreateEnemy(prefab);
        return pool.Dequeue();
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

    // Weighted pick among the types unlocked at the current run time
    private EnemyData PickEnemyType()
    {
        float time = GameManager.Instance.GameTime;
        m_SpawnWeights.Clear();
        foreach (var enemyType in m_EnemyTypes)
            m_SpawnWeights.Add(time >= enemyType.unlockTime ? enemyType.spawnWeight : 0f);
        int index = WeightedRandom.Pick(m_SpawnWeights, Random.value);
        return index >= 0 ? m_EnemyTypes[index] : m_EnemyTypes[0];
    }

    private Vector3 RandomSpawnPosition()
    {
        Vector2 circle = Random.insideUnitCircle.normalized * m_SpawnRange;
        return new Vector3(circle.x, 0, circle.y) + GameManager.Instance.GetPlayerPosition();
    }

    public void SpawnEnemy()
    {
        var enemyType = PickEnemyType();
        EnemyBehavior enemy = TakeFromPool(enemyType.prefab);
        Vector3 spawnPos = RandomSpawnPosition();
        // if time more than 30 seconds, increase enemy health, scale with time
        var enemyEnhancement = new EnemyEnhancement();
        var enemyKilled = GameManager.Instance.EnemyKilled;
        if (enemyKilled > 30)
        {
            // Bonus on top of base stats (EnemyBehavior.Initialize adds base + enhancement)
            enemyEnhancement.health = enemyKilled - 30;
            enemyEnhancement.damage = (enemyKilled - 30) * 0.1f;
        }
        // if time more than 60 seconds, spawn enemy boss, bigger, more health, slower speed, more damage
        if (enemyKilled > 20 && !m_IsBossSpawned)
        {
            enemy.InitializeBoss(spawnPos, m_EnemyTypes[0], enemyEnhancement, 1);
            m_IsBossSpawned = true;
        }
        else
        {
            enemy.Initialize(spawnPos, enemyType, enemyEnhancement);
        }
        enemy.gameObject.SetActive(true);
        m_ActiveEnemies.Add(enemy);
        m_CurrentActiveEnemies = m_ActiveEnemies.Count;
    }

    public void ReturnEnemy(EnemyBehavior enemy)
    {
        enemy.gameObject.SetActive(false);
        GetPool(enemy.SourcePrefab).Enqueue(enemy);
        m_ActiveEnemies.Remove(enemy);
        m_CurrentActiveEnemies = m_ActiveEnemies.Count;
        GameManager.Instance.KillEnemy();
    }

    public EnemyBehavior GetClosestEnemy(Vector3 position)
    {
        EnemyBehavior closest = null;
        float closestSqrDistance = float.MaxValue;
        foreach (var enemy in m_ActiveEnemies)
        {
            float sqrDistance = (enemy.transform.position - position).sqrMagnitude;
            if (sqrDistance < closestSqrDistance)
            {
                closestSqrDistance = sqrDistance;
                closest = enemy;
            }
        }
        return closest;
    }
}

[System.Serializable]
public class EnemyEnhancement
{
    public float health;
    public float speed;
    public float damage;
}
