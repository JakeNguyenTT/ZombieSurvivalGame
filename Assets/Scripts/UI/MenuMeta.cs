using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Menu screen extras built at runtime from the Start button's style:
// coins and personal bests, plus Shop / Character / Settings panels.
public class MenuMeta : MonoBehaviour
{
    private static readonly Color OverlayColor = new Color(0f, 0f, 0f, 0.7f);
    private static readonly Color BoxColor = new Color(0.08f, 0.08f, 0.1f, 0.95f);
    private const float RowLabelWidth = 560f;
    private const float RowButtonWidth = 300f;
    private const float RowHeight = 80f;

    private Button m_Template;
    private RectTransform m_Root;
    private TMP_FontAsset m_Font;
    private TextMeshProUGUI m_InfoText;
    private GameObject m_OpenPanel;
    // Refreshes the open panel's rows after anything changes (coins, levels, selection, volume)
    private readonly List<Action> m_Refreshers = new List<Action>();

    public void Init(Button template)
    {
        m_Template = template;
        m_Root = (RectTransform)template.GetComponentInParent<Canvas>().rootCanvas.transform;
        TMP_Text templateLabel = template.GetComponentInChildren<TMP_Text>(true);
        m_Font = templateLabel != null ? templateLabel.font : null;

        // Bottom-left corner: the title owns the top of the screen
        m_InfoText = RuntimeUI.CreateText(m_Root, "MetaInfo", m_Font, 40, TextAlignmentOptions.BottomLeft, Color.white);
        RectTransform infoRect = m_InfoText.rectTransform;
        infoRect.anchorMin = infoRect.anchorMax = Vector2.zero;
        infoRect.pivot = Vector2.zero;
        infoRect.anchoredPosition = new Vector2(40, 40);
        infoRect.sizeDelta = new Vector2(900, 140);

        // Row of buttons just under the centered Start button
        RectTransform bar = RuntimeUI.CreateRect(m_Root, "MetaButtons");
        RuntimeUI.Place(bar, new Vector2(0.5f, 0.5f), new Vector2(0, -170), new Vector2(1100, 100));
        RuntimeUI.AddHorizontalLayout(bar.gameObject, 30);
        AddBarButton(bar, "Shop", BuildShop);
        AddBarButton(bar, "Character", BuildCharacters);
        AddBarButton(bar, "Settings", BuildSettings);

        RefreshAll();
    }

    private void AddBarButton(Transform bar, string label, Action open)
    {
        Button button = RuntimeUI.CloneButton(m_Template, bar, label, () => open());
        RuntimeUI.SetPreferredSize(button, RowButtonWidth, 90);
    }

    private void RefreshAll()
    {
        foreach (var refresh in m_Refreshers) refresh();
        string info = $"Coins: {SaveData.Coins}";
        if (SaveData.BestTime > 0)
            info += $"\nBest: {RuntimeUI.FormatTime(SaveData.BestTime)}  |  {SaveData.BestKills} kills";
        m_InfoText.text = info;
    }

    // ---------- Panels ----------

    private RectTransform OpenPanel(string title)
    {
        ClosePanel();
        Image overlay = RuntimeUI.CreatePanel(m_Root, title + "Panel", OverlayColor);
        RuntimeUI.Stretch(overlay.rectTransform);
        m_OpenPanel = overlay.gameObject;

        Image box = RuntimeUI.CreatePanel(overlay.transform, "Box", BoxColor);
        RuntimeUI.Place(box.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1000, 760));
        RuntimeUI.AddVerticalLayout(box.gameObject, 24, 40);

        TextMeshProUGUI titleText = AddText(box.transform, title, 64, TextAlignmentOptions.Center);
        RuntimeUI.SetPreferredSize(titleText, 900, 90);

        RectTransform content = RuntimeUI.CreateRect(box.transform, "Content");
        RuntimeUI.AddVerticalLayout(content.gameObject, 16, 0);
        RuntimeUI.SetPreferredSize(content, 900, 440);

