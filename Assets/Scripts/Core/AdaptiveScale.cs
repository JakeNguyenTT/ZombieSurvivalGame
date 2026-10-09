using UnityEngine;

// Render-scale steps used by AdaptiveQuality on phones: below ~50 fps drop resolution,
// holding ~58+ fps raise it back, never outside [MinScale, maxScale].
public static class AdaptiveScale
{
    public const float SlowFrame = 1f / 50f;
    public const float FastFrame = 1f / 58f;
    public const float Step = 0.1f;
    public const float MinScale = 0.6f;

    public static float Next(float currentScale, float averageFrameTime, float maxScale)
    {
        if (averageFrameTime > SlowFrame) return Mathf.Max(MinScale, currentScale - Step);
        if (averageFrameTime < FastFrame) return Mathf.Min(maxScale, currentScale + Step);
        return currentScale;
    }
}
