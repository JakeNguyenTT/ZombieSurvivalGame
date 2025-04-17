using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpSpawner : MonoBehaviour
{
    public static ExpSpawner Instance;
    [SerializeField] private ExperienceGem m_ExpGemPrefab;

    void Awake()
    {
        Instance = this;
    }

    public void SpawnExp(Vector3 position)
    {
        position.y = 0.5f;
        Instantiate(m_ExpGemPrefab, position, Quaternion.identity);
    }

    public void SpawnExpAround(Vector3 position, int count = 10, float range = 10)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 randomPosition = new Vector3(Random.Range(-range, range), 0.5f, Random.Range(-range, range));
            Instantiate(m_ExpGemPrefab, position + randomPosition, Quaternion.identity);
        }
    }
}
