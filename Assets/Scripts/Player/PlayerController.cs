using StarterAssets;
using UnityEngine;

[RequireComponent(typeof(StarterAssetsInputs))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private WeaponSystem m_WeaponSystem;
    [SerializeField] private PlayerManager m_PlayerManager;
    [Tooltip("On-screen joysticks, created only on phones/tablets")]
    [SerializeField] private GameObject m_MobileControlsPrefab;
    private StarterAssetsInputs m_Input;

    private void Awake()
    {
        m_Input = GetComponent<StarterAssetsInputs>();
    }

    private void Start()
    {
        if (Application.isMobilePlatform && m_MobileControlsPrefab != null)
        {
            GameObject controls = Instantiate(m_MobileControlsPrefab);
            var canvasInput = controls.GetComponentInChildren<UICanvasControllerInput>(true);
            if (canvasInput != null) canvasInput.starterAssetsInputs = m_Input;
        }
    }

    void Update()
    {
        if (!GameManager.Instance.IsPlaying) return;
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