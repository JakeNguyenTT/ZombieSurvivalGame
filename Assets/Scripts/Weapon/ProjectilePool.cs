using UnityEngine;
using System.Collections.Generic;

public class ProjectilePool : MonoBehaviour
{
    [SerializeField] private int initialPoolSize = 100;
    private Dictionary<Projectile, Queue<Projectile>> pools = new Dictionary<Projectile, Queue<Projectile>>();

    void Start()
    {
        // Preload pools as needed
    }

    public Projectile GetProjectile(Projectile prefab)
    {
        if (!pools.ContainsKey(prefab))
        {
            pools[prefab] = new Queue<Projectile>();
            Preload(prefab, initialPoolSize);
        }

        Queue<Projectile> pool = pools[prefab];
        if (pool.Count == 0) Preload(prefab, initialPoolSize / 2);

        Projectile proj = pool.Dequeue();
        return proj;
    }

    private void Preload(Projectile prefab, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Projectile proj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            proj.gameObject.SetActive(false);
            pools[prefab].Enqueue(proj);
        }
    }
}