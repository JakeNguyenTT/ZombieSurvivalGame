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
}
