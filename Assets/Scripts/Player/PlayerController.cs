using StarterAssets;
using UnityEngine;

[RequireComponent(typeof(StarterAssetsInputs))]
public class PlayerController : MonoBehaviour
{
    private StarterAssetsInputs m_Input;

    private void Awake()
    {
        m_Input = GetComponent<StarterAssetsInputs>();
    }

    void Update()
    {
        if (m_Input.shoot)
        {
            Debug.Log("Shoot");
        }
    }
}