using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the single-turn tutorial in TutorialScene.
///
/// Design: this component only OBSERVES the game (polling the controllers' public
/// getters) and narrates through a top-anchored banner plus target highlights.
/// It never drives gameplay, so the normal game logic is untouched and the tutorial
/// can be removed or skipped freely.
///
/// Fixed play order enforced this turn:
///   1. Play TaxLedger (starting-deck element 1) at the Palace.
///   2. Play CourtPolitics (starting-deck element 0) at the Embassy.
/// Then after Reveal, Buy and Delete are highlighted; once the player opens Buy or
/// Delete, the highlight moves to the phase button so they advance to the mission.
///
/// Gating is soft ("guide, allow exploration"): the player may hover/inspect freely,
/// but on action steps the turn-phase button is held disabled (from LateUpdate, which
/// runs after TurnPhaseButton.Update) until the step is satisfied.
///
/// Setup in Unity:
///   1. Empty GameObject in TutorialScene named "Tutorial Director" + this component.
///   2. Leave the controller fields empty to auto-find them.
///   3. Drag the Palace and Embassy ClickOnLocation objects into their fields.
///   4. Confirm the two deck indices match your starting deck order.
/// </summary>
public class TutorialDirector : MonoBehaviour
{
    [Header("Controllers (auto-found if left empty)")]
    [SerializeField] private ZebraGameController cards;
    [SerializeField] private TurnController turns;
    [SerializeField] private EventManager events;
    [SerializeField] private MissionManager missions;

    [Header("Tutorial locations (required for the two play steps)")]
    [SerializeField] private ClickOnLocation palaceLocation;
    [SerializeField] private ClickOnLocation embassyLocation;

    [Header("Starting-deck indices")]
    [Tooltip("TaxLedger is element 1 of the starting deck by default.")]
    [SerializeField] private int taxLedgerDeckIndex = 1;
    [Tooltip("CourtPolitics is element 0 of the starting deck by default.")]
    [SerializeField] private int courtPoliticsDeckIndex = 0;

    [Header("Banner placement (top-center by default)")]
    [SerializeField] private Vector2 bannerAnchoredPosition = new Vector2(0f, -270f);
    [SerializeField] private Vector2 bannerSize = new Vector2(780f, 84f);

    [Header("Replay")]
    [Tooltip("If true, the tutorial runs only once per machine (tracked in PlayerPrefs).")]
    [SerializeField] private bool playOnlyOnce = false;
    [SerializeField] private string playerPrefsKey = "ZebraTutorialDone";

    // ---- one tutorial step ----
    private class Step
    {
        public string en;
        public string cn;
        public bool showNext;         // true => a Next/Finish button advances manually (informational step)
        public bool holdPhaseButton;  // true => force the phase button disabled until IsComplete()
        public bool isFinish;         // last step: the button returns to the main menu
        public Func<bool> isComplete; // auto-advance when true (ignored if showNext)
        public Action onEnter;
        public Action onExit;
    }

    private readonly List<Step> mSteps = new List<Step>();
    private int mIndex = -1;
    private bool mFinished;

    // banner UI
    private Canvas mCanvas;
    private Text mText;
    private Button mNextButton;
    private Text mNextLabel;
    private Font mFont;
    private bool mChinese;

    // highlight glow pool (supports several UI targets at once, e.g. Buy + Delete)
    private readonly List<Image> mGlows = new List<Image>();
    private readonly List<RectTransform> mGlowTargets = new List<RectTransform>();
    private ClickOnLocation mLocTarget;

    // phase button
    private Button mPhaseButton;
    private RectTransform mPhaseButtonRect;

    // per-step scratch
    private int mMinisterBaseline;
    private int mTurnBaseline;
    private bool mSawMissionAwaiting;
    private bool mClickedBuyOrDelete;
    private Button mBuyBtn;
    private Button mDeleteBtn;

