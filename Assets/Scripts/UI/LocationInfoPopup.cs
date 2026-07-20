using UnityEngine;
using UnityEngine.UI;

// 右键地点时在鼠标旁弹出的信息浮窗（名称 + 描述）。
// 非阻挡式：不拦截任何点击；显示后按下任意鼠标键即消失。
// 运行时按需自动创建，无需在场景中手动摆放。
public class LocationInfoPopup : MonoBehaviour
{
    private static LocationInfoPopup sInstance;

    private RectTransform mCanvasRect;
    private RectTransform mPanel;
    private Text mTitle;
    private Text mBody;
    private bool mVisible;
    private float mShownTime;

    public static LocationInfoPopup EnsureExists()
    {
        if (sInstance != null) return sInstance;
        sInstance = FindAnyObjectByType<LocationInfoPopup>();
        if (sInstance == null)
            sInstance = new GameObject("Location Info Popup").AddComponent<LocationInfoPopup>();
        return sInstance;
    }

    private void Awake()
    {
        if (sInstance == null) sInstance = this;
        Build();
        Hide();
    }

    // 在给定的屏幕坐标旁显示名称与描述。
    public void Show(string title, string body, Vector2 screenPos)
    {
        if (mPanel == null) return;
        if (mTitle != null) mTitle.text = string.IsNullOrEmpty(title) ? "(Unnamed)" : title;
        if (mBody != null) mBody.text = body == null ? string.Empty : body;
        mPanel.gameObject.SetActive(true);
        mVisible = true;
        mShownTime = Time.unscaledTime;
        Reposition(screenPos);
    }

    public void Hide()
    {
        mVisible = false;
        if (mPanel != null) mPanel.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!mVisible) return;
        if (Time.unscaledTime - mShownTime < 0.05f) return;   // 避免打开的当帧就被关闭
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
            Hide();
    }

    // 将面板左上角对齐到光标附近，并夹在屏幕范围内。
    private void Reposition(Vector2 screenPos)
    {
        if (mCanvasRect == null || mPanel == null) return;
        LayoutRebuilder.ForceRebuildLayoutImmediate(mPanel);   // 先让高度随内容更新
        RectTransformUtility.ScreenPointToLocalPointInRectangle(mCanvasRect, screenPos, null, out Vector2 local);

        Vector2 pos = local + new Vector2(14f, -14f);          // 光标右下方
        float w = mPanel.rect.width, h = mPanel.rect.height;
        float halfW = mCanvasRect.rect.width * 0.5f, halfH = mCanvasRect.rect.height * 0.5f;
        pos.x = Mathf.Clamp(pos.x, -halfW, halfW - w);
        pos.y = Mathf.Clamp(pos.y, -halfH + h, halfH);
        mPanel.anchoredPosition = pos;
    }

    private void Build()
    {
        GameObject canvasObject = new GameObject("Location Info Canvas", typeof(Canvas), typeof(CanvasScaler));
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 600;   // 位于大多数界面之上
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;
        mCanvasRect = canvasObject.GetComponent<RectTransform>();

        GameObject panelObject = new GameObject("Panel",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image),
            typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        panelObject.transform.SetParent(canvasObject.transform, false);
        mPanel = panelObject.GetComponent<RectTransform>();
        mPanel.anchorMin = new Vector2(0.5f, 0.5f);
        mPanel.anchorMax = new Vector2(0.5f, 0.5f);
        mPanel.pivot = new Vector2(0f, 1f);          // 左上角为锚点，向右下展开
        mPanel.sizeDelta = new Vector2(280f, 0f);    // 固定宽度，高度自适应

        Image bg = panelObject.GetComponent<Image>();
        bg.color = new Color(0.10f, 0.16f, 0.14f, 0.96f);
        bg.raycastTarget = false;                    // 非阻挡

        VerticalLayoutGroup vlg = panelObject.GetComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(12, 12, 10, 12);
        vlg.spacing = 6f;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter fitter = panelObject.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;   // 保持固定宽度
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;     // 高度随内容

        mTitle = CreateText(panelObject.transform, 20, FontStyle.Bold, new Color(0.95f, 0.82f, 0.42f));
        mBody = CreateText(panelObject.transform, 16, FontStyle.Normal, new Color(0.93f, 0.90f, 0.82f));
    }

    private Text CreateText(Transform parent, int fontSize, FontStyle style, Color color)
    {
        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);
        Text text = textObject.GetComponent<Text>();
        text.font = GameUITheme.GetLegacyFont();
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = TextAnchor.UpperLeft;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }
}
