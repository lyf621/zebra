using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Modal warning shown when the player picks a mission resolution they cannot pay for.
/// Confirm applies the resolution anyway (pushing gold below zero); Cancel closes the warning
/// and leaves the resolution buttons up so another choice can be made.
///
/// Created on demand at runtime — no scene object and no Inspector wiring. Styling matches the
/// mission panel: parchment surface, GameUITheme font, burgundy lacquer buttons.
///
/// ===========================================================================================
///  TO EDIT THE WARNING TEXT, change the four constants directly below.
/// ===========================================================================================
/// </summary>
public class GoldWarningPanel : MonoBehaviour
{
    // ---- EDIT THESE FOUR STRINGS TO CHANGE THE PANEL'S WORDING ----
    private const string kTitleEnglish = "Warning: Excessive Spending";
    private const string kTitleChinese = "警告：挥霍无度";
    private const string kBodyEnglish = "You don't have enough gold. Choosing this option will put you in debt. Unpaid debt at turn 10 leads to dire consequences. Insist on your choice?";
    private const string kBodyChinese = "你没有足够的金币。选择这个选项将使你负债。到第10回合仍未偿清的债务会造成严重后果。还想坚持你的选择吗？";
    // ---------------------------------------------------------------

    private static GoldWarningPanel sInstance;

    private GameObject mRoot;
    private Text mTitle;
    private Text mBody;
    private Text mConfirmLabel;
    private Text mCancelLabel;
    private System.Action mOnConfirm;
    private System.Action mOnCancel;

    public static GoldWarningPanel EnsureExists()
    {
        if (sInstance != null) return sInstance;
        sInstance = FindAnyObjectByType<GoldWarningPanel>();
        if (sInstance == null)
            sInstance = new GameObject("Gold Warning Panel").AddComponent<GoldWarningPanel>();
        return sInstance;
    }

    private void Awake()
    {
        if (sInstance == null) sInstance = this;
        Build();
        Hide();
    }

    /// <summary>Opens the warning. Exactly one of the two callbacks runs, then the panel closes.</summary>
    public void Show(bool chinese, System.Action onConfirm, System.Action onCancel)
    {
        if (mRoot == null) return;

        mOnConfirm = onConfirm;
        mOnCancel = onCancel;

        if (mTitle != null) mTitle.text = chinese ? kTitleChinese : kTitleEnglish;
        if (mBody != null) mBody.text = chinese ? kBodyChinese : kBodyEnglish;
        if (mConfirmLabel != null) mConfirmLabel.text = chinese ? "确认" : "Confirm";
        if (mCancelLabel != null) mCancelLabel.text = chinese ? "取消" : "Cancel";

        mRoot.SetActive(true);
    }

    public void Hide()
    {
        if (mRoot != null) mRoot.SetActive(false);
    }

    public bool IsOpen()
    {
        return mRoot != null && mRoot.activeSelf;
    }

    private void OnConfirmClicked()
    {
        System.Action callback = mOnConfirm;
        mOnConfirm = null;
        mOnCancel = null;
        Hide();                    // close first, so the callback may open something else
        callback?.Invoke();
    }

    private void OnCancelClicked()
    {
        System.Action callback = mOnCancel;
        mOnConfirm = null;
        mOnCancel = null;
        Hide();
        callback?.Invoke();
    }

    // ---------------------------------------------------------------- build
    private void Build()
    {
        GameObject canvasObject = new GameObject("Gold Warning Canvas",
            typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 700;   // above the mission panel, below the ending screen (1000)
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // The dim layer is what makes this modal: it swallows clicks aimed at the resolution
        // buttons underneath, so the player must answer the warning first.
        mRoot = new GameObject("Dim", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        mRoot.transform.SetParent(canvasObject.transform, false);
        RectTransform dimRect = mRoot.GetComponent<RectTransform>();
        dimRect.anchorMin = Vector2.zero;
        dimRect.anchorMax = Vector2.one;
        dimRect.sizeDelta = Vector2.zero;
        dimRect.anchoredPosition = Vector2.zero;
        mRoot.GetComponent<Image>().color = new Color(0.025f, 0.02f, 0.015f, 0.72f);

        Image panel = CreateImage("Panel", mRoot.transform, Vector2.zero, new Vector2(520f, 260f),
                                  new Color(0.88f, 0.84f, 0.72f, 1f));

        mTitle = CreateText("Title", panel.transform, string.Empty, 26, FontStyle.Bold,
                            new Vector2(0f, 86f), new Vector2(460f, 42f), GameUITheme.Ink, TextAnchor.MiddleCenter);

        mBody = CreateText("Body", panel.transform, string.Empty, 18, FontStyle.Normal,
                           new Vector2(0f, 14f), new Vector2(440f, 96f), new Color(0.26f, 0.22f, 0.16f), TextAnchor.UpperCenter);

        Button confirm = CreateButton("Confirm", panel.transform, new Vector2(-102f, -82f), new Vector2(170f, 44f), out mConfirmLabel);
        confirm.onClick.AddListener(OnConfirmClicked);

        Button cancel = CreateButton("Cancel", panel.transform, new Vector2(102f, -82f), new Vector2(170f, 44f), out mCancelLabel);
        cancel.onClick.AddListener(OnCancelClicked);
    }

    // ---------------------------------------------------------------- helpers
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

    private Text CreateText(string name, Transform parent, string value, int size, FontStyle style,
                            Vector2 position, Vector2 rectSize, Color color, TextAnchor alignment)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = rectSize;
        Text text = go.GetComponent<Text>();
        text.font = GameUITheme.GetLegacyFont();
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.supportRichText = true;
        text.raycastTarget = false;
        return text;
    }

    private Button CreateButton(string name, Transform parent, Vector2 position, Vector2 size, out Text label)
    {
        Image image = CreateImage(name, parent, position, size, new Color(0.32f, 0.3f, 0.27f));
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        label = CreateText("Label", image.transform, string.Empty, 19, FontStyle.Bold,
                           Vector2.zero, size - new Vector2(16f, 8f), Color.white, TextAnchor.MiddleCenter);
        GameUITheme.StyleButton(button);   // same burgundy lacquer strip as the mission panel
        return button;
    }
}