    private void Start()
    {
        if (playOnlyOnce && PlayerPrefs.GetInt(playerPrefsKey, 0) == 1) { enabled = false; return; }

        if (cards == null) cards = FindAnyObjectByType<ZebraGameController>();
        if (turns == null) turns = FindAnyObjectByType<TurnController>();
        if (events == null) events = FindAnyObjectByType<EventManager>();
        if (missions == null) missions = FindAnyObjectByType<MissionManager>();

        TurnPhaseButton tpb = FindAnyObjectByType<TurnPhaseButton>();
        if (tpb != null)
        {
            mPhaseButton = tpb.GetComponent<Button>();
            mPhaseButtonRect = tpb.GetComponent<RectTransform>();
        }

        // Same CJK-capable font the difficulty panel uses, so Chinese never shows as boxes.
        mFont = Resources.Load<Font>("Fonts/NotoSansSC");
        if (mFont == null) mFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        mChinese = cards != null && cards.UseChinese;
        if (cards != null) cards.LanguageChanged += OnLanguageChanged;

        BuildUI();
        BuildSteps();
        GoTo(0);
    }

    private void OnDestroy()
    {
        if (cards != null) cards.LanguageChanged -= OnLanguageChanged;
        UnhookBuyDelete();
    }

    private void OnLanguageChanged(bool chinese)
    {
        mChinese = chinese;
        RefreshText();
    }

