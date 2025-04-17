using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    [SerializeField] private ExperienceGem m_ExperienceGemPrefabs;

    [Header("Debug")]
    [SerializeField] private bool m_Spawn100Gems = false;

    [SerializeField] private bool m_SpawnEnemies = false;
    [SerializeField] private int m_EnemyToSpawn = 100;
    [SerializeField] private bool m_GameOver = false;
    [SerializeField] private bool m_SpawnBoss60Seconds = false;

    void Update()
    {
        if (m_Spawn100Gems)
        {
            m_Spawn100Gems = false;
            // spawn 100 gems around player
            Vector3 playerPosition = GameManager.Instance.GetPlayerPosition();
            for (int i = 0; i < 100; i++)
            {
                Vector3 randomPosition = new Vector3(Random.Range(-20f, 20f), 0, Random.Range(-20f, 20f));
                ExpSpawner.Instance.SpawnExp(playerPosition + randomPosition);
            }
        }

        if (m_SpawnEnemies)
        {
            m_SpawnEnemies = false;
            GameManager.Instance.SpawnEnemies(m_EnemyToSpawn);
        }

        if (m_SpawnBoss60Seconds)
        {
            m_SpawnBoss60Seconds = false;
            GameManager.Instance.SpawnBoss();
        }

        if (m_GameOver)
        {
            m_GameOver = false;
            GameManager.Instance.GameOver();
        }
    }
}
