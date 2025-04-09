using UnityEngine;

public class ExperienceGem : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float pickupRange = 2f;
    [SerializeField] private float expValue = 10f;

    void Update()
    {
        Vector3 playerPos = GameManager.Instance.GetPlayerPosition();
        float distance = Vector3.Distance(transform.position, playerPos);
        if (distance < pickupRange)
        {
            transform.position = Vector3.MoveTowards(transform.position, playerPos, speed * Time.deltaTime);
            if (distance < 0.1f)
            {
                ExperienceManager.Instance.AddExperience(expValue);
                Destroy(gameObject);
            }
        }
    }
}