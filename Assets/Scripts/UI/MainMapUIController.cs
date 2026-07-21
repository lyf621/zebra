using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 在运行时补全主地图 HUD，并统一已有事件、任务与阶段按钮的外观。
public class MainMapUIController : MonoBehaviour
{
    private StatManager mStats;
    private TurnController mTurns;
    private ZebraGameController mCards;
    private EventManager mEvents;
    private MissionManager mMissions;
    private Text mHudSummaryText;
    private StatBar[] mStatBars;
    private Button mShowMissionButton;

    private sealed class StatBar
    {
        public Text label;
        public Image[] segments;
    }

    // 确保场景中只有一个 UI 完善控制器；由 TurnController 启动时调用。
    public static MainMapUIController EnsureExists()
    {
        MainMapUIController controller = FindAnyObjectByType<MainMapUIController>();
        if (controller == null)
        {
            controller = new GameObject("Main Map UI Controller").AddComponent<MainMapUIController>();
        }
        return controller;
    }

    // 查找队友已有对象并在不改变游戏规则的前提下应用新样式。
    private void Start()
    {
        mStats = FindAnyObjectByType<StatManager>();
        mTurns = FindAnyObjectByType<TurnController>();
        mCards = FindAnyObjectByType<ZebraGameController>();
        mEvents = FindAnyObjectByType<EventManager>();
        mMissions = FindAnyObjectByType<MissionManager>();

        Canvas canvas = FindSceneCanvas();
        if (canvas == null)
        {
            Debug.LogWarning("MainMapUIController: scene Canvas was not found.");
            return;
        }

        canvas.overrideSorting = true;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        if (Camera.main != null)
        {
            Camera.main.backgroundColor = new Color(0.045f, 0.075f, 0.065f, 1f);
        }

        BuildHud(canvas.transform);
        StyleMainButtons(canvas.transform);
        StylePopupPanel(FindDescendant(canvas.transform, "EventPanel"), false);
        StylePopupPanel(FindDescendant(canvas.transform, "MissionPanel"), true);
    }

    // 每帧刷新 HUD，确保回合、资源和临时属性与 StatManager 保持同步。
    private void Update()
    {
        if (mHudSummaryText == null || mStats == null || mTurns == null)
        {
            return;
        }

        if (mCards == null) mCards = FindAnyObjectByType<ZebraGameController>();
        bool decisionOpen = (mEvents != null && mEvents.IsAwaitingChoice()) || (mMissions != null && mMissions.IsAwaitingChoice());
        if (mShowMissionButton != null) mShowMissionButton.interactable = mMissions != null && mMissions.HasMission() && !decisionOpen && (mCards == null || !mCards.IsDecisionReviewMode) && !mTurns.IsGameOver();
        bool chinese = mCards != null && mCards.UseChinese;
        mHudSummaryText.text = chinese
            ? "回合 " + mTurns.GetTurnCount() + "/" + mTurns.GetMaxTurnCount() + "    大臣 " + mTurns.GetMinistersLeft() + "/" + mTurns.GetMaxMinisters() + "    金币 " + mStats.GetGold() + "    威严 " + mStats.GetMajesty() + "    战斗力 " + mStats.GetFight()
            : "TURN " + mTurns.GetTurnCount() + "/" + mTurns.GetMaxTurnCount() + "    MINISTERS " + mTurns.GetMinistersLeft() + "/" + mTurns.GetMaxMinisters() + "    GOLD " + mStats.GetGold() + "    MAJESTY " + mStats.GetMajesty() + "    FIGHT " + mStats.GetFight();

        SetStatBar(0, chinese ? "民意" : "PUBLIC OPINION", mStats.GetPO(), mStats.GetMaxStat());
        SetStatBar(1, chinese ? "军力" : "MILITARY", mStats.GetMS(), mStats.GetMaxStat());
        SetStatBar(2, chinese ? "权威" : "AUTHORITY", mStats.GetAL(), mStats.GetMaxStat());
        SetStatBar(3, chinese ? "王室" : "KING", mStats.GetKR(), mStats.GetMaxStat());
        SetStatBar(4, chinese ? "教会" : "CHURCH", mStats.GetCR(), mStats.GetMaxStat());
        SetStatBar(5, chinese ? "大贵族" : "ARISTOCRATS", mStats.GetAR(), mStats.GetMaxStat());
    }

