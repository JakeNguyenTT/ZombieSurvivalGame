using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Reuses particle effect instances instead of Instantiate/Destroy on every hit or kill.
// Created on first use and lives with the current scene.
public class EffectPool : MonoBehaviour
{
    private static EffectPool s_Instance;
    private readonly Dictionary<ParticleSystem, Queue<ParticleSystem>> m_Pools = new Dictionary<ParticleSystem, Queue<ParticleSystem>>();

    public static void Play(ParticleSystem prefab, Vector3 position)
    {
        if (prefab == null) return;
        if (s_Instance == null)
            s_Instance = new GameObject("EffectPool").AddComponent<EffectPool>();
        s_Instance.PlayInternal(prefab, position);
    }

    private void PlayInternal(ParticleSystem prefab, Vector3 position)
    {
        if (!m_Pools.TryGetValue(prefab, out var pool))
        {
            pool = new Queue<ParticleSystem>();
            m_Pools.Add(prefab, pool);
        }

        ParticleSystem effect = pool.Count > 0 ? pool.Dequeue() : Instantiate(prefab, transform);
        effect.transform.SetPositionAndRotation(position, Quaternion.identity);
        effect.gameObject.SetActive(true);
        effect.Play(true);
        StartCoroutine(ReturnAfter(effect, pool, effect.main.duration));
    }

    private IEnumerator ReturnAfter(ParticleSystem effect, Queue<ParticleSystem> pool, float delay)
    {
        yield return new WaitForSeconds(delay);
        effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        effect.gameObject.SetActive(false);
        pool.Enqueue(effect);
    }
}
