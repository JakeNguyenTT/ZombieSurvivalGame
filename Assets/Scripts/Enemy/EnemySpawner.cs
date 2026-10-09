using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Random = UnityEngine.Random;

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

    private float m_SpawnTimer;
    private int m_BossLevel; // number of bosses spawned so far

    public IReadOnlyList<EnemyBehavior> ActiveEnemies => m_ActiveEnemies;
    public EnemyBehavior ActiveBoss { get; private set; }
    public int BossesKilled { get; private set; }
    public int BossesSpawned => m_BossLevel;
    public event Action<EnemyBehavior> OnBossSpawned;

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
            float time = GameManager.Instance.GameTime;
            if (Difficulty.BossLevelAt(time) > m_BossLevel)
                SpawnBoss(m_BossLevel + 1);

            m_SpawnTimer -= Time.deltaTime;
            if (m_SpawnTimer <= 0)
            {
                int batch = Difficulty.SpawnBatch(time);
                for (int i = 0; i < batch; i++)
                    SpawnEnemy();
                m_SpawnTimer = Difficulty.SpawnInterval(time);
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
        // A few tries to avoid dropping an enemy inside a rock or tree
        Vector3 position = Vector3.zero;
        for (int attempt = 0; attempt < 5; attempt++)
        {
            Vector2 circle = Random.insideUnitCircle.normalized * m_SpawnRange;
            position = new Vector3(circle.x, 0, circle.y) + GameManager.Instance.GetPlayerPosition();
            if (ArenaProps.IsClear(position, 2f)) break;
        }
        return position;
    }

    public void SpawnEnemy()
    {
        var enemyType = PickEnemyType();
        EnemyBehavior enemy = TakeFromPool(enemyType.prefab);
        enemy.Initialize(RandomSpawnPosition(), enemyType, CurrentEnhancement(enemyType));
        Track(enemy);
    }

    // Spawns the next boss immediately (debug / skip)
    public void SpawnBossNow() => SpawnBoss(m_BossLevel + 1);

    private void SpawnBoss(int level)
    {
        m_BossLevel = level;
        var bossType = m_EnemyTypes[0];
        EnemyBehavior boss = TakeFromPool(bossType.prefab);
        boss.InitializeBoss(RandomSpawnPosition(), bossType, level, Difficulty.HealthMultiplier(GameManager.Instance.GameTime));
        Track(boss);
        ActiveBoss = boss;
        if (bossType.specialSound != null)
            AudioManager.Instance.PlaySFX(bossType.specialSound, boss.transform.position, 1f, 0.8f); // pitched down: bigger
        OnBossSpawned?.Invoke(boss);
    }

    // Stats on top of the base type, growing with run time
    private EnemyEnhancement CurrentEnhancement(EnemyData enemyType)
    {
        float time = GameManager.Instance.GameTime;
        return new EnemyEnhancement
        {
            health = enemyType.health * (Difficulty.HealthMultiplier(time) - 1f),
            damage = enemyType.damage * (Difficulty.DamageMultiplier(time) - 1f),
        };
    }

    private void Track(EnemyBehavior enemy)
    {
        enemy.gameObject.SetActive(true);
        m_ActiveEnemies.Add(enemy);
        m_CurrentActiveEnemies = m_ActiveEnemies.Count;
    }

    public void ReturnEnemy(EnemyBehavior enemy)
    {
        if (enemy.IsBoss) BossesKilled++;
        if (enemy == ActiveBoss) ActiveBoss = null;
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
