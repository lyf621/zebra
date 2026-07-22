using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 集中保存正式游戏 UI 的颜色、字体与常用控件样式，避免各面板各自定义外观。
public static class GameUITheme
{
    public static readonly Color DeepGreen = new Color(0.08f, 0.15f, 0.13f, 0.98f);
    public static readonly Color ForestGreen = new Color(0.16f, 0.31f, 0.24f, 1f);
    public static readonly Color Gold = new Color(0.72f, 0.53f, 0.18f, 1f);
    public static readonly Color Parchment = new Color(0.91f, 0.85f, 0.70f, 1f);
    public static readonly Color Ink = new Color(0.12f, 0.10f, 0.07f, 1f);

    private static Font sLegacyFont;
    private static TMP_FontAsset sTmpFont;
    private static Sprite sPaperSprite;

    // 返回支持中英文的 Noto Sans SC 字体，供运行时创建的旧版 UI Text 使用。
    public static Font GetLegacyFont()
    {
        if (sLegacyFont == null)
        {
            sLegacyFont = Resources.Load<Font>("Fonts/NotoSansSC");
        }
        return sLegacyFont;
    }

    // 从同一字体生成动态 TMP 字体，使场景中已有的 TextMeshPro 文本保持统一。
    public static TMP_FontAsset GetTmpFont()
    {
        if (sTmpFont == null && GetLegacyFont() != null)
        {
            sTmpFont = TMP_FontAsset.CreateFontAsset(sLegacyFont);
        }
        return sTmpFont;
    }

    // 将 CC0 纸张纹理转换为可供 Image 使用的 Sprite，并在运行期间复用。
    public static Sprite GetPaperSprite()
    {
        if (sPaperSprite == null)
        {
            Texture2D texture = Resources.Load<Texture2D>("Art/PaperBackground");
            if (texture != null)
            {
                sPaperSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            }
        }
        return sPaperSprite;
    }

    // 为场景按钮和运行时生成的选项按钮应用相同的颜色、边框、字体和高度。
    public static void StyleButton(Button button)
    {
        if (button == null)
        {
            return;
        }

        Image image = button.GetComponent<Image>();
        if (image != null)
        {
            image.color = ForestGreen;
        }

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.16f, 1.12f, 0.94f, 1f);
        colors.pressedColor = new Color(0.72f, 0.72f, 0.66f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.38f, 0.39f, 0.36f, 0.65f);
        button.colors = colors;

        Outline outline = button.GetComponent<Outline>();
        if (outline == null)
        {
            outline = button.gameObject.AddComponent<Outline>();
        }
        outline.effectColor = Gold;
        outline.effectDistance = new Vector2(2f, -2f);

        LayoutElement layout = button.GetComponent<LayoutElement>();
        if (layout == null)
        {
            layout = button.gameObject.AddComponent<LayoutElement>();
        }
        layout.preferredHeight = 44f;
        layout.minHeight = 40f;

        TMP_Text tmpText = button.GetComponentInChildren<TMP_Text>(true);
        if (tmpText != null)
        {
            if (GetTmpFont() != null) tmpText.font = sTmpFont;
            tmpText.color = Color.white;
            tmpText.fontSize = 17f;
            tmpText.fontStyle = FontStyles.Bold;
        }

        Text legacyText = button.GetComponentInChildren<Text>(true);
        if (legacyText != null)
        {
            if (GetLegacyFont() != null) legacyText.font = sLegacyFont;
            legacyText.color = Color.white;
            legacyText.fontSize = 17;
            legacyText.fontStyle = FontStyle.Bold;
        }
    }

    // 为 TMP 文本应用统一字体与字号，但保留 Inspector 中设置的颜色（不再在代码里强制改色）。
    public static void StyleTmpText(TMP_Text text, float fontSize, FontStyles style)
    {
        if (text == null)
        {
            return;
        }
        if (GetTmpFont() != null) text.font = sTmpFont;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.textWrappingMode = TextWrappingModes.Normal;
    }
}