    // ---------------------------------------------------------------- steps
    private void BuildSteps()
    {
        mSteps.Add(new Step
        {
            en = "Greetings, my lord! Let me show you around the realm in one turn. Press Next to begin.",
            cn = "向您致意，大人！让我花上一个回合带您熟悉领地。点击“下一步”开始。",
            showNext = true, holdPhaseButton = true
        });

        mSteps.Add(new Step
        {
            en = "Every turn starts with an event. Click the glowing turn-phase button to face it.",
            cn = "每个回合都从一个事件开始。点击发光的回合阶段按钮来面对它。",
            onEnter = () => HighlightUi(mPhaseButtonRect),
            onExit = () => HighlightUi(),
            isComplete = () => events != null && events.IsAwaitingChoice()
        });

        mSteps.Add(new Step
        {
            en = "Read the event, then click one of its options. Your choice sets this turn's mission.",
            cn = "阅读事件，然后点击其中一个选项。你的选择会决定本回合的任务。",
            isComplete = () => events == null || !events.IsAwaitingChoice()
        });

        // Fixed order 1: TaxLedger -> Palace
        mSteps.Add(new Step
        {
            en = "Try to play the highlighted card in your hand. Click it once to select the card, click it again to pick a location. Then click the highlighted Palace.",
            cn = "试试打出你手中的高亮卡牌。点击卡牌一次来选中它，再点击一次来选择地点。之后点击地图上高亮的宫殿。",
            holdPhaseButton = true,
            onEnter = () =>
            {
                mMinisterBaseline = turns != null ? turns.GetMinistersLeft() : 0;
                HighlightUi(HandCardRect(taxLedgerDeckIndex));
                HighlightLocation(palaceLocation);
            },
            onExit = () => { HighlightUi(); HighlightLocation(null); },
            isComplete = () => TargetVisited(palaceLocation)
        });

        // Fixed order 2: CourtPolitics -> Embassy
        mSteps.Add(new Step
        {
            en = "Now play the other card, sending your minister to the highlighted Embassy. Try to right click any location to see its effects.",
            cn = "现在打出另一张牌，把大臣派到高亮的使馆上。试试鼠标右键点击地块来查看地块效果。",
            holdPhaseButton = true,
            onEnter = () =>
            {
                mMinisterBaseline = turns != null ? turns.GetMinistersLeft() : 0;
                HighlightUi(HandCardRect(courtPoliticsDeckIndex));
                HighlightLocation(embassyLocation);
            },
            onExit = () => { HighlightUi(); HighlightLocation(null); },
            isComplete = () => TargetVisited(embassyLocation)
        });

        mSteps.Add(new Step
        {
            en = "Thank you, my lord. Now click the highlighted turn-phase button to reveal your unplayed cards, though you don't have any.",
            cn = "感谢您，大人。现在点击高亮的回合阶段按钮揭示未打出的手牌，尽管目前您手里没有牌了。",
            onEnter = () => HighlightUi(mPhaseButtonRect),
            onExit = () => HighlightUi(),
            isComplete = () => turns != null && turns.CheckTurnPhase() >= 2
        });

        // Buy phase: highlight Buy + Delete until one is opened.
        mSteps.Add(new Step
        {
            en = "We're in the middle of reveal phase now. Check Buy (add a card) or Delete (remove one) — both are highlighted.",
            cn = "现在我们进入了展示阶段。看看“购买”（购买新牌加入弃牌堆）或“删除”（从牌库中移除卡牌）——两者都已高亮。",
            holdPhaseButton = true,
            onEnter = () =>
            {
                mClickedBuyOrDelete = false;
                RectTransform buy = cards != null ? cards.BuyButtonRect : null;
                RectTransform del = cards != null ? cards.DeleteButtonRect : null;
                HookBuyDelete(buy, del);
                HighlightUi(buy, del);
            },
            onExit = () => { UnhookBuyDelete(); HighlightUi(); },
            isComplete = () => mClickedBuyOrDelete
        });

        // After Buy/Delete, highlight moves to the phase button ("Reveal").
        mSteps.Add(new Step
        {
            en = "You didn't gain Majesty this turn, so you can't buy or delete cards. Just click the turn-phase button to face the mission.",
            cn = "本回合你没有获得威严，所以你无法买牌或删牌。那就点击回合阶段按钮来面对任务吧。",
            onEnter = () => HighlightUi(mPhaseButtonRect),
            onExit = () => HighlightUi(),
            isComplete = () => turns != null && turns.CheckTurnPhase() >= 3
        });

        mSteps.Add(new Step
        {
            en = "Resolve the mission by choosing one of its resolutions. Usually, you must choose from 'accept' or 'reject'. ",
            cn = "选择一个处理方式来完成任务。通常您只能从“接受”和“拒绝”中选择一个。",
            onEnter = () => mSawMissionAwaiting = false,
            isComplete = () =>
            {
                if (missions == null || !missions.HasMission()) return true;
                if (missions.IsAwaitingChoice()) { mSawMissionAwaiting = true; return false; }
                return mSawMissionAwaiting; // was awaiting a choice, now resolved
            }
        });

        // After the mission is resolved, guide the player to end the turn (the phase
        // button now reads "End Turn"). Don't jump to the menu yet.
        mSteps.Add(new Step
        {
            en = "The mission is handled. Click the End Turn button to close out the turn.",
            cn = "任务已处理。点击“结束回合”按钮来结束本回合。",
            onEnter = () =>
            {
                mTurnBaseline = turns != null ? turns.GetTurnCount() : 0;
                HighlightUi(mPhaseButtonRect);
            },
            onExit = () => HighlightUi(),
            isComplete = () => turns != null && (turns.GetTurnCount() > mTurnBaseline || turns.IsGameOver())
        });

        mSteps.Add(new Step
        {
            en = "You won, my lord! Congratulations. But you know it's only a dream, do you? It's time to return to the main menu and start a real ten-turn game.",
            cn = "您获胜了，大人！恭喜。但你知道这只是一个梦，对吧？胜利不会如此轻松。现在是时候返回主菜单开始一局真正的十回合游戏了。",
            showNext = true, holdPhaseButton = true, isFinish = true
        });
    }

    private RectTransform HandCardRect(int deckIndex)
    {
        if (cards == null) return null;
        return cards.GetHandCardRect(cards.GetStartingDeckCard(deckIndex));
    }

