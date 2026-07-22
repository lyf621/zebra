using UnityEngine;
using UnityEngine.UI;

// 悬停事件选项按钮时，在鼠标旁弹出对应任务的潜在数值变化预览。
// 非阻挡式；由悬停进入 / 退出显式控制显隐（不自动关闭）。运行时按需自动创建。
public class MissionPreviewTooltip : MonoBehaviour
{
    private static MissionPreviewTooltip sInstance;

    private RectTransform mCanvasRect;
    private RectTransform mPanel;
    private Text mTitle;
    private Text mBody;

    public static MissionPreviewTooltip EnsureExists()
    {
        if (sInstance != null) return sInstance;
        sInstance = FindAnyObjectByType<MissionPreviewTooltip>();
        if (sInstance == null)
            sInstance = new GameObject("Mission Preview Tooltip").AddComponent<MissionPreviewTooltip>();
        return sInstance;
    }

    // 不存在时不创建，仅在已有实例时隐藏（供悬停退出调用）。
    public static void HideTooltip() { if (sInstance != null) sInstance.Hide(); }

    private void Awake()
    {
        if (sInstance == null) sInstance = this;
        Build();
        Hide();
    }

    public void Show(string title, string body, Vector2 screenPos)
    {
        if (mPanel == null) return;
        if (mTitle != null) mTitle.text = title == null ? string.Empty : title;
        if (mBody != null) mBody.text = body == null ? string.Empty : body;
        mPanel.gameObject.SetActive(true);
        Reposition(screenPos);
    }

    public void Hide()
    {
        if (mPanel != null) mPanel.gameObject.SetActive(false);
    }

    private void Reposition(Vector2 screenPos)
    {
        if (mCanvasRect == null || mPanel == null) return;
        LayoutRebuilder.ForceRebuildLayoutImmediate(mPanel);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(mCanvasRect, screenPos, null, out Vector2 local);

        Vector2 pos = local + new Vector2(16f, -16f);
        float w = mPanel.rect.width, h = mPanel.rect.height;
        float halfW = mCanvasRect.rect.width * 0.5f, halfH = mCanvasRect.rect.height * 0.5f;
        pos.x = Mathf.Clamp(pos.x, -halfW, halfW - w);
        pos.y = Mathf.Clamp(pos.y, -halfH + h, halfH);
        mPanel.anchoredPosition = pos;
    }

    private void Build()
    {
        GameObject canvasObject = new GameObject("Mission Preview Canvas", typeof(Canvas), typeof(CanvasScaler));
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 650;   // 位于事件面板及其它界面之上
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
        mPanel.pivot = new Vector2(0f, 1f);
        mPanel.sizeDelta = new Vector2(320f, 0f);   // 固定宽度，高度随内容自适应

        Image bg = panelObject.GetComponent<Image>();
        bg.color = new Color(0.10f, 0.16f, 0.14f, 0.97f);
        bg.raycastTarget = false;                   // 非阻挡，避免抢走按钮的悬停事件

        VerticalLayoutGroup vlg = panelObject.GetComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(12, 12, 10, 12);
        vlg.spacing = 6f;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter fitter = panelObject.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        mTitle = CreateText(panelObject.transform, 18, FontStyle.Bold, new Color(0.95f, 0.82f, 0.42f));
        mBody = CreateText(panelObject.transform, 14, FontStyle.Normal, new Color(0.93f, 0.90f, 0.82f));
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
