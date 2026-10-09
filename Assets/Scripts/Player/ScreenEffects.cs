using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Full-screen feedback through a runtime URP Volume layered over the scene's profile:
// red vignette that pulses at low health and spikes on hits, a brightness flash on explosions
// and chromatic aberration when a boss arrives. Only the parameters used here are overridden.
public class ScreenEffects : MonoBehaviour
{
    private const float LowHealthThreshold = 0.35f;
    private const float LowHealthMaxVignette = 0.45f;
    private const float HitVignette = 0.35f;
    private const float HitDuration = 0.25f;
    private const float ExplosionExposure = 0.8f;
    private const float ExplosionDuration = 0.2f;
    private const float BossAberration = 1f;
    private const float BossDuration = 0.6f;
    private static readonly Color DamageColor = new Color(0.6f, 0f, 0f, 1f);

    public static ScreenEffects Instance { get; private set; }

    private VolumeProfile m_Profile;
    private Vignette m_Vignette;
    private ColorAdjustments m_ColorAdjustments;
    private ChromaticAberration m_Aberration;
    private float m_HealthFraction = 1f;
    private float m_HitTimer;
    private float m_ExplosionTimer;
    private float m_BossTimer;

    void Awake()
    {
        Instance = this;

        m_Profile = ScriptableObject.CreateInstance<VolumeProfile>();
        m_Vignette = m_Profile.Add<Vignette>();
        m_Vignette.color.Override(DamageColor);
        m_Vignette.smoothness.Override(0.6f);
        m_Vignette.intensity.Override(0f);
        m_ColorAdjustments = m_Profile.Add<ColorAdjustments>();
        m_ColorAdjustments.postExposure.Override(0f);
        m_Aberration = m_Profile.Add<ChromaticAberration>();
        m_Aberration.intensity.Override(0f);

        var volume = gameObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 100f;
        volume.sharedProfile = m_Profile;

        // The scene camera ships with post-processing off
        Camera camera = Camera.main;
        if (camera != null && camera.TryGetComponent(out UniversalAdditionalCameraData cameraData))
            cameraData.renderPostProcessing = true;
    }

    void OnDestroy()
    {
        if (m_Profile != null) Destroy(m_Profile);
    }

    public void SetHealth(float current, float max) => m_HealthFraction = max > 0 ? Mathf.Clamp01(current / max) : 0f;

    public void PlayHit() => m_HitTimer = HitDuration;

    public void PlayExplosion() => m_ExplosionTimer = ExplosionDuration;

    public void PlayBossArrival() => m_BossTimer = BossDuration;

    void Update()
    {
        // Unscaled so a hit right before game over / level-up still fades out
        float dt = Time.unscaledDeltaTime;
        m_HitTimer = Mathf.Max(0f, m_HitTimer - dt);
        m_ExplosionTimer = Mathf.Max(0f, m_ExplosionTimer - dt);
        m_BossTimer = Mathf.Max(0f, m_BossTimer - dt);

        float lowHealth = 0f;
        if (m_HealthFraction > 0f && m_HealthFraction < LowHealthThreshold)
        {
            float severity = 1f - m_HealthFraction / LowHealthThreshold;          // 0 at threshold, 1 near death
            float pulse = 0.75f + 0.25f * Mathf.Sin(Time.unscaledTime * (4f + 4f * severity));
            lowHealth = LowHealthMaxVignette * severity * pulse;
        }
        float hit = HitVignette * (m_HitTimer / HitDuration);
        m_Vignette.intensity.value = Mathf.Clamp01(Mathf.Max(lowHealth, hit));
        m_ColorAdjustments.postExposure.value = ExplosionExposure * (m_ExplosionTimer / ExplosionDuration);
        m_Aberration.intensity.value = BossAberration * (m_BossTimer / BossDuration);
    }
}
