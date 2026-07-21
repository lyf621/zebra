using UnityEngine;
using UnityEngine.UI;

// 第一回合中，每次状态刷新都弹出一个带关闭按钮的模态面板，
// 背景遮罩会拦截点击，玩家必须点击“关闭”才能继续，确保不会忽略状态信息。
public class StatusPopup : MonoBehaviour
{
    private static StatusPopup sInstance;

    private GameObject mRoot;      // 遮罩 + 面板整体（一起显隐）
    private Text mMessage;
    private Text mCloseLabel;

    public static StatusPopup EnsureExists()
    {
        if (sInstance != null) return sInstance;
        sInstance = FindAnyObjectByType<StatusPopup>();
        if (sInstance == null) sInstance = new GameObject("Status Popup").AddComponent<StatusPopup>();
        return sInstance;
    }

    public static void HidePopup() { if (sInstance != null) sInstance.Hide(); }

    private void Awake()
    {
        if (sInstance == null) sInstance = this;
        Build();
        Hide();
    }

    // 显示（或更新）当前状态信息。已经显示时只更新文本，不叠加新面板。
    public void Show(string message, bool chinese)
    {
        if (mRoot == null) return;
        if (mMessage != null) mMessage.text = message == null ? string.Empty : message;
        if (mCloseLabel != null) mCloseLabel.text = chinese ? "关闭" : "Close";
        mRoot.SetActive(true);
    }

    public void Hide()
    {
        if (mRoot != null) mRoot.SetActive(false);
    }

    private void Build()
    {
        GameObject canvasObject = new GameObject("Status Popup Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 700;   // 高于其它所有界面
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        // 遮罩：拦截背后的一切点击，逼玩家先关闭。
        mRoot = new GameObject("Backdrop", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        mRoot.transform.SetParent(canvasObject.transform, false);
        RectTransform backdropRect = mRoot.GetComponent<RectTransform>();
        backdropRect.anchorMin = Vector2.zero;
        backdropRect.anchorMax = Vector2.one;
        backdropRect.offsetMin = Vector2.zero;
        backdropRect.offsetMax = Vector2.zero;
        Image backdrop = mRoot.GetComponent<Image>();
        backdrop.color = new Color(0.03f, 0.04f, 0.03f, 0.55f);
        backdrop.raycastTarget = true;

        // 面板：居中，高度随内容自适应。
        GameObject panelObject = new GameObject("Panel",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image),
            typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        panelObject.transform.SetParent(mRoot.transform, false);
        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(520f, 0f);
        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = GameUITheme.DeepGreen;
        Outline panelOutline = panelObject.AddComponent<Outline>();
        panelOutline.effectColor = GameUITheme.Gold;
        panelOutline.effectDistance = new Vector2(2f, -2f);

        VerticalLayoutGroup vlg = panelObject.GetComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(24, 24, 22, 20);
        vlg.spacing = 18f;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childAlignment = TextAnchor.UpperCenter;

        ContentSizeFitter fitter = panelObject.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // 状态信息文本
        GameObject messageObject = new GameObject("Message", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        messageObject.transform.SetParent(panelObject.transform, false);
        mMessage = messageObject.GetComponent<Text>();
        mMessage.font = GameUITheme.GetLegacyFont();
        mMessage.fontSize = 18;
        mMessage.fontStyle = FontStyle.Bold;
        mMessage.alignment = TextAnchor.MiddleCenter;
        mMessage.color = GameUITheme.Parchment;
        mMessage.horizontalOverflow = HorizontalWrapMode.Wrap;
        mMessage.verticalOverflow = VerticalWrapMode.Overflow;
        mMessage.raycastTarget = false;

        // 关闭按钮
        GameObject buttonObject = new GameObject("Close Button",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonObject.transform.SetParent(panelObject.transform, false);
        buttonObject.GetComponent<LayoutElement>().preferredHeight = 44f;
        Button closeButton = buttonObject.GetComponent<Button>();
        closeButton.targetGraphic = buttonObject.GetComponent<Image>();
        closeButton.onClick.AddListener(Hide);

        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        labelObject.transform.SetParent(buttonObject.transform, false);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        mCloseLabel = labelObject.GetComponent<Text>();
        mCloseLabel.font = GameUITheme.GetLegacyFont();
        mCloseLabel.fontSize = 18;
        mCloseLabel.fontStyle = FontStyle.Bold;
        mCloseLabel.alignment = TextAnchor.MiddleCenter;
        mCloseLabel.color = Color.white;
        mCloseLabel.text = "Close";
        mCloseLabel.raycastTarget = false;

        GameUITheme.StyleButton(closeButton);   // 与其它按钮统一风格
    }
}
