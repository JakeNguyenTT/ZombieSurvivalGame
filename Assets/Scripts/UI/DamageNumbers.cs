using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Floating damage numbers drawn on the HUD canvas. Fixed pool; when it runs out the oldest is reused.
public class DamageNumbers : MonoBehaviour
{
    private const int PoolSize = 40;
    private const float Lifetime = 0.6f;
    private const float RiseDistance = 60f; // canvas units
    private const float BigHitThreshold = 50f;
    private static readonly Color NormalColor = Color.white;
    private static readonly Color BigColor = new Color(1f, 0.85f, 0.2f, 1f);

    private static DamageNumbers s_Instance;

    private class Entry
    {
        public TextMeshProUGUI Text;
        public Vector3 WorldPosition;
        public float Age;
    }

    private readonly List<Entry> m_Entries = new List<Entry>();
    private int m_Next;
    private Camera m_Camera;
    private RectTransform m_Root;

    public void Init(RectTransform root, TMP_FontAsset font)
    {
        s_Instance = this;
        m_Root = root;
        m_Camera = Camera.main;
        for (int i = 0; i < PoolSize; i++)
        {
            var text = RuntimeUI.CreateText(root, "DamageNumber", font, 36, TextAlignmentOptions.Center, NormalColor);
            RuntimeUI.Place(text.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(200, 60));
            text.gameObject.SetActive(false);
            m_Entries.Add(new Entry { Text = text, Age = Lifetime });
        }
    }

    public static void Show(Vector3 worldPosition, float amount)
    {
        if (s_Instance == null) return;
        s_Instance.ShowInternal(worldPosition, amount);
    }

    private void ShowInternal(Vector3 worldPosition, float amount)
    {
        Entry entry = m_Entries[m_Next];
        m_Next = (m_Next + 1) % m_Entries.Count;

        bool big = amount >= BigHitThreshold;
        entry.WorldPosition = worldPosition + new Vector3(Random.Range(-0.3f, 0.3f), 0, Random.Range(-0.3f, 0.3f));
        entry.Age = 0;
        entry.Text.text = Mathf.RoundToInt(amount).ToString();
        entry.Text.fontSize = big ? 52 : 36;
        entry.Text.color = big ? BigColor : NormalColor;
        entry.Text.gameObject.SetActive(true);
        Place(entry);
    }

    void LateUpdate()
    {
        foreach (var entry in m_Entries)
        {
            if (!entry.Text.gameObject.activeSelf) continue;
            entry.Age += Time.deltaTime;
            if (entry.Age >= Lifetime)
            {
                entry.Text.gameObject.SetActive(false);
                continue;
            }
            Place(entry);
        }
    }

    private void Place(Entry entry)
    {
        if (m_Camera == null) m_Camera = Camera.main;
        Vector3 screen = m_Camera.WorldToScreenPoint(entry.WorldPosition);
        if (screen.z < 0)
        {
            entry.Text.gameObject.SetActive(false);
            return;
        }
        float scale = m_Root.lossyScale.x > 0 ? m_Root.lossyScale.x : 1f;
        float t = entry.Age / Lifetime;
        Vector2 center = new Vector2(Screen.width, Screen.height) * 0.5f;
        entry.Text.rectTransform.anchoredPosition = ((Vector2)screen - center) / scale + Vector2.up * RiseDistance * t;
        entry.Text.alpha = 1f - t * t;
    }
}
