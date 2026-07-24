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
    private static Sprite sBurgundyButtonSprite;
    private static Sprite sCardFrameSprite;
    private static Sprite sCardFrameOverlaySprite;
    private static Sprite sCardFaceSprite;
    private static Sprite sRoyalCardFaceSprite;
    private static Sprite sCardBackSprite;
    private static Sprite sCountBadgeSprite;

    // 返回支持中英文的 Noto Sans SC 字体，供运行时创建的旧版 UI Text 使用。
    public static Font GetLegacyFont()
    {
        if (sLegacyFont == null)
        {
            sLegacyFont = Resources.Load<Font>("Fonts/LXGWWenKai-Regular");
            if (sLegacyFont == null)
            {
                sLegacyFont = Resources.Load<Font>("Fonts/NotoSansSC");
            }
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
            Texture2D texture = Resources.Load<Texture2D>("Art/OldPaperBackground");
            if (texture != null)
            {
                sPaperSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            }
        }
        return sPaperSprite;
    }

    public static Sprite GetCardFrameSprite()
    {
        if (sCardFrameSprite == null)
        {
            Texture2D texture = Resources.Load<Texture2D>("Art/CardUI/GoldCardFrame");
            if (texture != null)
            {
                sCardFrameSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            }
        }
        return sCardFrameSprite;
    }

    public static Sprite GetCardFrameOverlaySprite()
    {
        if (sCardFrameOverlaySprite == null)
        {
            Texture2D texture = Resources.Load<Texture2D>("Art/CardUI/GoldCardFrameOverlay");
            if (texture != null)
            {
                sCardFrameOverlaySprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            }
        }
        return sCardFrameOverlaySprite;
    }

    public static Sprite GetCardFaceSprite(bool royal)
    {
        if (royal)
        {
            if (sRoyalCardFaceSprite == null)
            {
                Texture2D royalTexture = Resources.Load<Texture2D>("Art/CardUI/royal card");
                if (royalTexture != null)
                {
                    sRoyalCardFaceSprite = Sprite.Create(royalTexture, new Rect(0f, 0f, royalTexture.width, royalTexture.height), new Vector2(0.5f, 0.5f), 100f);
                }
            }
            return sRoyalCardFaceSprite;
        }

        if (sCardFaceSprite == null)
        {
            Texture2D texture = Resources.Load<Texture2D>("Art/CardUI/normal card");
            if (texture != null)
            {
                sCardFaceSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            }
        }
        return sCardFaceSprite;
    }

    public static Sprite GetCardBackSprite()
    {
        if (sCardBackSprite == null)
        {
            Texture2D texture = Resources.Load<Texture2D>("Art/CardUI/BlackGoldCardBack");
            if (texture != null)
            {
                sCardBackSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            }
        }
        return sCardBackSprite;
    }

    private static Sprite GetBurgundyButtonSprite()
    {
        if (sBurgundyButtonSprite == null)
        {
            Texture2D texture = Resources.Load<Texture2D>("Art/CardUI/BurgundyButtonStrip");
            if (texture != null)
            {
                float rowHeight = texture.height / 3f;
                sBurgundyButtonSprite = Sprite.Create(
                    texture,
                    new Rect(0f, rowHeight * 2f, texture.width, rowHeight),
                    new Vector2(0.5f, 0.5f),
                    100f,
                    0u,
                    SpriteMeshType.FullRect,
                    new Vector4(24f, 12f, 24f, 12f));
            }
        }
        return sBurgundyButtonSprite;
    }

    public static void StyleCardBack(Image image)
    {
        if (image == null) return;
        image.sprite = GetCardBackSprite();
        image.type = Image.Type.Simple;
        image.preserveAspect = true;
        image.color = Color.white;
    }

    public static Sprite GetCountBadgeSprite()
    {
        if (sCountBadgeSprite == null)
        {
            Texture2D texture = Resources.Load<Texture2D>("Art/CardUI/CountBadge");
            if (texture != null)
            {
                sCountBadgeSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            }
        }
        return sCountBadgeSprite;
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
            image.sprite = GetBurgundyButtonSprite();
            image.type = Image.Type.Sliced;
            image.preserveAspect = false;
            image.color = Color.white;
        }

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.16f, 1.12f, 0.94f, 1f);
        colors.pressedColor = new Color(0.72f, 0.72f, 0.66f, 1f);
        colors.selectedColor = colors.highlightedColor;
        // Keep disabled commands visibly present as darkened lacquer, not faded away.
        colors.disabledColor = new Color(0.42f, 0.42f, 0.42f, 1f);
        button.colors = colors;

        Outline outline = button.GetComponent<Outline>();
        if (outline != null) outline.enabled = false;

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
            tmpText.color = new Color(0.95f, 0.79f, 0.37f);
            tmpText.fontSize = 17f;
            tmpText.fontStyle = FontStyles.Bold;
        }

        Text legacyText = button.GetComponentInChildren<Text>(true);
        if (legacyText != null)
        {
            if (GetLegacyFont() != null) legacyText.font = sLegacyFont;
            legacyText.color = new Color(0.95f, 0.79f, 0.37f);
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