    // A location step is complete when its location is visited. If no location is assigned,
    // fall back to detecting that any minister was spent (a card was played).
    private bool TargetVisited(ClickOnLocation loc)
    {
        if (loc != null) return !loc.IsAvailable();
        return turns != null && turns.GetMinistersLeft() < mMinisterBaseline;
    }

    private void GoTo(int index)
    {
        if (mIndex >= 0 && mIndex < mSteps.Count) mSteps[mIndex].onExit?.Invoke();

        mIndex = index;
        if (mIndex >= mSteps.Count) { Finish(); return; }

        Step s = mSteps[mIndex];
        s.onEnter?.Invoke();
        RefreshText();

        mNextButton.gameObject.SetActive(s.showNext);
        if (s.showNext) mNextLabel.text = s.isFinish ? (mChinese ? "完成" : "Finish") : (mChinese ? "下一步" : "Next");
    }

    private void Advance()
    {
        if (mIndex < 0 || mIndex >= mSteps.Count) return;
        if (mSteps[mIndex].isFinish) { Finish(); return; }
        GoTo(mIndex + 1);
    }

    private void Finish()
    {
        if (mFinished) return;
        mFinished = true;
        HighlightUi();
        HighlightLocation(null);
        UnhookBuyDelete();
        if (playOnlyOnce) PlayerPrefs.SetInt(playerPrefsKey, 1);
        if (mCanvas != null) Destroy(mCanvas.gameObject);

        LoadScene loader = FindAnyObjectByType<LoadScene>();
        if (loader == null) loader = new GameObject("LoadScene").AddComponent<LoadScene>();
        loader.LoadMainMenu();
    }

    private void Update()
    {
        if (mFinished || mIndex < 0 || mIndex >= mSteps.Count) return;
        PulseGlow();

        Step s = mSteps[mIndex];
        if (!s.showNext && s.isComplete != null && s.isComplete()) Advance();
    }

    // Runs after TurnPhaseButton.Update, so this reliably wins for the frame.
    private void LateUpdate()
    {
        if (mFinished || mPhaseButton == null || mIndex < 0 || mIndex >= mSteps.Count) return;
        if (mSteps[mIndex].holdPhaseButton) mPhaseButton.interactable = false;
    }

    // ---------------------------------------------------------------- buy / delete hook
    private void HookBuyDelete(RectTransform buy, RectTransform del)
    {
        UnhookBuyDelete();
        mBuyBtn = buy != null ? buy.GetComponent<Button>() : null;
        mDeleteBtn = del != null ? del.GetComponent<Button>() : null;
        if (mBuyBtn != null) mBuyBtn.onClick.AddListener(OnBuyOrDeleteClicked);
        if (mDeleteBtn != null) mDeleteBtn.onClick.AddListener(OnBuyOrDeleteClicked);
    }

    private void UnhookBuyDelete()
    {
        if (mBuyBtn != null) mBuyBtn.onClick.RemoveListener(OnBuyOrDeleteClicked);
        if (mDeleteBtn != null) mDeleteBtn.onClick.RemoveListener(OnBuyOrDeleteClicked);
        mBuyBtn = null;
        mDeleteBtn = null;
    }

    private void OnBuyOrDeleteClicked() { mClickedBuyOrDelete = true; }

    // ---------------------------------------------------------------- highlight
    private void HighlightUi(params RectTransform[] targets)
    {
        mGlowTargets.Clear();
        if (targets != null)
            foreach (RectTransform t in targets)
                if (t != null) mGlowTargets.Add(t);

        EnsureGlowPool(mGlowTargets.Count);
        for (int i = 0; i < mGlows.Count; i++)
            mGlows[i].enabled = i < mGlowTargets.Count;
    }

