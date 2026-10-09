using System.Collections.Generic;
using UnityEngine;

public class ExperienceGem : MonoBehaviour
{
    [SerializeField] private float m_Speed = 5f;
    [SerializeField] private float m_ExpValue = 10f;
    private const float CollectRadius = 0.5f;

    private static readonly List<ExperienceGem> s_Active = new List<ExperienceGem>();

    // Gems currently lying in the world
    public static IReadOnlyList<ExperienceGem> Active => s_Active;

    void OnEnable() => s_Active.Add(this);

    void OnDisable() => s_Active.Remove(this);

    void Update()
    {
        Vector3 playerPos = GameManager.Instance.GetPlayerPosition();
        float sqrDistance = (transform.position - playerPos).sqrMagnitude;
        float radius = PlayerManager.Instance.PickupRadius;
        if (sqrDistance < radius * radius)
        {
            // Pull faster with a bigger radius so far-away gems don't crawl in
            float speed = Mathf.Max(m_Speed, radius * 3f);
            transform.position = Vector3.MoveTowards(transform.position, playerPos, speed * Time.deltaTime);
            // Checked after moving, with a radius bigger than one frame of player movement:
            // checking the old distance against 0.1 never collected while walking at 30 fps
            if ((transform.position - playerPos).sqrMagnitude < CollectRadius * CollectRadius)
            {
                ExperienceManager.Instance.AddExperience(m_ExpValue);
                ExpSpawner.Instance.ReturnGem(this);
            }
        }
    }
}