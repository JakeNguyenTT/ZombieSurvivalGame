using UnityEngine;

public class ExperienceGem : MonoBehaviour
{
    [SerializeField] private float m_Speed = 5f;
    [SerializeField] private float m_ExpValue = 10f;

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
            if (sqrDistance < 0.1f * 0.1f)
            {
                ExperienceManager.Instance.AddExperience(m_ExpValue);
                ExpSpawner.Instance.ReturnGem(this);
            }
        }
    }
}