    private void EnsureGlowPool(int count)
    {
        while (mGlows.Count < count)
        {
            Image g = CreateImage("Highlight", mCanvas.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(120f, 60f), new Color(1f, 0.83f, 0.28f, 0.35f));
            g.raycastTarget = false;
            g.enabled = false;
            g.transform.SetAsFirstSibling(); // behind the banner
            mGlows.Add(g);
        }
    }

    private void HighlightLocation(ClickOnLocation loc)
    {
        if (mLocTarget != null && mLocTarget != loc) mLocTarget.SetHighlighted(false);
        mLocTarget = loc;
        if (loc != null) loc.SetHighlighted(true);
    }

    private void PulseGlow()
    {
        float a = 0.30f + 0.22f * Mathf.Sin(Time.unscaledTime * 4.2f);
        Vector3[] corners = new Vector3[4];
        for (int i = 0; i < mGlows.Count; i++)
        {
            Image g = mGlows[i];
            if (!g.enabled) continue;
            RectTransform target = i < mGlowTargets.Count ? mGlowTargets[i] : null;
            if (target == null) { g.enabled = false; continue; }

            // Overlay canvases use screen pixels as world space, so world corners map 1:1
            // even though the target lives on a different canvas.
            target.GetWorldCorners(corners);
            g.rectTransform.position = (corners[0] + corners[2]) * 0.5f;
            float w = Vector3.Distance(corners[0], corners[3]);
            float h = Vector3.Distance(corners[0], corners[1]);
            g.rectTransform.sizeDelta = new Vector2(w + 28f, h + 28f);
            Color c = g.color; c.a = a; g.color = c;
        }
    }

    // ---------------------------------------------------------------- UI build
    private void BuildUI()
    {
        GameObject canvasObject = new GameObject("Tutorial Canvas", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        mCanvas = canvasObject.GetComponent<Canvas>();
        mCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        mCanvas.sortingOrder = 1100; // above the game's overlays (500) and the ending panel (1000)

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Image banner = CreateImage("Banner", mCanvas.transform, new Vector2(0.5f, 1f), bannerAnchoredPosition, bannerSize, new Color(0.06f, 0.05f, 0.04f, 0.92f));
        banner.raycastTarget = true;

        mText = CreateText("Instruction", banner.transform, "", new Vector2(0f, 0f), new Vector2(0.72f, 1f), new Vector2(24f, 8f), new Vector2(-12f, -8f), 22, FontStyle.Bold, TextAnchor.MiddleLeft);
        mText.color = new Color(0.97f, 0.94f, 0.85f);

        mNextButton = CreateButton("Next", banner.transform, "Next", new Vector2(1f, 0.5f), new Vector2(-88f, 0f), new Vector2(140f, 52f), new Color(0.20f, 0.42f, 0.30f), out mNextLabel);
        mNextButton.onClick.AddListener(Advance);
        mNextButton.gameObject.SetActive(false);
    }

    private void RefreshText()
    {
        if (mText == null || mIndex < 0 || mIndex >= mSteps.Count) return;
        Step s = mSteps[mIndex];
        mText.text = mChinese ? s.cn : s.en;
        if (s.showNext && mNextLabel != null)
            mNextLabel.text = s.isFinish ? (mChinese ? "完成" : "Finish") : (mChinese ? "下一步" : "Next");
    }

    // ---------------------------------------------------------------- helpers
    private Image CreateImage(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Image image = go.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private Text CreateText(string name, Transform parent, string value, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, int size, FontStyle style, TextAnchor align)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        Text text = go.GetComponent<Text>();
        text.font = mFont;
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = align;
        text.color = new Color(0.96f, 0.92f, 0.8f);
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    private Button CreateButton(string name, Transform parent, string label, Vector2 anchor, Vector2 position, Vector2 size, Color color, out Text labelText)
    {
        Image image = CreateImage(name, parent, anchor, position, size, color);
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        labelText = CreateText("Label", image.transform, label, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 19, FontStyle.Bold, TextAnchor.MiddleCenter);
        labelText.color = Color.white;
        return button;
    }
}
