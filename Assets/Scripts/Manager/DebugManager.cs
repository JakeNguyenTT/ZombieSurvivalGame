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
            Vector3 playerPosition = GameManager.Instance.GetPlayerPosition();
            ExpSpawner.Instance.SpawnExpAround(playerPosition, 100, 20);
        }

        if (m_SpawnEnemies)
        {
            m_SpawnEnemies = false;
            GameManager.Instance.SpawnEnemies(m_EnemyToSpawn);
        }

        if (m_SpawnBoss60Seconds)
        {
            m_SpawnBoss60Seconds = false;
            GameManager.Instance.SkipToBoss();
        }

        if (m_GameOver)
        {
            m_GameOver = false;
            GameManager.Instance.GameOver();
        }
    }
}
