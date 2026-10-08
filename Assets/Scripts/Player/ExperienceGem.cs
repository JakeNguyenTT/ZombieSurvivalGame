using UnityEngine;

public class ExperienceGem : MonoBehaviour
{
    [SerializeField] private float m_Speed = 5f;
    [SerializeField] private float m_ExpValue = 10f;

    void Update()
    {
        Vector3 playerPos = GameManager.Instance.GetPlayerPosition();
        float sqrDistance = (transform.position - playerPos).sqrMagnitude;
        if (sqrDistance < 2f * 2f)
        {
            transform.position = Vector3.MoveTowards(transform.position, playerPos, m_Speed * Time.deltaTime);
            if (sqrDistance < 0.1f * 0.1f)
            {
                ExperienceManager.Instance.AddExperience(m_ExpValue);
                ExpSpawner.Instance.ReturnGem(this);
            }
        }
    }
}