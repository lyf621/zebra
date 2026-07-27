using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A standalone Credits panel for the MainMenu scene.
///
/// Built entirely in code on its own overlay canvas, exactly like MainMenuSettings, so it
/// shares that panel's art: the same parchment surface, the same LXGWWenKai/NotoSansSC face
/// from GameUITheme, and the same burgundy lacquer buttons via GameUITheme.StyleButton.
/// Text follows GameSessionSettings.UseChinese, the same language flag the Settings panel
/// writes and the game reads on start.
///
/// Setup in Unity:
///   1. Add this component to any object in MainMenu (the existing "MainMenuSettings" object
///      is a natural home, or use the Canvas).
///   2. On the "Credits" button, add an OnClick entry -> drag that object in -> choose
///      MainMenuCredits.OpenCredits().
///   3. Fill in the Entries list in the Inspector. Leave a Chinese field blank to fall back
///      to its English counterpart.
/// </summary>
public class MainMenuCredits : MonoBehaviour
{
    [System.Serializable]
    public class CreditEntry
    {
        [Tooltip("Role heading, e.g. \"Programming\".")]
        public string RoleEnglish = "Role";
        [Tooltip("Optional. Blank falls back to the English heading.")]
        public string RoleChinese;
        [Tooltip("One or more names. Use line breaks for several people.")]
        [TextArea(1, 4)] public string Names = "Name";
        [Tooltip("Optional. Blank falls back to the English names.")]
        [TextArea(1, 4)] public string NamesChinese;

        public string GetRole(bool chinese)
        {
            return chinese && !string.IsNullOrEmpty(RoleChinese) ? RoleChinese : RoleEnglish;
        }

        public string GetNames(bool chinese)
        {
            return chinese && !string.IsNullOrEmpty(NamesChinese) ? NamesChinese : Names;
        }
    }

    [Header("Credits content (edit these in the Inspector)")]
    [SerializeField]
    private CreditEntry[] entries =
    {
        new CreditEntry { RoleEnglish = "Design",      RoleChinese = "设计", Names = "Your name here" },
        new CreditEntry { RoleEnglish = "Programming", RoleChinese = "程序", Names = "Your name here" },
        new CreditEntry { RoleEnglish = "Art",         RoleChinese = "美术", Names = "Your name here" },
        new CreditEntry { RoleEnglish = "Music",       RoleChinese = "音乐", Names = "Your name here" }
    };

    [Header("Optional closing line (blank to omit)")]
    [TextArea(1, 3)][SerializeField] private string footerEnglish = "Thank you for playing.";
    [TextArea(1, 3)][SerializeField] private string footerChinese = "感谢您的游玩。";

    [Header("Panel size")]
    [Tooltip("Widen this if your descriptions are long. The title, body and Close button all reposition themselves from it.")]
    [SerializeField] private Vector2 panelSize = new Vector2(760f, 520f);
    [Tooltip("Left-align description paragraphs. Long prose is much easier to read ragged-right than centred.")]
    [SerializeField] private bool leftAlignDescriptions = true;

    private GameObject mPanelRoot;
    private Font mFont;

