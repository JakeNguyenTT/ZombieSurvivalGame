using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Small helpers for building UI from code, reusing the scene's fonts and button styles.
public static class RuntimeUI
{
    public static RectTransform CreateRect(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = parent.gameObject.layer;
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        return rect;
    }

    public static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    public static void Place(RectTransform rect, Vector2 anchor, Vector2 anchoredPosition, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
    }

    public static Image CreatePanel(Transform parent, string name, Color color)
    {
        var image = CreateRect(parent, name).gameObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    public static TextMeshProUGUI CreateText(Transform parent, string name, TMP_FontAsset font, float size,
        TextAlignmentOptions alignment, Color color)
    {
        var text = CreateRect(parent, name).gameObject.AddComponent<TextMeshProUGUI>();
        if (font != null) text.font = font;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        text.text = "";
        return text;
    }

    // Clones an existing styled button, dropping its inspector-assigned click handlers.
    public static Button CloneButton(Button template, Transform parent, string label, UnityAction onClick)
    {
        Button button = Object.Instantiate(template, parent);
        button.name = label;
        button.onClick = new Button.ButtonClickedEvent();
        if (onClick != null) button.onClick.AddListener(onClick);
        SetLabel(button, label);
        button.interactable = true;
        button.gameObject.SetActive(true);
        return button;
    }

    public static void SetLabel(Button button, string label)
    {
        var text = button.GetComponentInChildren<TMP_Text>(true);
        if (text != null) text.text = label;
    }

    public static LayoutElement SetPreferredSize(Component component, float width, float height)
    {
        var element = component.GetComponent<LayoutElement>();
        if (element == null) element = component.gameObject.AddComponent<LayoutElement>();
        element.preferredWidth = width;
        element.preferredHeight = height;
        return element;
    }

    public static VerticalLayoutGroup AddVerticalLayout(GameObject go, float spacing, int padding)
    {
        var layout = go.AddComponent<VerticalLayoutGroup>();
        layout.spacing = spacing;
        layout.padding = new RectOffset(padding, padding, padding, padding);
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        return layout;
    }

    public static HorizontalLayoutGroup AddHorizontalLayout(GameObject go, float spacing)
    {
        var layout = go.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = spacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        return layout;
    }

    public static string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return $"{minutes:00}:{seconds:00}";
    }
}
