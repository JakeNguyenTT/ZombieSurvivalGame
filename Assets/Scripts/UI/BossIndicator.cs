using TMPro;
using UnityEngine;

// "BOSS INCOMING" banner when a boss spawns, plus a marker on the screen edge pointing
// at the boss while it is off-screen.
public class BossIndicator : MonoBehaviour
{
    private const float EdgeMargin = 70f;
    private const float BannerDuration = 2.5f;
    private static readonly Color BossColor = new Color(0.9f, 0.15f, 0.15f, 1f);

    private RectTransform m_Root;
    private Camera m_Camera;
    private TextMeshProUGUI m_Banner;
    private RectTransform m_Marker;
    private float m_BannerTimer;

    public void Init(RectTransform root, TMP_FontAsset font)
    {
        m_Root = root;
        m_Camera = Camera.main;

        m_Banner = RuntimeUI.CreateText(root, "BossBanner", font, 80, TextAlignmentOptions.Center, BossColor);
        RuntimeUI.Place(m_Banner.rectTransform, new Vector2(0.5f, 0.75f), Vector2.zero, new Vector2(1400, 140));
        m_Banner.text = "BOSS INCOMING!";
        m_Banner.gameObject.SetActive(false);

        m_Marker = RuntimeUI.CreateRect(root, "BossMarker");
        RuntimeUI.Place(m_Marker, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(60, 60));
        var diamond = RuntimeUI.CreatePanel(m_Marker, "Diamond", BossColor);
        RuntimeUI.Place(diamond.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(40, 40));
        diamond.rectTransform.localRotation = Quaternion.Euler(0, 0, 45);
        diamond.raycastTarget = false;
        var label = RuntimeUI.CreateText(m_Marker, "Label", font, 30, TextAlignmentOptions.Center, Color.white);
        RuntimeUI.Place(label.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -45), new Vector2(160, 40));
        label.text = "BOSS";
        m_Marker.gameObject.SetActive(false);

        EnemySpawner.Instance.OnBossSpawned += HandleBossSpawned;
    }

    void OnDestroy()
    {
        if (EnemySpawner.Instance != null)
            EnemySpawner.Instance.OnBossSpawned -= HandleBossSpawned;
    }

    private void HandleBossSpawned(EnemyBehavior boss)
    {
        m_BannerTimer = BannerDuration;
        m_Banner.gameObject.SetActive(true);
    }

    void Update()
    {
        if (m_Banner == null) return;
        UpdateBanner();
        UpdateMarker();
    }

    private void UpdateBanner()
    {
        if (m_BannerTimer <= 0) return;
        m_BannerTimer -= Time.deltaTime;
        // Pulse while visible, fade out over the last half second
        float pulse = 0.75f + 0.25f * Mathf.Sin(Time.time * 12f);
        float fade = Mathf.Clamp01(m_BannerTimer / 0.5f);
        m_Banner.alpha = pulse * fade;
        if (m_BannerTimer <= 0) m_Banner.gameObject.SetActive(false);
    }

    private void UpdateMarker()
    {
        EnemyBehavior boss = EnemySpawner.Instance.ActiveBoss;
        if (boss == null || !boss.IsAlive || m_Camera == null)
        {
            m_Marker.gameObject.SetActive(false);
            return;
        }

        Vector3 screen = m_Camera.WorldToScreenPoint(boss.transform.position + Vector3.up * 2f);
        bool behind = screen.z < 0;
        if (behind) screen = -screen; // mirror so the marker points the right way
        bool onScreen = !behind && screen.x >= 0 && screen.x <= Screen.width && screen.y >= 0 && screen.y <= Screen.height;
        m_Marker.gameObject.SetActive(!onScreen);
        if (onScreen) return;

        float scale = m_Root.lossyScale.x > 0 ? m_Root.lossyScale.x : 1f; // canvas scale factor
        Vector2 center = new Vector2(Screen.width, Screen.height) * 0.5f;
        Vector2 direction = (Vector2)screen - center;
        if (direction.sqrMagnitude < 1f) direction = Vector2.up;
        Vector2 halfExtent = center - Vector2.one * EdgeMargin * scale;
        float tx = halfExtent.x / Mathf.Max(Mathf.Abs(direction.x), 0.001f);
        float ty = halfExtent.y / Mathf.Max(Mathf.Abs(direction.y), 0.001f);
        Vector2 edge = direction * Mathf.Min(tx, ty);
        m_Marker.anchoredPosition = edge / scale;
    }
}