    // Bind the MainMenu "Credits" button's OnClick to this.
    public void OpenCredits()
    {
        if (mPanelRoot != null) return;

        mFont = GameUITheme.GetLegacyFont();
        if (mFont == null) mFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Dedicated overlay canvas (matches the 1280x720 scaler the game UI is designed for).
        mPanelRoot = new GameObject("MainMenu Credits", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = mPanelRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 900;
        CanvasScaler scaler = mPanelRoot.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        // The full-screen dim is also what makes this panel modal: it swallows clicks that
        // would otherwise reach the menu buttons behind it.
        Image dim = mPanelRoot.AddComponent<Image>();
        dim.color = new Color(0.025f, 0.02f, 0.015f, 0.82f);

        BuildContents();
    }

    public void CloseCredits()
    {
        if (mPanelRoot != null) Destroy(mPanelRoot);
        mPanelRoot = null;
    }

    private void BuildContents()
    {
        bool chinese = GameSessionSettings.UseChinese;

        // Everything below is derived from panelSize, so resizing the panel in the Inspector
        // keeps the title, body and Close button correctly spaced.
        Vector2 half = panelSize * 0.5f;
        float titleY = half.y - 42f;
        float closeY = -half.y + 34f;
        Vector2 bodySize = new Vector2(panelSize.x - 60f, panelSize.y - 140f);

        Image panel = CreateImage("Panel", mPanelRoot.transform, new Vector2(0.5f, 0.5f), panelSize, new Color(0.88f, 0.84f, 0.72f, 1f));

        CreateText("Title", panel.transform, chinese ? "致谢" : "Credits", 28, FontStyle.Bold, new Vector2(0f, titleY), new Vector2(panelSize.x - 200f, 44f), GameUITheme.Ink);

        // Scrolling body, so any number of entries fits without re-tuning the panel size.
        RectTransform content = CreateScrollArea(panel.transform, new Vector2(0f, -4f), bodySize);

        TextAnchor descriptionAlign = leftAlignDescriptions ? TextAnchor.UpperLeft : TextAnchor.UpperCenter;

        if (entries != null)
        {
            foreach (CreditEntry entry in entries)
            {
                if (entry == null) continue;

                string role = entry.GetRole(chinese);
                if (!string.IsNullOrEmpty(role))
                    CreateRow(content, "Role", role, 20, FontStyle.Bold, GameUITheme.Ink, TextAnchor.UpperCenter);

                string names = entry.GetNames(chinese);
                if (!string.IsNullOrEmpty(names))
                    CreateRow(content, "Names", names, 17, FontStyle.Normal, new Color(0.26f, 0.22f, 0.16f), descriptionAlign);

                CreateSpacer(content, 10f);
            }
        }

        string footer = chinese ? footerChinese : footerEnglish;
        if (!string.IsNullOrEmpty(footer))
            CreateRow(content, "Footer", footer, 17, FontStyle.Italic, new Color(0.32f, 0.27f, 0.19f), TextAnchor.UpperCenter);

        CreateButton("Close", panel.transform, chinese ? "关闭" : "Close", new Vector2(0f, closeY), new Vector2(140f, 42f), new Color(0.32f, 0.3f, 0.27f))
            .onClick.AddListener(CloseCredits);

        SettleLayout(content);
    }

    /// <summary>
    /// A wrapped Text reports its preferred height from its CURRENT width, but the rows are
    /// created before the layout group has assigned them one. The first pass therefore measures
    /// against a placeholder width and returns a wrong height, which leaves long paragraphs
    /// overflowing their row and clipped by the viewport mask. Rebuilding a second time — once
    /// the widths are settled — makes the heights correct.
    /// </summary>
    private void SettleLayout(RectTransform content)
    {
        if (content == null) return;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);   // pass 1: assigns widths
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);   // pass 2: heights from those widths
    }

    // ---------------------------------------------------------------- helpers
    // Returns the content transform that rows are parented to.
    private RectTransform CreateScrollArea(Transform parent, Vector2 position, Vector2 size)
    {
        GameObject scrollObject = new GameObject("Scroll Area", typeof(RectTransform), typeof(ScrollRect));
        scrollObject.transform.SetParent(parent, false);
        RectTransform scrollRect = scrollObject.GetComponent<RectTransform>();
        scrollRect.anchorMin = scrollRect.anchorMax = new Vector2(0.5f, 0.5f);
        scrollRect.pivot = new Vector2(0.5f, 0.5f);
        scrollRect.anchoredPosition = position;
        scrollRect.sizeDelta = size;

        GameObject viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask));
        viewportObject.transform.SetParent(scrollObject.transform, false);
        RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;   // same reason as the content rect below
        viewportRect.anchoredPosition = Vector2.zero;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        viewportObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.01f);
        viewportObject.GetComponent<Mask>().showMaskGraphic = false;

        GameObject contentObject = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentObject.transform.SetParent(viewportObject.transform, false);
        RectTransform contentRect = contentObject.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        // MUST be explicit. With the anchors stretched horizontally, width = viewport width +
        // sizeDelta.x, and a raw RectTransform does not start at zero. Left alone, the content
        // ends up wider than the viewport, text wraps to that oversized width, and the mask
        // clips the overhang — words disappear off both edges. The ContentSizeFitter only
        // drives y, so x stays exactly as set here.
        contentRect.sizeDelta = Vector2.zero;

        VerticalLayoutGroup layout = contentObject.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(22, 22, 8, 8);   // keep text off the mask edge
        layout.spacing = 2f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        // The fitter lets the body grow with the entry list; the ScrollRect handles the rest.
        ContentSizeFitter fitter = contentObject.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scroll = scrollObject.GetComponent<ScrollRect>();
        scroll.content = contentRect;
        scroll.viewport = viewportRect;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 24f;

        return contentRect;
    }

    private void CreateRow(Transform parent, string name, string value, int size, FontStyle style, Color color, TextAnchor alignment)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(ContentSizeFitter));
        go.transform.SetParent(parent, false);
        Text text = go.GetComponent<Text>();
        text.font = mFont;
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;   // never spill past the panel edge
        text.verticalOverflow = VerticalWrapMode.Overflow;   // grow downwards instead of clipping
        text.lineSpacing = 1.15f;
        text.supportRichText = true;
        text.raycastTarget = false;

        // The row owns its height so a paragraph of any length gets the space it needs; the
        // parent VerticalLayoutGroup then stacks the measured rows and the ScrollRect scrolls
        // whatever does not fit on screen.
        ContentSizeFitter fitter = go.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private void CreateSpacer(Transform parent, float height)
    {
        GameObject go = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        go.GetComponent<LayoutElement>().minHeight = height;
    }

    private Image CreateImage(string name, Transform parent, Vector2 position, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Image image = go.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private Text CreateText(string name, Transform parent, string value, int size, FontStyle style, Vector2 position, Vector2 rectSize, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = rectSize;
        Text text = go.GetComponent<Text>();
        text.font = mFont;
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.supportRichText = true;
        text.raycastTarget = false;
        return text;
    }

    private Button CreateButton(string name, Transform parent, string label, Vector2 position, Vector2 size, Color color)
    {
        Image image = CreateImage(name, parent, position, size, color);
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        CreateText("Label", image.transform, label, 19, FontStyle.Bold, Vector2.zero, size - new Vector2(16f, 8f), Color.white);
        GameUITheme.StyleButton(button);   // same burgundy lacquer strip as the Settings panel
        return button;
    }
}
