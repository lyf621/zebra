using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Full-screen picture guide opened from the Hint button on the in-game settings panel.
/// Pages are stepped through with Next / Back; Close hides the guide and leaves the settings
/// panel visible underneath, so the player returns to exactly where they were.
///
/// Created on demand at runtime — no scene object and no Inspector wiring of its own. The
/// pictures themselves come from ZebraGameController's "Hint Pages" array, which IS
/// Inspector-editable: drag Sprites in and reorder them there.
/// </summary>
public class HintPanel : MonoBehaviour
{
    private static HintPanel sInstance;

    private GameObject mRoot;
    private Image mPage;
    private Text mCounter;
    private Button mBackButton;
    private Button mNextButton;
    private Text mBackLabel;
    private Text mNextLabel;
    private Text mCloseLabel;

    private Sprite[] mPages;
    private int mIndex;
    private bool mChinese;

    public static HintPanel EnsureExists()
    {
        if (sInstance != null) return sInstance;
        sInstance = FindAnyObjectByType<HintPanel>();
        if (sInstance == null)
            sInstance = new GameObject("Hint Panel").AddComponent<HintPanel>();
        return sInstance;
    }

    private void Awake()
    {
        if (sInstance == null) sInstance = this;
        Build();
        Hide();
    }

    /// <summary>Opens the guide at page 1. Does nothing when no pictures have been assigned.</summary>
    public void Show(Sprite[] pages, bool chinese)
    {
        if (mRoot == null) return;
        if (pages == null || pages.Length == 0)
        {
            Debug.LogWarning("HintPanel: no hint pages assigned — set them on ZebraGameController's Hint Pages array.");
            return;
        }

        mPages = pages;
        mChinese = chinese;
        mIndex = 0;

        if (mBackLabel != null) mBackLabel.text = chinese ? "返回" : "Back";
        if (mNextLabel != null) mNextLabel.text = chinese ? "继续" : "Next";
        if (mCloseLabel != null) mCloseLabel.text = chinese ? "关闭" : "Close";

        mRoot.SetActive(true);
        RefreshPage();
    }

    public void Hide()
    {
        if (mRoot != null) mRoot.SetActive(false);
    }

    public bool IsOpen()
    {
        return mRoot != null && mRoot.activeSelf;
    }

    private void Step(int delta)
    {
        if (mPages == null || mPages.Length == 0) return;
        mIndex = Mathf.Clamp(mIndex + delta, 0, mPages.Length - 1);
        RefreshPage();
    }

    private void RefreshPage()
    {
        if (mPages == null || mPages.Length == 0) return;

        // Skip over any empty slot left in the Inspector array rather than showing a blank page.
        Sprite sprite = mPages[Mathf.Clamp(mIndex, 0, mPages.Length - 1)];
        if (mPage != null)
        {
            mPage.sprite = sprite;
            mPage.enabled = sprite != null;
        }

        if (mCounter != null) mCounter.text = (mIndex + 1) + " / " + mPages.Length;

        // Ends of the guide simply grey out, rather than wrapping around unexpectedly.
        if (mBackButton != null) mBackButton.interactable = mIndex > 0;
        if (mNextButton != null) mNextButton.interactable = mIndex < mPages.Length - 1;
    }

    // ---------------------------------------------------------------- build
    private void Build()
    {
        GameObject canvasObject = new GameObject("Hint Canvas",
            typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        // Must clear BOTH settings panels that can open it: the in-game overlay (500) and the
        // MainMenu one (900). Still below the ending screen (1000).
        canvas.sortingOrder = 950;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // Opaque backdrop: hides the settings panel while the guide is up and swallows clicks
        // meant for it. The settings overlay stays alive underneath, so Close reveals it again.
        mRoot = new GameObject("Backdrop", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        mRoot.transform.SetParent(canvasObject.transform, false);
        Stretch(mRoot.GetComponent<RectTransform>());
        mRoot.GetComponent<Image>().color = new Color(0.03f, 0.025f, 0.02f, 0.97f);

        // The picture fills the screen but keeps its aspect ratio, so pages of any shape or
        // resolution display uncropped and undistorted.
        GameObject pageObject = new GameObject("Page", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        pageObject.transform.SetParent(mRoot.transform, false);
        Stretch(pageObject.GetComponent<RectTransform>());
        mPage = pageObject.GetComponent<Image>();
        mPage.preserveAspect = true;
        mPage.raycastTarget = false;

        mBackButton = CreateButton("Back", mRoot.transform, new Vector2(0.5f, 0f), new Vector2(-210f, 54f),
                                   new Vector2(170f, 48f), out mBackLabel);
        mBackButton.onClick.AddListener(() => Step(-1));

        mNextButton = CreateButton("Next", mRoot.transform, new Vector2(0.5f, 0f), new Vector2(210f, 54f),
                                   new Vector2(170f, 48f), out mNextLabel);
        mNextButton.onClick.AddListener(() => Step(1));

        Button closeButton = CreateButton("Close", mRoot.transform, new Vector2(1f, 1f), new Vector2(-96f, -44f),
                                          new Vector2(150f, 46f), out mCloseLabel);
        closeButton.onClick.AddListener(Hide);

        mCounter = CreateText("Counter", mRoot.transform, new Vector2(0.5f, 0f), new Vector2(0f, 54f),
                              new Vector2(160f, 34f), 18, FontStyle.Bold, new Color(0.92f, 0.86f, 0.68f));
    }

    // ---------------------------------------------------------------- helpers
    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
    }

    private Text CreateText(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size,
                            int fontSize, FontStyle style, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Text text = go.GetComponent<Text>();
        text.font = GameUITheme.GetLegacyFont();
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    private Button CreateButton(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size, out Text label)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = go.GetComponent<Image>();
        image.color = new Color(0.32f, 0.3f, 0.27f);
        Button button = go.AddComponent<Button>();
        button.targetGraphic = image;

        label = CreateText("Label", go.transform, new Vector2(0.5f, 0.5f), Vector2.zero,
                           size - new Vector2(16f, 8f), 19, FontStyle.Bold, Color.white);
        GameUITheme.StyleButton(button);   // same burgundy lacquer strip as every other panel
        return button;
    }
}
