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
    private Text mHudText;
    private Button mShowMissionButton;

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
        if (mHudText == null || mStats == null || mTurns == null)
        {
            return;
        }

        if (mCards == null) mCards = FindAnyObjectByType<ZebraGameController>();
        bool decisionOpen = (mEvents != null && mEvents.IsAwaitingChoice()) || (mMissions != null && mMissions.IsAwaitingChoice());
        if (mShowMissionButton != null) mShowMissionButton.interactable = mMissions != null && mMissions.HasMission() && !decisionOpen && (mCards == null || !mCards.IsDecisionReviewMode) && !mTurns.IsGameOver();
        bool chinese = mCards != null && mCards.UseChinese;
        if (chinese)
        {
            mHudText.text = "回合 " + mTurns.GetTurnCount() + "/" + mTurns.GetMaxTurnCount() + "    大臣 " + mTurns.GetMinistersLeft() + "/" + mTurns.GetMaxMinisters() + "    金币 " + mStats.GetGold() + "    威严 " + mStats.GetMajesty() + "    战斗力 " + mStats.GetFight() + "\n" +
                            "资源  民意 " + mStats.GetPO() + "  军力 " + mStats.GetMS() + "  权威 " + mStats.GetAL() + "      声望  王室 " + mStats.GetKR() + "  教会 " + mStats.GetCR() + "  大贵族 " + mStats.GetAR();
        }
        else
        {
            mHudText.text = "TURN " + mTurns.GetTurnCount() + "/" + mTurns.GetMaxTurnCount() + "    MINISTERS " + mTurns.GetMinistersLeft() + "/" + mTurns.GetMaxMinisters() + "    GOLD " + mStats.GetGold() + "    MAJESTY " + mStats.GetMajesty() + "    FIGHT " + mStats.GetFight() + "\n" +
                            "RESOURCES  PO " + mStats.GetPO() + "  MS " + mStats.GetMS() + "  AL " + mStats.GetAL() + "      REPUTATION  KING " + mStats.GetKR() + "  CHURCH " + mStats.GetCR() + "  ARISTOCRATS " + mStats.GetAR();
        }
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

    // 在主 Canvas 顶部创建不会拦截鼠标的两行属性 HUD。
    private void BuildHud(Transform canvasTransform)
    {
        GameObject panelObject = new GameObject("Game HUD", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelObject.transform.SetParent(canvasTransform, false);
        panelObject.transform.SetAsFirstSibling();
        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.offsetMin = new Vector2(12f, -82f);
        panelRect.offsetMax = new Vector2(-220f, -10f);
        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = GameUITheme.DeepGreen;
        panelImage.raycastTarget = false;
        Outline outline = panelObject.AddComponent<Outline>();
        outline.effectColor = GameUITheme.Gold;
        outline.effectDistance = new Vector2(2f, -2f);

        GameObject textObject = new GameObject("HUD Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(panelObject.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(14f, 6f);
        textRect.offsetMax = new Vector2(-14f, -6f);
        mHudText = textObject.GetComponent<Text>();
        mHudText.font = GameUITheme.GetLegacyFont();
        mHudText.fontSize = 15;
        mHudText.fontStyle = FontStyle.Bold;
        mHudText.color = Color.white;
        mHudText.alignment = TextAnchor.MiddleLeft;
        mHudText.horizontalOverflow = HorizontalWrapMode.Wrap;
        mHudText.verticalOverflow = VerticalWrapMode.Truncate;
        mHudText.raycastTarget = false;
        mHudText.resizeTextForBestFit = true;
        mHudText.resizeTextMinSize = 12;
        mHudText.resizeTextMaxSize = 15;
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
