using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    [SerializeField] private ExperienceGem m_ExperienceGemPrefabs;

    [Header("Debug")]
    [SerializeField] private bool m_Spawn100Gems = false;
    [SerializeField] private bool m_Spawn100Enemies = false;
    [SerializeField] private bool m_GameOver = false;

    void Update()
    {
        if (m_Spawn100Gems)
        {
            m_Spawn100Gems = false;
            // spawn 100 gems around player
            Vector3 playerPosition = GameManager.Instance.GetPlayerPosition();
            for (int i = 0; i < 100; i++)
            {
                Vector3 randomPosition = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
                Instantiate(m_ExperienceGemPrefabs, playerPosition + randomPosition, Quaternion.identity);
            }
        }

        if (m_Spawn100Enemies)
        {
            m_Spawn100Enemies = false;
        }

        if (m_GameOver)
        {
            m_GameOver = false;
            GameManager.Instance.GameOver();
        }
    }
}