        Button close = RuntimeUI.CloneButton(m_Template, box.transform, "Close", ClosePanel);
        RuntimeUI.SetPreferredSize(close, RowButtonWidth, 90);
        return content;
    }

    private void ClosePanel()
    {
        if (m_OpenPanel != null) Destroy(m_OpenPanel);
        m_OpenPanel = null;
        m_Refreshers.Clear();
    }

    private void BuildShop()
    {
        RectTransform content = OpenPanel("Shop");
        foreach (MetaStat value in Enum.GetValues(typeof(MetaStat)))
        {
            MetaStat stat = value;
            RectTransform row = AddRow(content);
            TextMeshProUGUI label = AddText(row, "", 34, TextAlignmentOptions.MidlineLeft);
            RuntimeUI.SetPreferredSize(label, RowLabelWidth, RowHeight);
            Button buy = RuntimeUI.CloneButton(m_Template, row, "", () =>
            {
                if (SaveData.TryBuyMeta(stat)) RefreshAll();
            });
            RuntimeUI.SetPreferredSize(buy, RowButtonWidth, RowHeight);

            m_Refreshers.Add(() =>
            {
                int level = SaveData.GetMetaLevel(stat);
                bool maxed = level >= MetaUpgrades.MaxLevel;
                int cost = MetaUpgrades.Cost(level);
                label.text = $"{MetaUpgrades.Label(stat)}  Lv {level}/{MetaUpgrades.MaxLevel}  ({MetaUpgrades.FormatBonus(stat, level)})";
                RuntimeUI.SetLabel(buy, maxed ? "MAX" : $"Buy {cost}");
                buy.interactable = !maxed && SaveData.Coins >= cost;
            });
        }
        RefreshAll();
    }

    private void BuildCharacters()
    {
        RectTransform content = OpenPanel("Character");
        foreach (CharacterData value in CharacterRoster.All())
        {
            CharacterData character = value;
            RectTransform row = AddRow(content);
            TextMeshProUGUI label = AddText(row,
                $"{character.displayName}\n<size=70%>{character.description}</size>", 38, TextAlignmentOptions.MidlineLeft);
            RuntimeUI.SetPreferredSize(label, RowLabelWidth, 110);
            Button select = RuntimeUI.CloneButton(m_Template, row, "", () =>
            {
                SaveData.SelectedCharacter = character.name;
                RefreshAll();
            });
            RuntimeUI.SetPreferredSize(select, RowButtonWidth, RowHeight);

            m_Refreshers.Add(() =>
            {
                bool selected = CharacterRoster.Selected(null) == character;
                RuntimeUI.SetLabel(select, selected ? "Selected" : "Select");
                select.interactable = !selected;
            });
        }
        RefreshAll();
    }

    private void BuildSettings()
    {
        RectTransform content = OpenPanel("Settings");
        AudioManager audio = AudioManager.Instance;
        if (audio == null) return;
        AddVolumeRow(content, "Music", () => audio.MusicVolume, audio.SetMusicVolume);
        AddVolumeRow(content, "Sound", () => audio.SfxVolume, audio.SetSFXVolume);
        RefreshAll();
    }

    private void AddVolumeRow(Transform content, string name, Func<float> get, Action<float> set)
    {
        RectTransform row = AddRow(content);
        TextMeshProUGUI label = AddText(row, "", 40, TextAlignmentOptions.MidlineLeft);
        RuntimeUI.SetPreferredSize(label, 420, RowHeight);
        Button minus = RuntimeUI.CloneButton(m_Template, row, "-", () => { set(Step(get(), -1)); RefreshAll(); });
        RuntimeUI.SetPreferredSize(minus, 140, RowHeight);
        Button plus = RuntimeUI.CloneButton(m_Template, row, "+", () => { set(Step(get(), +1)); RefreshAll(); });
        RuntimeUI.SetPreferredSize(plus, 140, RowHeight);
        m_Refreshers.Add(() => label.text = $"{name}: {Mathf.RoundToInt(get() * 100)}%");
    }

    // 10% steps, snapped so repeated presses land on round numbers
    private static float Step(float volume, int direction) => Mathf.Clamp01((Mathf.Round(volume * 10f) + direction) / 10f);

    // ---------- Helpers ----------

    private RectTransform AddRow(Transform parent)
    {
        RectTransform row = RuntimeUI.CreateRect(parent, "Row");
        RuntimeUI.AddHorizontalLayout(row.gameObject, 20);
        return row;
    }

    private TextMeshProUGUI AddText(Transform parent, string text, float size, TextAlignmentOptions alignment)
    {
        TextMeshProUGUI label = RuntimeUI.CreateText(parent, "Text", m_Font, size, alignment, Color.white);
        label.text = text;
        return label;
    }
}
