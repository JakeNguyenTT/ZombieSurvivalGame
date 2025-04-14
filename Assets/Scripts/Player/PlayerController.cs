using StarterAssets;
using UnityEngine;

[RequireComponent(typeof(StarterAssetsInputs))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private WeaponSystem m_WeaponSystem;
    [SerializeField] private PlayerManager m_PlayerManager;
    private StarterAssetsInputs m_Input;
    private bool m_IsFiring = false;

    private void Awake()
    {
        m_Input = GetComponent<StarterAssetsInputs>();
    }

    void Update()
    {
        if (m_Input.shoot)
        {
            Debug.Log("Shoot");
            m_IsFiring = true;
        }
    }

    private void FixedUpdate()
    {
        if (m_IsFiring)
        {
            Debug.Log("Fire");
            m_WeaponSystem.FireMainWeapon(m_PlayerManager.GunMuzzle.position);
            m_IsFiring = false;
        }
    }
}