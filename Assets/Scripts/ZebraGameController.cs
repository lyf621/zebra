using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ZebraGameController : MonoBehaviour
{
    private enum GamePhase
    {
        WaitingToStart,
        PlayerAction,
        ChoosingLocation,
        Animating
    }

    private enum OverlayMode
    {
        View,
        Delete,
        Market
    }

    private const int kDrawPerRound = 5;
    private const int kMaximumHandSize = 10;
    private const int kMaximumMinisters = 2;

    private readonly List<CardModel> mOwnedCards = new List<CardModel>();
    private readonly List<CardModel> mDrawPile = new List<CardModel>();
    private readonly List<CardModel> mDiscardPile = new List<CardModel>();
    private readonly List<CardModel> mHand = new List<CardModel>();
    private readonly List<CardModel> mMarketCards = new List<CardModel>();
    private readonly Dictionary<CardModel, CardView> mHandViews = new Dictionary<CardModel, CardView>();
    private readonly List<LocationView> mLocations = new List<LocationView>();

    private Font mFont;
    private Canvas mCanvas;
    private RectTransform mCanvasRect;
    private RectTransform mHandLayer;
    private Text mStatsText;
    private Text mRoundText;
    private Text mStatusText;
    private Text mDrawCountText;
    private Text mDiscardCountText;
    private Text mDrawPileNameText;
    private Text mDiscardPileNameText;
    private Text mInHandText;
    private Button mDrawPileButton;
    private Button mDiscardPileButton;
    private Button mAllCardsButton;
    private Button mBuyButton;
    private Button mDeleteButton;
    private Button mEndRoundButton;
    private Button mCancelPlayButton;
    private Button mStartRoundButton;
    private Button mSettingsButton;
    private GameObject mOverlay;
    private GameObject mSettingsOverlay;
    private List<CardModel> mOverlayCards;
    private CardModel mOverlaySelectedCard;
    private OverlayMode mOverlayMode;
    private string mOverlayTitleEnglish;
    private string mOverlayTitleChinese;
    private string mStatusEnglish;
    private string mStatusChinese;
    private CardView mSelectedCardView;
    private CardModel mPendingCard;
    private GamePhase mPhase = GamePhase.WaitingToStart;
    private int mNextCardId = 1;
    private int mRoundNumber = 1;
    private int mMinistersLeft = kMaximumMinisters;
    private int mGold = 3;
    private int mPublicOpinion = 5;
    private int mMilitaryStrength = 5;
    private int mAuthorityLevel = 5;
    private bool mUseChinese = false;

    public bool UseChinese => mUseChinese;

    private void Start()
    {
        mFont = Resources.Load<Font>("Fonts/NotoSansSC");
        if (mFont == null)
        {
            mFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        BuildInterface();
        CreateInitialCards();
        RefreshInterface();
        mStartRoundButton.gameObject.SetActive(true);
        SetStatus("Press Start Round to draw five cards.", "点击开始回合并抽取五张牌。");
    }

    // 左键选中或打出卡牌，右键取消当前选中的卡牌。
    public void OnHandCardClicked(CardView cardView, PointerEventData.InputButton button)
    {
        if (mOverlay != null || mSettingsOverlay != null || mPhase != GamePhase.PlayerAction)
        {
            return;
        }

        if (button == PointerEventData.InputButton.Right)
        {
            if (mSelectedCardView == cardView && cardView.IsFollowingPointer)
            {
                ReturnSelectedCardToHand("Card selection cancelled.", "已取消选择卡牌。");
            }
            return;
        }

        if (button != PointerEventData.InputButton.Left)
        {
            return;
        }

        if (mSelectedCardView == cardView && cardView.IsFollowingPointer)
        {
            cardView.StopFollowingPointer();
            BeginPlaySelectedCard();
            return;
        }

        SelectHandCard(cardView);
    }

    // 只有玩家行动阶段的未锁定手牌可以响应悬停效果。
    public bool CanHoverCard(CardView cardView)
    {
        return mOverlay == null && mSettingsOverlay == null && mPhase == GamePhase.PlayerAction && (mSelectedCardView == null || mSelectedCardView == cardView);
    }

    // 选择允许的空地点后，放置大臣并结算地点效果。
    public void OnLocationClicked(LocationView location)
    {
        if (mOverlay != null || mSettingsOverlay != null || mPhase != GamePhase.ChoosingLocation || mPendingCard == null)
        {
            return;
        }

        if (location.IsOccupied)
        {
            ReturnSelectedCardToHand("That location is occupied. Card returned to hand.", "该地点已被占用，卡牌已返回手牌。");
            return;
        }

        if (!CardCanUseLocation(mPendingCard, location.Type))
        {
            ReturnSelectedCardToHand("That location cannot use this card. Card returned to hand.", "该地点不能使用这张牌，卡牌已返回手牌。");
            return;
        }

        StartCoroutine(ResolvePlayedCardRoutine(location));
    }

    // 创建响应式 Canvas 和游戏所需的全部运行时 UI。
    private void BuildInterface()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystemObject.transform.SetParent(transform, false);
        }

        GameObject canvasObject = new GameObject("Game Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        mCanvas = canvasObject.GetComponent<Canvas>();
        mCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        mCanvasRect = canvasObject.GetComponent<RectTransform>();

        GameObject backgroundObject = new GameObject("Paper Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        backgroundObject.transform.SetParent(canvasObject.transform, false);
        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        StretchToParent(backgroundRect);
        RawImage background = backgroundObject.GetComponent<RawImage>();
        background.texture = Resources.Load<Texture2D>("Art/PaperBackground");
        background.color = new Color(0.78f, 0.72f, 0.61f, 1f);
        background.raycastTarget = false;
        background.enabled = !IntegrationPlaceholderMode.Enabled;

        Image shade = CreatePanel("Background Shade", canvasObject.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.08f, 0.09f, 0.08f, 0.28f));
        shade.raycastTarget = false;
        shade.enabled = !IntegrationPlaceholderMode.Enabled;
        CreatePanel("Top Bar", canvasObject.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -35f), new Vector2(0f, 70f), new Color(0.09f, 0.11f, 0.1f, 0.94f));

        mStatsText = CreateText("Stats", canvasObject.transform, "", 17, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -35f), new Vector2(570f, 44f), Color.white);
        mStatsText.rectTransform.pivot = new Vector2(0f, 0.5f);
        mRoundText = CreateText("Round", canvasObject.transform, "", 18, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -35f), new Vector2(300f, 44f), Color.white);
        mAllCardsButton = CreateButton("All Cards", canvasObject.transform, "All Cards", new Vector2(1f, 1f), new Vector2(-72f, -35f), new Vector2(112f, 38f), new Color(0.2f, 0.24f, 0.21f));
        mAllCardsButton.GetComponent<RectTransform>().pivot = new Vector2(1f, 0.5f);
        mAllCardsButton.onClick.AddListener(OpenAllCardsView);
        mSettingsButton = CreateButton("Settings", canvasObject.transform, "", new Vector2(1f, 1f), new Vector2(-20f, -35f), new Vector2(40f, 38f), new Color(0.28f, 0.29f, 0.27f));
        mSettingsButton.GetComponent<RectTransform>().pivot = new Vector2(1f, 0.5f);
        Texture2D settingsTexture = Resources.Load<Texture2D>("Art/SettingsIcon");
        if (settingsTexture != null)
        {
            Image settingsIcon = CreatePanel("Settings Icon", mSettingsButton.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(24f, 24f), Color.white);
            settingsIcon.sprite = Sprite.Create(settingsTexture, new Rect(0f, 0f, settingsTexture.width, settingsTexture.height), new Vector2(0.5f, 0.5f), 100f);
            settingsIcon.preserveAspect = true;
            settingsIcon.raycastTarget = false;
        }
        mSettingsButton.onClick.AddListener(OpenSettings);

        RectTransform boardLayer = CreateLayer("Board Layer", canvasObject.transform);
        mLocations.Add(LocationView.Create(boardLayer, mFont, this, LocationType.Economy, "Village", "村庄", IntegrationPlaceholderMode.Enabled ? "" : "Gold +1\nPO +1", IntegrationPlaceholderMode.Enabled ? "" : "金币 +1\n民意 +1", new Vector2(-280f, 105f), new Color(0.18f, 0.43f, 0.31f)));
        mLocations.Add(LocationView.Create(boardLayer, mFont, this, LocationType.Military, "Barracks", "兵营", IntegrationPlaceholderMode.Enabled ? "" : "MS +2", IntegrationPlaceholderMode.Enabled ? "" : "军力 +2", new Vector2(0f, 105f), new Color(0.55f, 0.22f, 0.19f)));
        mLocations.Add(LocationView.Create(boardLayer, mFont, this, LocationType.Administration, "Council", "议会", IntegrationPlaceholderMode.Enabled ? "" : "AL +2", IntegrationPlaceholderMode.Enabled ? "" : "权威 +2", new Vector2(280f, 105f), new Color(0.25f, 0.34f, 0.47f)));

        mStatusText = CreateText("Status", canvasObject.transform, "", 16, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -82f), new Vector2(700f, 38f), new Color(1f, 0.95f, 0.78f));
        mInHandText = CreateText("In Hand", canvasObject.transform, "IN HAND", 15, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 18f), new Vector2(240f, 28f), Color.white);

        mDrawPileButton = CreatePileButton("Draw Pile", canvasObject.transform, "DECK", new Vector2(-550f, -128f), out mDrawCountText, out mDrawPileNameText);
        mDrawPileButton.onClick.AddListener(OpenDrawPileView);
        mDiscardPileButton = CreatePileButton("Discard Pile", canvasObject.transform, "DISCARD", new Vector2(550f, -128f), out mDiscardCountText, out mDiscardPileNameText);
        mDiscardPileButton.onClick.AddListener(OpenDiscardPileView);

        mBuyButton = CreateButton("Buy", canvasObject.transform, "Buy", new Vector2(1f, 0.5f), new Vector2(-24f, 126f), new Vector2(120f, 42f), new Color(0.56f, 0.43f, 0.13f));
        mBuyButton.GetComponent<RectTransform>().pivot = new Vector2(1f, 0.5f);
        mBuyButton.onClick.AddListener(OpenMarket);
        mDeleteButton = CreateButton("Delete", canvasObject.transform, "Delete", new Vector2(1f, 0.5f), new Vector2(-24f, 72f), new Vector2(120f, 42f), new Color(0.39f, 0.2f, 0.18f));
        mDeleteButton.GetComponent<RectTransform>().pivot = new Vector2(1f, 0.5f);
        mDeleteButton.onClick.AddListener(OpenDeleteView);
        mEndRoundButton = CreateButton("End Round", canvasObject.transform, "End Round", new Vector2(1f, 0.5f), new Vector2(-24f, 18f), new Vector2(120f, 42f), new Color(0.17f, 0.31f, 0.38f));
        mEndRoundButton.GetComponent<RectTransform>().pivot = new Vector2(1f, 0.5f);
        mEndRoundButton.onClick.AddListener(EndRound);
        mCancelPlayButton = CreateButton("Cancel Play", canvasObject.transform, "Cancel", new Vector2(1f, 0.5f), new Vector2(-24f, -36f), new Vector2(120f, 42f), new Color(0.32f, 0.31f, 0.29f));
        mCancelPlayButton.GetComponent<RectTransform>().pivot = new Vector2(1f, 0.5f);
        mCancelPlayButton.onClick.AddListener(CancelPendingPlay);

        mHandLayer = CreateLayer("Hand Layer", canvasObject.transform);
        mStartRoundButton = CreateButton("Start Round", canvasObject.transform, "Start Round", new Vector2(0.5f, 0.5f), new Vector2(0f, -14f), new Vector2(190f, 54f), new Color(0.18f, 0.39f, 0.28f));
        mStartRoundButton.onClick.AddListener(BeginFirstRound);
    }

    // 建立初始牌库和三张可购买的皇家牌。
    private void CreateInitialCards()
    {
        AddInitialCard("Village Charter", "村庄宪章", "Place a minister in Economy.", "在经济地点放置一名大臣。", LocationType.Economy, RetainEffectType.None);
        AddInitialCard("Harvest Plan", "收获计划", "Place a minister in Economy.", "在经济地点放置一名大臣。", LocationType.Economy, RetainEffectType.None);
        AddInitialCard("Barracks Order", "兵营命令", "Place a minister in Military.", "在军事地点放置一名大臣。", LocationType.Military, RetainEffectType.None);
        AddInitialCard("Drill Schedule", "训练日程", "Place a minister in Military.", "在军事地点放置一名大臣。", LocationType.Military, RetainEffectType.None);
        AddInitialCard("Royal Seal", "皇家印章", "Place a minister in Administration.", "在行政地点放置一名大臣。", LocationType.Administration, RetainEffectType.None);
        AddInitialCard("Clerk's Report", "书记官报告", "Place a minister in Administration.", "在行政地点放置一名大臣。", LocationType.Administration, RetainEffectType.None);
        AddInitialCard("Public Feast", "公众宴会", "If retained: PO +1.", "保留时：民意 +1。", LocationType.Economy, RetainEffectType.PublicOpinionUp);
        AddInitialCard("Forced Levy", "强制征募", "If retained: MS -1.", "保留时：军力 -1。", LocationType.Military, RetainEffectType.MilitaryStrengthDown);
        AddInitialCard("Tax Ledger", "税务账册", "Place a minister in Economy.", "在经济地点放置一名大臣。", LocationType.Economy, RetainEffectType.None);
        AddInitialCard("Council Petition", "议会请愿", "Place a minister in Administration.", "在行政地点放置一名大臣。", LocationType.Administration, RetainEffectType.None);
        mDrawPile.AddRange(mOwnedCards);
        Shuffle(mDrawPile);

        mMarketCards.Add(CreateCard("Royal Decree", "皇家法令", "May use any location.", "可以进入任意地点。", LocationType.Any, RetainEffectType.None, true));
        mMarketCards.Add(CreateCard("Crown Levy", "王室征税", "Military order from the crown.", "来自王室的军事命令。", LocationType.Military, RetainEffectType.None, true));
        mMarketCards.Add(CreateCard("Royal Pardon", "皇家赦免", "Administrative order from the crown.", "来自王室的行政命令。", LocationType.Administration, RetainEffectType.None, true));
    }

    private void AddInitialCard(string nameEnglish, string nameChinese, string descriptionEnglish, string descriptionChinese, LocationType location, RetainEffectType retainEffect)
    {
        mOwnedCards.Add(CreateCard(nameEnglish, nameChinese, descriptionEnglish, descriptionChinese, location, retainEffect, false));
    }

    private CardModel CreateCard(string nameEnglish, string nameChinese, string descriptionEnglish, string descriptionChinese, LocationType location, RetainEffectType retainEffect, bool isRoyal)
    {
        if (IntegrationPlaceholderMode.Enabled)
        {
            nameEnglish = "";
            nameChinese = "";
            descriptionEnglish = "";
            descriptionChinese = "";
            location = LocationType.Any;
            retainEffect = RetainEffectType.None;
            isRoyal = false;
        }
        return new CardModel { InstanceId = mNextCardId++, NameEnglish = nameEnglish, NameChinese = nameChinese, DescriptionEnglish = descriptionEnglish, DescriptionChinese = descriptionChinese, Location = location, RetainEffect = retainEffect, IsRoyal = isRoyal };
    }

    private void BeginFirstRound()
    {
        if (mPhase != GamePhase.WaitingToStart)
        {
            return;
        }

        mStartRoundButton.gameObject.SetActive(false);
        StartCoroutine(StartRoundRoutine());
    }

    // 开始新回合，重置地点和大臣，然后抽到五张手牌。
    private IEnumerator StartRoundRoutine()
    {
        mPhase = GamePhase.Animating;
        mMinistersLeft = kMaximumMinisters;
        foreach (LocationView location in mLocations)
        {
            location.SetOccupied(false);
            location.SetHighlighted(false);
        }

        SetStatus("Round " + mRoundNumber + ": drawing cards...", "第 " + mRoundNumber + " 回合：正在抽牌……");
        int drawCount = Mathf.Min(kDrawPerRound, kMaximumHandSize - mHand.Count);
        for (int i = 0; i < drawCount && (mDrawPile.Count > 0 || mDiscardPile.Count > 0); i++)
        {
            yield return DrawOneCardRoutine();
        }

        mPhase = GamePhase.PlayerAction;
        SetStatus("Hover over a card. Click once to hold it, then click again to play.", "将鼠标移到卡牌上。点击一次拿起，再次点击打出。");
        RefreshInterface();
    }

    // 从抽牌堆顶部抽一张牌；抽牌堆为空时先重洗弃牌堆。
    private IEnumerator DrawOneCardRoutine()
    {
        if (mDrawPile.Count == 0 && mDiscardPile.Count > 0)
        {
            SetStatus("Reshuffling the discard pile...", "正在将弃牌堆重新洗入抽牌堆……");
            yield return new WaitForSeconds(0.28f);
            mDrawPile.AddRange(mDiscardPile);
            mDiscardPile.Clear();
            Shuffle(mDrawPile);
            RefreshInterface();
        }

        if (mDrawPile.Count == 0)
        {
            yield break;
        }

        CardModel card = mDrawPile[mDrawPile.Count - 1];
        mDrawPile.RemoveAt(mDrawPile.Count - 1);
        mHand.Add(card);
        CardView cardView = CardView.Create(mHandLayer, mFont, card, this);
        cardView.SetInteractable(false);
        mHandViews.Add(card, cardView);
        LayoutHand();
        Vector2 targetPosition = cardView.RectTransform.anchoredPosition;
        cardView.RectTransform.anchoredPosition = GetCanvasPosition(mDrawPileButton.GetComponent<RectTransform>());
        yield return MoveRectRoutine(cardView.RectTransform, targetPosition, 0.16f, 24f);
        cardView.SetInteractable(true);
        RefreshInterface();
        yield return new WaitForSeconds(0.04f);
    }

    private void SelectHandCard(CardView cardView)
    {
        if (mSelectedCardView != null)
        {
            mSelectedCardView.SetSelected(false);
        }

        mSelectedCardView = cardView;
        mSelectedCardView.SetSelected(true);
        mSelectedCardView.BeginFollowingPointer();
        SetStatus("Click the held card again to play it.", "再次点击拿起的卡牌即可打出。");
    }

    private void BeginPlaySelectedCard()
    {
        if (mSelectedCardView == null)
        {
            SetStatus("Select a card first.", "请先选择一张卡牌。");
            return;
        }

        if (mMinistersLeft <= 0)
        {
            ReturnSelectedCardToHand("No ministers remain this round. Card returned to hand.", "本回合已没有可用大臣，卡牌已返回手牌。");
            return;
        }

        mPendingCard = mSelectedCardView.Card;
        mPhase = GamePhase.ChoosingLocation;
        int availableLocationCount = 0;
        foreach (LocationView location in mLocations)
        {
            bool canUseLocation = !location.IsOccupied && CardCanUseLocation(mPendingCard, location.Type);
            location.SetHighlighted(canUseLocation);
            if (canUseLocation)
            {
                availableLocationCount++;
            }
        }
        if (availableLocationCount == 0)
        {
            ReturnSelectedCardToHand("No matching empty location is available. Card returned to hand.", "没有可用的匹配空地点，卡牌已返回手牌。");
            return;
        }
        SetStatus("Choose a highlighted location for the minister.", "选择一个高亮地点放置大臣。");
        RefreshInterface();
    }

    private bool CardCanUseLocation(CardModel card, LocationType locationType)
    {
        return card.Location == LocationType.Any || card.Location == locationType;
    }

    // 将所选卡牌移动到地点，触发地点效果，再送入弃牌堆。
    private IEnumerator ResolvePlayedCardRoutine(LocationView location)
    {
        mPhase = GamePhase.Animating;
        CardModel card = mPendingCard;
        CardView view = mHandViews[card];
        mPendingCard = null;
        mSelectedCardView = null;
        mHand.Remove(card);
        mHandViews.Remove(card);
        foreach (LocationView item in mLocations)
        {
            item.SetHighlighted(false);
        }
        LayoutHand();

        PrepareCardForCanvasAnimation(view);
        Vector2 locationPosition = GetCanvasPosition(location.RectTransform);
        yield return MoveRectRoutine(view.RectTransform, locationPosition, 0.2f, 34f);
        location.SetOccupied(true);
        mMinistersLeft--;
        ApplyLocationEffect(location.Type);
        SetStatus(IntegrationPlaceholderMode.Enabled ? "Card played with no effect." : location.Type + " location effect resolved.", IntegrationPlaceholderMode.Enabled ? "卡牌已打出，不产生效果。" : GetLocationChinese(location.Type) + "地点效果已结算。");
        RefreshInterface();
        yield return new WaitForSeconds(0.34f);
        yield return MoveRectRoutine(view.RectTransform, GetCanvasPosition(mDiscardPileButton.GetComponent<RectTransform>()), 0.18f, 26f);
        mDiscardPile.Add(card);
        Destroy(view.gameObject);
        mPhase = GamePhase.PlayerAction;
        SetStatus("Card discarded. Continue playing or end the round.", "卡牌已进入弃牌堆。继续行动或结束回合。");
        RefreshInterface();
    }

    // 复用 Round Demo 的资源变化思路结算三个地点。
    private void ApplyLocationEffect(LocationType locationType)
    {
        if (IntegrationPlaceholderMode.Enabled)
        {
            return;
        }

        if (locationType == LocationType.Economy)
        {
            mGold++;
            mPublicOpinion = Mathf.Clamp(mPublicOpinion + 1, 0, 10);
        }
        else if (locationType == LocationType.Military)
        {
            mMilitaryStrength = Mathf.Clamp(mMilitaryStrength + 2, 0, 10);
        }
        else if (locationType == LocationType.Administration)
        {
            mAuthorityLevel = Mathf.Clamp(mAuthorityLevel + 2, 0, 10);
        }
    }

    private void CancelPendingPlay()
    {
        if (mPhase != GamePhase.ChoosingLocation)
        {
            return;
        }

        ReturnSelectedCardToHand("Card play cancelled.", "已取消打牌。");
    }

    // 取消当前卡牌操作并恢复玩家继续选择手牌的状态。
    private void ReturnSelectedCardToHand(string englishStatus, string chineseStatus)
    {
        ClearSelection();
        mPhase = GamePhase.PlayerAction;
        foreach (LocationView location in mLocations)
        {
            location.SetHighlighted(false);
        }
        SetStatus(englishStatus, chineseStatus);
        RefreshInterface();
    }

    private void EndRound()
    {
        if (mPhase == GamePhase.PlayerAction && mOverlay == null && mSettingsOverlay == null)
        {
            StartCoroutine(EndRoundRoutine());
        }
    }

    // 逐张展示剩余手牌，触发保留效果，并依次送入弃牌堆。
    private IEnumerator EndRoundRoutine()
    {
        mPhase = GamePhase.Animating;
        ClearSelection();
        SetStatus("Ending round: revealing retained cards...", "回合结束：正在逐张展示剩余手牌……");
        RefreshInterface();

        while (mHand.Count > 0)
        {
            CardModel card = mHand[0];
            CardView view = mHandViews[card];
            mHand.RemoveAt(0);
            mHandViews.Remove(card);
            LayoutHand();
            PrepareCardForCanvasAnimation(view);
            yield return MoveRectRoutine(view.RectTransform, new Vector2(0f, 70f), 0.2f, 34f);
            ApplyRetainEffect(card);
            RefreshInterface();
            yield return new WaitForSeconds(0.48f);
            yield return MoveRectRoutine(view.RectTransform, GetCanvasPosition(mDiscardPileButton.GetComponent<RectTransform>()), 0.18f, 24f);
            mDiscardPile.Add(card);
            Destroy(view.gameObject);
            RefreshInterface();
            yield return new WaitForSeconds(0.06f);
        }

        SetStatus("Round complete.", "本回合结算完成。");
        yield return new WaitForSeconds(0.38f);
        mRoundNumber++;
        yield return StartRoundRoutine();
    }

    private void ApplyRetainEffect(CardModel card)
    {
        if (IntegrationPlaceholderMode.Enabled)
        {
            SetStatus("Retained card revealed with no effect.", "保留卡牌已展示，不产生效果。");
            return;
        }

        if (card.RetainEffect == RetainEffectType.PublicOpinionUp)
        {
            mPublicOpinion = Mathf.Clamp(mPublicOpinion + 1, 0, 10);
            SetStatus(card.NameEnglish + " retained: PO +1.", card.NameChinese + "保留效果：民意 +1。");
        }
        else if (card.RetainEffect == RetainEffectType.MilitaryStrengthDown)
        {
            mMilitaryStrength = Mathf.Clamp(mMilitaryStrength - 1, 0, 10);
            SetStatus(card.NameEnglish + " retained: MS -1.", card.NameChinese + "保留效果：军力 -1。");
        }
        else
        {
            SetStatus(card.NameEnglish + " has no retain effect.", card.NameChinese + "没有保留效果。");
        }
    }

    private void OpenAllCardsView()
    {
        if (CanOpenOverlay())
        {
            OpenOverlay("All Cards", "总牌库", mOwnedCards.OrderBy(card => card.InstanceId).ToList(), OverlayMode.View);
        }
    }

    private void OpenDrawPileView()
    {
        if (CanOpenOverlay())
        {
            List<CardModel> ordered = new List<CardModel>(mDrawPile);
            ordered.Reverse();
            OpenOverlay("Draw Pile", "抽牌堆", ordered, OverlayMode.View);
        }
    }

    private void OpenDiscardPileView()
    {
        if (CanOpenOverlay())
        {
            List<CardModel> ordered = new List<CardModel>(mDiscardPile);
            ordered.Reverse();
            OpenOverlay("Discard Pile", "弃牌堆", ordered, OverlayMode.View);
        }
    }

    private void OpenDeleteView()
    {
        if (mPhase == GamePhase.PlayerAction && mOverlay == null && mSettingsOverlay == null)
        {
            OpenOverlay("Delete a Card", "删除卡牌", mOwnedCards.OrderBy(card => card.InstanceId).ToList(), OverlayMode.Delete);
        }
    }

    private void OpenMarket()
    {
        if (mPhase == GamePhase.PlayerAction && mOverlay == null && mSettingsOverlay == null)
        {
            OpenOverlay("Royal Decrees", "皇家牌", new List<CardModel>(mMarketCards), OverlayMode.Market);
        }
    }

    private bool CanOpenOverlay()
    {
        return mOverlay == null && mSettingsOverlay == null && (mPhase == GamePhase.PlayerAction || mPhase == GamePhase.ChoosingLocation);
    }

    // 创建可用鼠标滚轮上下滚动的卡牌查看、删牌或购买界面。
    private void OpenOverlay(string titleEnglish, string titleChinese, List<CardModel> cards, OverlayMode mode)
    {
        mOverlayCards = cards;
        mOverlayMode = mode;
        mOverlayTitleEnglish = titleEnglish;
        mOverlayTitleChinese = titleChinese;
        mOverlaySelectedCard = null;
        BuildOverlay();
    }

    private void BuildOverlay()
    {
        if (mOverlay != null)
        {
            Destroy(mOverlay);
        }

        mOverlay = new GameObject("Card Overlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        mOverlay.transform.SetParent(mCanvasRect, false);
        RectTransform overlayRect = mOverlay.GetComponent<RectTransform>();
        StretchToParent(overlayRect);
        mOverlay.GetComponent<Image>().color = new Color(0.03f, 0.03f, 0.025f, 0.88f);

        Image panel = CreatePanel("Panel", mOverlay.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(980f, 620f), new Color(0.88f, 0.84f, 0.72f, 1f));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        CreateText("Overlay Title", panel.transform, mUseChinese ? mOverlayTitleChinese : mOverlayTitleEnglish, 26, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -38f), new Vector2(600f, 44f), new Color(0.12f, 0.11f, 0.09f));
        Button closeButton = CreateButton("Close", panel.transform, mUseChinese ? "关闭" : "Close", new Vector2(1f, 1f), new Vector2(-20f, -38f), new Vector2(100f, 38f), new Color(0.32f, 0.3f, 0.27f));
        closeButton.GetComponent<RectTransform>().pivot = new Vector2(1f, 0.5f);
        closeButton.onClick.AddListener(CloseOverlay);

        GameObject scrollObject = new GameObject("Scroll Area", typeof(RectTransform), typeof(ScrollRect));
        scrollObject.transform.SetParent(panel.transform, false);
        RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        scrollRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        scrollRectTransform.pivot = new Vector2(0.5f, 0.5f);
        scrollRectTransform.anchoredPosition = new Vector2(0f, -8f);
        scrollRectTransform.sizeDelta = new Vector2(900f, 458f);

        GameObject viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask));
        viewportObject.transform.SetParent(scrollObject.transform, false);
        RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
        StretchToParent(viewportRect);
        viewportObject.GetComponent<Image>().color = new Color(0.13f, 0.12f, 0.1f, 0.1f);
        viewportObject.GetComponent<Mask>().showMaskGraphic = false;

        GameObject contentObject = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup));
        contentObject.transform.SetParent(viewportObject.transform, false);
        RectTransform contentRect = contentObject.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        int rowCount = Mathf.Max(1, Mathf.CeilToInt(mOverlayCards.Count / 5f));
        contentRect.sizeDelta = new Vector2(0f, rowCount * 202f + 20f);
        GridLayoutGroup grid = contentObject.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(140f, 182f);
        grid.spacing = new Vector2(26f, 20f);
        grid.padding = new RectOffset(38, 20, 12, 12);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;
        grid.childAlignment = TextAnchor.UpperCenter;

        ScrollRect scrollRect = scrollObject.GetComponent<ScrollRect>();
        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 38f;

        foreach (CardModel card in mOverlayCards)
        {
            CardModel listedCard = card;
            Button cardButton = CreateOverlayCard(contentObject.transform, listedCard, listedCard == mOverlaySelectedCard);
            if (mOverlayMode != OverlayMode.View)
            {
                cardButton.onClick.AddListener(() => SelectOverlayCard(listedCard));
            }
            else
            {
                cardButton.interactable = false;
            }
        }

        if (mOverlayMode == OverlayMode.Delete || mOverlayMode == OverlayMode.Market)
        {
            string actionText = mOverlayMode == OverlayMode.Delete ? (mUseChinese ? "删除" : "Delete") : (mUseChinese ? "购买" : "Buy");
            Button actionButton = CreateButton("Confirm", panel.transform, actionText, new Vector2(0.5f, 0f), new Vector2(0f, 32f), new Vector2(150f, 42f), mOverlayMode == OverlayMode.Delete ? new Color(0.48f, 0.2f, 0.18f) : new Color(0.58f, 0.45f, 0.14f));
            actionButton.interactable = mOverlaySelectedCard != null;
            actionButton.onClick.AddListener(ConfirmOverlayAction);
        }

        panelRect.SetAsLastSibling();
    }

    private Button CreateOverlayCard(Transform parent, CardModel card, bool selected)
    {
        Color border = selected ? new Color(0.77f, 0.18f, 0.14f) : card.IsRoyal ? new Color(0.86f, 0.64f, 0.12f) : new Color(0.1f, 0.1f, 0.09f);
        Button button = CreateButton("Overlay Card " + card.InstanceId, parent, "", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(140f, 182f), border);
        Image face = CreatePanel("Face", button.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, card.IsRoyal ? new Color(1f, 0.95f, 0.7f) : new Color(0.96f, 0.95f, 0.9f));
        RectTransform faceRect = face.GetComponent<RectTransform>();
        faceRect.offsetMin = new Vector2(5f, 5f);
        faceRect.offsetMax = new Vector2(-5f, -5f);
        face.raycastTarget = false;
        CreateText("Title", button.transform, mUseChinese ? card.NameChinese : card.NameEnglish, 16, FontStyle.Bold, TextAnchor.UpperCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(122f, 48f), new Color(0.12f, 0.11f, 0.09f));
        CreateText("Description", button.transform, mUseChinese ? card.DescriptionChinese : card.DescriptionEnglish, 12, FontStyle.Normal, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -2f), new Vector2(120f, 76f), new Color(0.12f, 0.11f, 0.09f));
        CreateText("Type", button.transform, GetCardTypeText(card), 11, FontStyle.Bold, TextAnchor.LowerCenter, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 18f), new Vector2(122f, 28f), new Color(0.12f, 0.11f, 0.09f));
        return button;
    }

    private void SelectOverlayCard(CardModel card)
    {
        mOverlaySelectedCard = card;
        BuildOverlay();
    }

    private void ConfirmOverlayAction()
    {
        if (mOverlaySelectedCard == null)
        {
            return;
        }

        if (mOverlayMode == OverlayMode.Delete)
        {
            DeleteOwnedCard(mOverlaySelectedCard);
            SetStatus("Card deleted from the total deck.", "已从总牌库中删除卡牌。");
        }
        else if (mOverlayMode == OverlayMode.Market)
        {
            CardModel purchased = CreateCard(mOverlaySelectedCard.NameEnglish, mOverlaySelectedCard.NameChinese, mOverlaySelectedCard.DescriptionEnglish, mOverlaySelectedCard.DescriptionChinese, mOverlaySelectedCard.Location, mOverlaySelectedCard.RetainEffect, true);
            mOwnedCards.Add(purchased);
            mDiscardPile.Add(purchased);
            mMarketCards.Remove(mOverlaySelectedCard);
            SetStatus("Royal card purchased and added to the discard pile.", "皇家牌已购买并加入弃牌堆。");
        }

        CloseOverlay();
        RefreshInterface();
    }

    // 从总牌库以及该牌当前所在区域中删除同一个卡牌实例。
    private void DeleteOwnedCard(CardModel card)
    {
        mOwnedCards.Remove(card);
        mDrawPile.Remove(card);
        mDiscardPile.Remove(card);
        if (mHand.Remove(card) && mHandViews.TryGetValue(card, out CardView view))
        {
            if (mSelectedCardView == view)
            {
                mSelectedCardView = null;
            }
            mHandViews.Remove(card);
            Destroy(view.gameObject);
            LayoutHand();
        }
    }

    private void CloseOverlay()
    {
        if (mOverlay != null)
        {
            Destroy(mOverlay);
        }
        mOverlay = null;
        mOverlayCards = null;
        mOverlaySelectedCard = null;
    }

    // 将手牌排成中心较高、两侧平滑下落的扇形，最多容纳十张。
    private void LayoutHand()
    {
        int count = mHand.Count;
        float spacing = count <= 1 ? 0f : Mathf.Min(100f, 760f / (count - 1));
        float totalWidth = Mathf.Max(0f, count - 1) * spacing;
        for (int i = 0; i < count; i++)
        {
            float centerOffset = i - (count - 1) * 0.5f;
            float normalizedOffset = count <= 1 ? 0f : centerOffset / ((count - 1) * 0.5f);
            Vector2 position = new Vector2(-totalWidth * 0.5f + i * spacing, -210f - normalizedOffset * normalizedOffset * 46f);
            float angle = -normalizedOffset * 15f;
            CardView view = mHandViews[mHand[i]];
            view.transform.SetSiblingIndex(i);
            view.SetLayout(position, angle);
        }

        if (mSelectedCardView != null)
        {
            mSelectedCardView.transform.SetAsLastSibling();
        }
    }

    private void PrepareCardForCanvasAnimation(CardView view)
    {
        Vector3 worldPosition = view.RectTransform.position;
        view.PrepareForAnimation();
        view.SetInteractable(false);
        view.RectTransform.SetParent(mCanvasRect, true);
        view.RectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        view.RectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        view.RectTransform.pivot = new Vector2(0.5f, 0.5f);
        view.RectTransform.anchoredPosition = WorldToCanvasPosition(worldPosition);
        view.transform.SetAsLastSibling();
    }

    private IEnumerator MoveRectRoutine(RectTransform rect, Vector2 target, float duration, float arcHeight)
    {
        Vector2 start = rect.anchoredPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float smooth = Mathf.SmoothStep(0f, 1f, progress);
            Vector2 position = Vector2.Lerp(start, target, smooth);
            position.y += Mathf.Sin(progress * Mathf.PI) * arcHeight;
            rect.anchoredPosition = position;
            yield return null;
        }
        rect.anchoredPosition = target;
    }

    private void ClearSelection()
    {
        if (mSelectedCardView != null)
        {
            mSelectedCardView.StopFollowingPointer();
            mSelectedCardView.SetSelected(false);
        }
        mSelectedCardView = null;
        mPendingCard = null;
    }

    // 打开设置页；语言切换是当前版本的第一个正式设置项。
    private void OpenSettings()
    {
        if (mSettingsOverlay != null || mOverlay != null || mPhase == GamePhase.Animating)
        {
            return;
        }

        if (mPhase == GamePhase.ChoosingLocation)
        {
            CancelPendingPlay();
        }
        else
        {
            ClearSelection();
        }

        mSettingsOverlay = new GameObject("Settings Overlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        mSettingsOverlay.transform.SetParent(mCanvasRect, false);
        RectTransform overlayRect = mSettingsOverlay.GetComponent<RectTransform>();
        StretchToParent(overlayRect);
        mSettingsOverlay.GetComponent<Image>().color = new Color(0.03f, 0.03f, 0.025f, 0.88f);

        Image panel = CreatePanel("Settings Panel", mSettingsOverlay.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500f, 320f), new Color(0.88f, 0.84f, 0.72f, 1f));
        CreateText("Settings Title", panel.transform, mUseChinese ? "设置" : "Settings", 28, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -52f), new Vector2(300f, 50f), new Color(0.12f, 0.11f, 0.09f));
        CreateText("Language", panel.transform, mUseChinese ? "语言" : "Language", 20, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 46f), new Vector2(260f, 40f), new Color(0.12f, 0.11f, 0.09f));
        Button englishButton = CreateButton("English", panel.transform, "English", new Vector2(0.5f, 0.5f), new Vector2(-92f, -10f), new Vector2(160f, 46f), !mUseChinese ? new Color(0.18f, 0.43f, 0.31f) : new Color(0.34f, 0.33f, 0.3f));
        englishButton.onClick.AddListener(() => SetLanguage(false));
        Button chineseButton = CreateButton("Chinese", panel.transform, "中文", new Vector2(0.5f, 0.5f), new Vector2(92f, -10f), new Vector2(160f, 46f), mUseChinese ? new Color(0.18f, 0.43f, 0.31f) : new Color(0.34f, 0.33f, 0.3f));
        chineseButton.onClick.AddListener(() => SetLanguage(true));
        Button closeButton = CreateButton("Close Settings", panel.transform, mUseChinese ? "关闭" : "Close", new Vector2(0.5f, 0f), new Vector2(0f, 38f), new Vector2(140f, 42f), new Color(0.32f, 0.3f, 0.27f));
        closeButton.onClick.AddListener(CloseSettings);
        mSettingsOverlay.transform.SetAsLastSibling();
        RefreshInterface();
    }

    private void SetLanguage(bool useChinese)
    {
        mUseChinese = useChinese;
        CloseSettings();
        RefreshLocalizedInterface();
        RefreshInterface();
    }

    private void CloseSettings()
    {
        if (mSettingsOverlay != null)
        {
            Destroy(mSettingsOverlay);
        }
        mSettingsOverlay = null;
        RefreshInterface();
    }

    // 立即刷新主界面、地点和所有当前手牌的语言。
    private void RefreshLocalizedInterface()
    {
        SetButtonLabel(mAllCardsButton, mUseChinese ? "总牌库" : "All Cards");
        SetButtonLabel(mBuyButton, mUseChinese ? "买牌" : "Buy");
        SetButtonLabel(mDeleteButton, mUseChinese ? "删牌" : "Delete");
        SetButtonLabel(mEndRoundButton, mUseChinese ? "结束回合" : "End Round");
        SetButtonLabel(mCancelPlayButton, mUseChinese ? "取消" : "Cancel");
        SetButtonLabel(mStartRoundButton, mUseChinese ? "开始回合" : "Start Round");
        SetButtonLabel(mDrawPileButton, mUseChinese ? "抽牌" : "DECK");
        SetButtonLabel(mDiscardPileButton, mUseChinese ? "弃牌" : "DISCARD");
        mDrawPileNameText.text = mUseChinese ? "抽牌堆" : "DRAW PILE";
        mDiscardPileNameText.text = mUseChinese ? "弃牌堆" : "DISCARD PILE";
        mInHandText.text = mUseChinese ? "手牌" : "IN HAND";
        mStatusText.text = mUseChinese ? mStatusChinese : mStatusEnglish;

        foreach (LocationView location in mLocations)
        {
            location.SetTexts(mUseChinese);
        }
        foreach (CardView cardView in mHandViews.Values)
        {
            cardView.SetTexts(mUseChinese);
        }
    }

    private void RefreshInterface()
    {
        mStatsText.text = mUseChinese ? "金币 " + mGold + "    民意 " + mPublicOpinion + "    军力 " + mMilitaryStrength + "    权威 " + mAuthorityLevel : "Gold " + mGold + "    PO " + mPublicOpinion + "    MS " + mMilitaryStrength + "    AL " + mAuthorityLevel;
        mRoundText.text = mUseChinese ? "第 " + mRoundNumber + " 回合    大臣 " + mMinistersLeft + "/" + kMaximumMinisters : "ROUND " + mRoundNumber + "    MINISTERS " + mMinistersLeft + "/" + kMaximumMinisters;
        mDrawCountText.text = mDrawPile.Count.ToString();
        mDiscardCountText.text = mDiscardPile.Count.ToString();
        bool playerAction = mPhase == GamePhase.PlayerAction && mOverlay == null && mSettingsOverlay == null;
        bool canView = mOverlay == null && mSettingsOverlay == null && (mPhase == GamePhase.PlayerAction || mPhase == GamePhase.ChoosingLocation);
        mBuyButton.interactable = playerAction && mMarketCards.Count > 0;
        mDeleteButton.interactable = playerAction && mOwnedCards.Count > 0;
        mEndRoundButton.interactable = playerAction;
        mCancelPlayButton.gameObject.SetActive(mPhase == GamePhase.ChoosingLocation);
        mAllCardsButton.interactable = canView;
        mDrawPileButton.interactable = canView;
        mDiscardPileButton.interactable = canView;
        mSettingsButton.interactable = mOverlay == null && mSettingsOverlay == null && mPhase != GamePhase.Animating;
    }

    private void SetStatus(string english, string chinese)
    {
        mStatusEnglish = english;
        mStatusChinese = chinese;
        mStatusText.text = mUseChinese ? chinese : english;
    }

    private string GetCardTypeText(CardModel card)
    {
        if (IntegrationPlaceholderMode.Enabled)
        {
            return "";
        }

        if (card.IsRoyal)
        {
            return mUseChinese ? "皇家牌" : "ROYAL";
        }
        return mUseChinese ? GetLocationChinese(card.Location) : card.Location.ToString().ToUpperInvariant();
    }

    private string GetLocationChinese(LocationType locationType)
    {
        if (locationType == LocationType.Economy)
        {
            return "经济";
        }
        if (locationType == LocationType.Military)
        {
            return "军事";
        }
        if (locationType == LocationType.Administration)
        {
            return "行政";
        }
        return "任意";
    }

    private void Shuffle(List<CardModel> cards)
    {
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            CardModel temp = cards[i];
            cards[i] = cards[swapIndex];
            cards[swapIndex] = temp;
        }
    }

    private Vector2 GetCanvasPosition(RectTransform target)
    {
        return WorldToCanvasPosition(target.position);
    }

    private Vector2 WorldToCanvasPosition(Vector3 worldPosition)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, worldPosition);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(mCanvasRect, screenPoint, null, out Vector2 localPoint);
        return localPoint;
    }

    private RectTransform CreateLayer(string name, Transform parent)
    {
        GameObject layerObject = new GameObject(name, typeof(RectTransform));
        layerObject.transform.SetParent(parent, false);
        RectTransform rect = layerObject.GetComponent<RectTransform>();
        StretchToParent(rect);
        return rect;
    }

    private Image CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size, Color color)
    {
        GameObject panelObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelObject.transform.SetParent(parent, false);
        RectTransform rect = panelObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Image image = panelObject.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private Text CreateText(string name, Transform parent, string textValue, int fontSize, FontStyle style, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size, Color color)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Text text = textObject.GetComponent<Text>();
        text.font = mFont;
        text.text = textValue;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;
        return text;
    }

    private Button CreateButton(string name, Transform parent, string label, Vector2 anchor, Vector2 position, Vector2 size, Color color)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Image image = buttonObject.GetComponent<Image>();
        image.color = color;
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
        colors.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
        colors.disabledColor = new Color(0.38f, 0.38f, 0.38f, 0.7f);
        button.colors = colors;
        CreateText("Label", buttonObject.transform, label, 16, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.white);
        return button;
    }

    private void SetButtonLabel(Button button, string label)
    {
        Transform labelTransform = button.transform.Find("Label");
        if (labelTransform != null)
        {
            labelTransform.GetComponent<Text>().text = label;
        }
    }

    private Button CreatePileButton(string name, Transform parent, string label, Vector2 position, out Text countText, out Text nameText)
    {
        Button button = CreateButton(name, parent, label, new Vector2(0.5f, 0.5f), position, new Vector2(92f, 128f), new Color(0.28f, 0.29f, 0.28f));
        nameText = CreateText("Pile Name", parent, name.ToUpperInvariant(), 14, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position + new Vector2(0f, 84f), new Vector2(170f, 28f), Color.white);
        countText = CreateText("Count", parent, "0", 18, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position + new Vector2(0f, -82f), new Vector2(92f, 28f), Color.white);
        return button;
    }

    private static void StretchToParent(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
