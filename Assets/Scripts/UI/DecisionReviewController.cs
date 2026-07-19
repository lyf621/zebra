using UnityEngine;
using UnityEngine.UI;

// 在事件或任务选择期间提供半返回查看模式，查看时锁定所有游戏操作。
public class DecisionReviewController : MonoBehaviour
{
    private EventManager mEvents;
    private MissionManager mMissions;
    private EventPanelUI mEventPanel;
    private MissionPanelUI mMissionPanel;
    private ZebraGameController mCards;
    private Button mReviewButton;
    private Text mButtonText;
    private bool mReviewing;

    public static DecisionReviewController EnsureExists()
    {
        DecisionReviewController controller = FindAnyObjectByType<DecisionReviewController>();
        if (controller == null) controller = new GameObject("Decision Review Controller").AddComponent<DecisionReviewController>();
        return controller;
    }

    private void Start()
    {
        mEvents = FindAnyObjectByType<EventManager>();
        mMissions = FindAnyObjectByType<MissionManager>();
        mEventPanel = FindAnyObjectByType<EventPanelUI>();
        mMissionPanel = FindAnyObjectByType<MissionPanelUI>();
        mCards = FindAnyObjectByType<ZebraGameController>();
        BuildButton();
    }

    private void Update()
    {
        bool eventChoice = mEvents != null && mEvents.IsAwaitingChoice();
        bool missionChoice = mMissions != null && mMissions.IsAwaitingChoice();
        bool hasChoice = eventChoice || missionChoice;
        if (!hasChoice && mReviewing) ExitReview();
        if (mReviewButton == null) return;
        mReviewButton.gameObject.SetActive(hasChoice);
        bool chinese = mCards != null && mCards.UseChinese;
        if (mButtonText != null)
        {
            if (mReviewing) mButtonText.text = chinese ? (eventChoice ? "返回事件" : "返回任务") : (eventChoice ? "Return to Event" : "Return to Mission");
            else mButtonText.text = chinese ? "暂时返回查看" : "Review Cards and Map";
        }
    }

    // 在决策面板和只读查看页面之间切换，决策状态本身不会被清除。
    private void ToggleReview()
    {
        if (mReviewing) ExitReview();
        else EnterReview();
    }

    private void EnterReview()
    {
        mReviewing = true;
        if (mEventPanel != null) mEventPanel.SetVisibleForReview(false);
        if (mMissionPanel != null) mMissionPanel.SetVisibleForReview(false);
        if (mCards != null) mCards.SetDecisionReviewMode(true);
    }

    private void ExitReview()
    {
        mReviewing = false;
        if (mCards != null) mCards.SetDecisionReviewMode(false);
        if (mEventPanel != null && mEvents != null && mEvents.IsAwaitingChoice()) mEventPanel.SetVisibleForReview(true);
        if (mMissionPanel != null && mMissions != null && mMissions.IsAwaitingChoice()) mMissionPanel.SetVisibleForReview(true);
    }

    // 创建始终位于决策面板上层、但低于设置页的半返回按钮。
    private void BuildButton()
    {
        GameObject canvasObject = new GameObject("Decision Review Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 400;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject buttonObject = new GameObject("Decision Review Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(canvasObject.transform, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 22f);
        rect.sizeDelta = new Vector2(230f, 46f);
        mReviewButton = buttonObject.GetComponent<Button>();
        mReviewButton.targetGraphic = buttonObject.GetComponent<Image>();
        mReviewButton.onClick.AddListener(ToggleReview);

        GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(buttonObject.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        mButtonText = textObject.GetComponent<Text>();
        mButtonText.font = GameUITheme.GetLegacyFont();
        mButtonText.fontSize = 17;
        mButtonText.fontStyle = FontStyle.Bold;
        mButtonText.alignment = TextAnchor.MiddleCenter;
        mButtonText.color = Color.white;
        mButtonText.raycastTarget = false;
        GameUITheme.StyleButton(mReviewButton);
        mReviewButton.gameObject.SetActive(false);
    }
}
