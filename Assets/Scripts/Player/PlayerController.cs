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
            Shoot();
        }
    }

    private void Shoot()
    {
        m_WeaponSystem.FireMainWeapon(m_PlayerManager.GunMuzzle.position);
        m_Input.shoot = false;
    }
}