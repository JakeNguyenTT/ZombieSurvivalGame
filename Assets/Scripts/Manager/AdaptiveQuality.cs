using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Phones and tablets only: trades resolution for frame rate. Every few seconds it compares the
// average frame time with the 60 fps target and moves the URP render scale down (to 0.6) or back
// up (to the asset's own value) in small steps. The mobile URP asset upscales with STP, so lower
// scales stay sharp. Installed automatically once per app run.
public class AdaptiveQuality : MonoBehaviour
{
    private const int TargetFrameRate = 60;
    private const float SampleWindow = 3f;

    private UniversalRenderPipelineAsset m_Asset;
    private float m_MaxScale;
    private float m_Elapsed;
    private int m_Frames;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        if (!Application.isMobilePlatform) return;
        // Android defaults to 30 fps; aim for 60 and let render scale absorb the cost
        Application.targetFrameRate = TargetFrameRate;
        new GameObject(nameof(AdaptiveQuality)).AddComponent<AdaptiveQuality>();
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        m_Asset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (m_Asset == null)
        {
            enabled = false;
            return;
        }
        m_MaxScale = m_Asset.renderScale;
    }

    void Update()
    {
        m_Elapsed += Time.unscaledDeltaTime;
        m_Frames++;
        if (m_Elapsed < SampleWindow) return;

        float averageFrame = m_Elapsed / m_Frames;
        m_Elapsed = 0f;
        m_Frames = 0;

        m_Asset.renderScale = AdaptiveScale.Next(m_Asset.renderScale, averageFrame, m_MaxScale);
    }

    // Never leave the shared asset changed
    void OnDestroy()
    {
        if (m_Asset != null) m_Asset.renderScale = m_MaxScale;
    }
}
