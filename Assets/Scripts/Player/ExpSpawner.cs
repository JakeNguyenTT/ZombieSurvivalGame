using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpSpawner : MonoBehaviour
{
    public static ExpSpawner Instance;
    [SerializeField] private ExperienceGem m_ExpGemPrefab;
    private readonly Queue<ExperienceGem> m_Pool = new Queue<ExperienceGem>();
    public int GemsSpawned { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public void SpawnExp(Vector3 position)
    {
        position.y = 0.5f;
        GetGem(position);
    }

    public void SpawnExpAround(Vector3 position, int count = 10, float range = 10)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 randomPosition = new Vector3(Random.Range(-range, range), 0.5f, Random.Range(-range, range));
            GetGem(position + randomPosition);
        }
    }

    public void ReturnGem(ExperienceGem gem)
    {
        gem.gameObject.SetActive(false);
        m_Pool.Enqueue(gem);
    }

    private void GetGem(Vector3 position)
    {
        GemsSpawned++;
        if (m_Pool.Count == 0)
        {
            Instantiate(m_ExpGemPrefab, position, Quaternion.identity);
            return;
        }
        ExperienceGem gem = m_Pool.Dequeue();
        gem.transform.position = position;
        gem.gameObject.SetActive(true);
    }
}
