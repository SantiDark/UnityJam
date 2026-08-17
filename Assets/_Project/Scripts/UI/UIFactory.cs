using UnityEngine;
using UnityEngine.UI;

namespace Subject626
{
    /// <summary>Helpers para crear UI uGUI por codigo (sin prefabs ni TMP).</summary>
    public static class UIFactory
    {
        static Font font;
        public static Font Font
        {
            get
            {
                if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                if (font == null) font = Font.CreateDynamicFontFromOSFont("Arial", 14);
                return font;
            }
        }

        public static Canvas Canvas(string name, int sort)
        {
            GameObject go = new GameObject(name);
            Canvas c = go.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = sort;
            CanvasScaler s = go.AddComponent<CanvasScaler>();
            s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            s.referenceResolution = new Vector2(1920f, 1080f);
            s.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return c;
        }

        public static RectTransform Rect(Transform parent, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            return rt;
        }

        public static Image Panel(Transform parent, Color color, Vector2 size, Vector2 pos, Vector2 anchor)
        {
            RectTransform rt = Rect(parent, "Panel");
            rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = anchor;
            rt.sizeDelta = size; rt.anchoredPosition = pos;
            Image img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            return img;
        }

        public static Image Stretch(Transform parent, Color color)
        {
            RectTransform rt = Rect(parent, "Stretch");
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            Image img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            return img;
        }

        public static Text Label(Transform parent, string text, Vector2 size, Vector2 pos, Vector2 anchor, int fontSize, FontStyle style, TextAnchor align, Color color)
        {
            RectTransform rt = Rect(parent, "Text");
            rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = anchor;
            rt.sizeDelta = size; rt.anchoredPosition = pos;
            Text tx = rt.gameObject.AddComponent<Text>();
            tx.font = Font;
            tx.text = text;
            tx.fontSize = fontSize;
            tx.fontStyle = style;
            tx.alignment = align;
            tx.color = color;
            tx.horizontalOverflow = HorizontalWrapMode.Wrap;
            tx.verticalOverflow = VerticalWrapMode.Overflow;
            return tx;
        }
    }
}