    // 找到队友场景中原有的 Canvas，排除卡牌系统运行时创建的 Game Canvas。
    private Canvas FindSceneCanvas()
    {
        foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include))
        {
            if (canvas.name == "Canvas")
            {
                return canvas;
            }
        }
        return null;
    }

    // 在主 Canvas 顶部创建摘要行与六组十格属性条。
    private void BuildHud(Transform canvasTransform)
    {
        GameObject panelObject = new GameObject("Game HUD", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelObject.transform.SetParent(canvasTransform, false);
        panelObject.transform.SetAsFirstSibling();
        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.offsetMin = new Vector2(12f, -96f);
        panelRect.offsetMax = new Vector2(-220f, -10f);
        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = GameUITheme.DeepGreen;
        panelImage.raycastTarget = false;
        Outline outline = panelObject.AddComponent<Outline>();
        outline.effectColor = GameUITheme.Gold;
        outline.effectDistance = new Vector2(2f, -2f);

        GameObject textObject = new GameObject("HUD Summary", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(panelObject.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 1f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.pivot = new Vector2(0.5f, 1f);
        textRect.offsetMin = new Vector2(14f, -27f);
        textRect.offsetMax = new Vector2(-14f, -6f);
        mHudSummaryText = textObject.GetComponent<Text>();
        mHudSummaryText.font = GameUITheme.GetLegacyFont();
        mHudSummaryText.fontSize = 14;
        mHudSummaryText.fontStyle = FontStyle.Bold;
        mHudSummaryText.color = Color.white;
        mHudSummaryText.alignment = TextAnchor.MiddleLeft;
        mHudSummaryText.horizontalOverflow = HorizontalWrapMode.Overflow;
        mHudSummaryText.verticalOverflow = VerticalWrapMode.Truncate;
        mHudSummaryText.raycastTarget = false;

        mStatBars = new StatBar[6];
        string[] keys = { "PO", "MS", "AL", "KING", "CHURCH", "ARISTOCRATS" };
        for (int i = 0; i < mStatBars.Length; i++)
        {
            int column = i / 3;
            int row = i % 3;
            mStatBars[i] = CreateStatBar(panelObject.transform, keys[i], column, row);
        }
    }

    private StatBar CreateStatBar(Transform parent, string key, int column, int row)
    {
        GameObject root = new GameObject("Stat " + key, typeof(RectTransform));
        root.transform.SetParent(parent, false);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(column == 0 ? 0f : 0.5f, 1f);
        rootRect.anchorMax = new Vector2(column == 0 ? 0.5f : 1f, 1f);
        rootRect.pivot = new Vector2(0f, 1f);
        rootRect.offsetMin = new Vector2(column == 0 ? 14f : 8f, -52f - row * 14f);
        rootRect.offsetMax = new Vector2(column == 0 ? -8f : -14f, -38f - row * 14f);

        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        labelObject.transform.SetParent(root.transform, false);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(0f, 1f);
        labelRect.pivot = new Vector2(0f, 0.5f);
        labelRect.sizeDelta = new Vector2(94f, 0f);
        Text label = labelObject.GetComponent<Text>();
        label.font = GameUITheme.GetLegacyFont();
        label.fontSize = 10;
        label.fontStyle = FontStyle.Bold;
        label.color = GameUITheme.Parchment;
        label.alignment = TextAnchor.MiddleLeft;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.raycastTarget = false;

        GameObject segmentsObject = new GameObject("Segments", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        segmentsObject.transform.SetParent(root.transform, false);
        RectTransform segmentsRect = segmentsObject.GetComponent<RectTransform>();
        segmentsRect.anchorMin = new Vector2(0f, 0f);
        segmentsRect.anchorMax = new Vector2(1f, 1f);
        segmentsRect.offsetMin = new Vector2(98f, 1f);
        segmentsRect.offsetMax = new Vector2(0f, -1f);
        HorizontalLayoutGroup layout = segmentsObject.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 2f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        Image[] segments = new Image[10];
        for (int i = 0; i < segments.Length; i++)
        {
            GameObject segment = new GameObject("Cell " + (i + 1), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            segment.transform.SetParent(segmentsObject.transform, false);
            Image image = segment.GetComponent<Image>();
            image.color = new Color(0.18f, 0.22f, 0.19f, 1f);
            image.raycastTarget = false;
            Outline outline = segment.AddComponent<Outline>();
            outline.effectColor = new Color(0.72f, 0.63f, 0.37f, 0.5f);
            outline.effectDistance = new Vector2(1f, -1f);
            segments[i] = image;
        }

        return new StatBar { label = label, segments = segments };
    }

    private void SetStatBar(int index, string label, int value, int max)
    {
        if (mStatBars == null || index < 0 || index >= mStatBars.Length) return;
        StatBar bar = mStatBars[index];
        bar.label.text = label + " " + Mathf.Clamp(value, 0, max) + "/" + max;

        // 高于 6 变绿，低于 4 变红，介于 4~6 保持默认金色（仅影响条形图，不影响文本）。
        Color fill = value > 6 ? new Color(0.30f, 0.70f, 0.32f)
                   : value < 4 ? new Color(0.80f, 0.26f, 0.22f)
                   : GameUITheme.Gold;
        for (int i = 0; i < bar.segments.Length; i++)
        {
            bar.segments[i].color = i < value ? fill : new Color(0.18f, 0.22f, 0.19f, 1f);
        }
    }

    // 调整两个主操作按钮的位置与尺寸，并应用统一按钮样式。
    private void StyleMainButtons(Transform canvasTransform)
    {
        Transform nextPhase = FindDescendant(canvasTransform, "NextPhaseButton");
        Transform showMission = FindDescendant(canvasTransform, "ShowMissionButton");
        if (showMission != null) mShowMissionButton = showMission.GetComponent<Button>();
        ConfigureMainButton(nextPhase, new Vector2(-22f, -106f));
        ConfigureMainButton(showMission, new Vector2(-22f, -160f));
    }

    // 设置单个主操作按钮的右上角锚点、稳定尺寸与视觉状态。
    private void ConfigureMainButton(Transform buttonTransform, Vector2 position)
    {
        if (buttonTransform == null)
        {
            return;
        }
        RectTransform rect = buttonTransform as RectTransform;
        rect.anchorMin = Vector2.one;
        rect.anchorMax = Vector2.one;
        rect.pivot = Vector2.one;
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(180f, 44f);
        GameUITheme.StyleButton(buttonTransform.GetComponent<Button>());
    }

    // 将事件或任务窗口整理成统一的羊皮纸面板，并调整标题、正文与选项区域。
    private void StylePopupPanel(Transform panelTransform, bool missionPanel)
    {
        if (panelTransform == null)
        {
            return;
        }

        RectTransform panelRect = panelTransform as RectTransform;
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(720f, 500f);

        Transform backgroundTransform = FindDescendant(panelTransform, "Background");
        if (backgroundTransform != null)
        {
            RectTransform backgroundRect = backgroundTransform as RectTransform;
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
            Image background = backgroundTransform.GetComponent<Image>();
            if (background != null)
            {
                background.sprite = GameUITheme.GetPaperSprite();
                background.color = GameUITheme.Parchment;
                background.raycastTarget = true;
                Outline outline = background.GetComponent<Outline>();
                if (outline == null) outline = background.gameObject.AddComponent<Outline>();
                outline.effectColor = GameUITheme.Gold;
                outline.effectDistance = new Vector2(4f, -4f);
            }
        }

        TMP_Text title = GetNamedText(panelTransform, "Title");
        TMP_Text description = GetNamedText(panelTransform, "Description");
        ConfigurePanelText(title, new Vector2(0f, -48f), new Vector2(620f, 58f), 30f, FontStyles.Bold, TextAlignmentOptions.Center);
        ConfigurePanelText(description, new Vector2(0f, -150f), new Vector2(610f, 150f), 19f, FontStyles.Normal, TextAlignmentOptions.TopLeft);

        Transform container = FindDescendant(panelTransform, missionPanel ? "ResolutionContainer" : "OptionContainer");
        if (container != null)
        {
            RectTransform containerRect = container as RectTransform;
            containerRect.anchorMin = new Vector2(0.5f, 0f);
            containerRect.anchorMax = new Vector2(0.5f, 0f);
            containerRect.pivot = new Vector2(0.5f, 0f);
            containerRect.anchoredPosition = new Vector2(0f, 28f);
            containerRect.sizeDelta = new Vector2(560f, 168f);
            VerticalLayoutGroup layout = container.GetComponent<VerticalLayoutGroup>();
            if (layout != null)
            {
                layout.spacing = 10f;
                layout.childControlHeight = true;
                layout.childForceExpandHeight = false;
                layout.childControlWidth = true;
                layout.childForceExpandWidth = true;
            }
        }

        if (missionPanel)
        {
            TMP_Text result = GetNamedText(panelTransform, "Result");
            ConfigurePanelText(result, new Vector2(0f, -290f), new Vector2(560f, 42f), 18f, FontStyles.Bold, TextAlignmentOptions.Center);
        }
    }

    // 设置事件与任务面板中的单个 TMP 文本位置和排版。
    private void ConfigurePanelText(TMP_Text text, Vector2 position, Vector2 size, float fontSize, FontStyles style, TextAlignmentOptions alignment)
    {
        if (text == null)
        {
            return;
        }
        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        text.alignment = alignment;
        text.overflowMode = TextOverflowModes.Ellipsis;
        GameUITheme.StyleTmpText(text, fontSize, style, GameUITheme.Ink);
    }

    // 在指定父节点的所有后代中按名称查找对象，也能找到当前未激活的面板。
    private Transform FindDescendant(Transform root, string objectName)
    {
        if (root == null)
        {
            return null;
        }
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == objectName)
            {
                return child;
            }
        }
        return null;
    }

    // 在面板后代中按对象名称查找 TMP 文本组件。
    private TMP_Text GetNamedText(Transform root, string objectName)
    {
        Transform target = FindDescendant(root, objectName);
        return target != null ? target.GetComponent<TMP_Text>() : null;
    }
}
