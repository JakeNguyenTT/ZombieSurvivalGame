using UnityEngine;
using System.Collections.Generic;

public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance;
    [SerializeField] private int initialPoolSize = 20;
    [Header("Read Only")]
    [SerializeField] private List<ProjectileData> m_ProjectileDatas = new List<ProjectileData>();
    private Dictionary<string, Queue<Projectile>> pools = new Dictionary<string, Queue<Projectile>>();
    private Dictionary<string, ProjectileData> m_DataByName = new Dictionary<string, ProjectileData>();

    private void Awake()
    {
        Instance = this;
        m_ProjectileDatas.Clear();
    }

    public Projectile GetProjectile(Projectile prefab)
    {
        if (!pools.ContainsKey(prefab.name))
        {
            pools[prefab.name] = new Queue<Projectile>();
            var data = new ProjectileData { ProjectileName = prefab.name };
            m_DataByName[prefab.name] = data;
            m_ProjectileDatas.Add(data);
            Preload(prefab, initialPoolSize);
        }

        Queue<Projectile> pool = pools[prefab.name];
        if (pool.Count == 0) Preload(prefab, initialPoolSize / 2);

        Projectile proj = pool.Dequeue();
        m_DataByName[prefab.name].currentPoolSize = pool.Count;
        return proj;
    }

    private void Preload(Projectile prefab, int count)
    {
        Debug.Log($"Preloading {count} {prefab.name} projectiles");
        for (int i = 0; i < count; i++)
        {
            Projectile proj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            proj.name = prefab.name;
            proj.gameObject.SetActive(false);
            pools[prefab.name].Enqueue(proj);
        }
        m_DataByName[prefab.name].currentPoolSize = pools[prefab.name].Count;
    }

    public void ReturnProjectile(Projectile proj)
    {
        if (proj == null) return;
        // Return to pool
        pools[proj.name].Enqueue(proj);
        m_DataByName[proj.name].currentPoolSize = pools[proj.name].Count;
    }
}

[System.Serializable]
public class ProjectileData
{
    public string ProjectileName;
    public int currentPoolSize;
}
