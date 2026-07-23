using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private ZebraGameController mController;
    private Image mHitArea;
    private Image mBorder;
    private Image mFace;
    private Text mTitle;
    private Text mDescription;
    private Text mLocation;
    private Vector2 mLayoutPosition;
    private float mLayoutAngle;
    private bool mSelected;
    private bool mHovered;
    private bool mFollowingPointer;
    private int mLayoutSiblingIndex;
    private RectTransform mVisualTransform;

    public CardModel Card { get; private set; }
    public RectTransform RectTransform { get; private set; }
    // The visual child (correctly sized 130x182); used by TutorialDirector to highlight this card.
    public RectTransform VisualTransform => mVisualTransform != null ? mVisualTransform : RectTransform;
    public bool IsFollowingPointer => mFollowingPointer;

    // 创建一张运行时 UI 卡牌并绑定卡牌数据。
    public static CardView Create(Transform parent, Font font, CardModel card, ZebraGameController controller)
    {
        GameObject cardObject = new GameObject("Card " + card.InstanceId, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CardView));
        cardObject.transform.SetParent(parent, false);
        CardView view = cardObject.GetComponent<CardView>();
        view.RectTransform = cardObject.GetComponent<RectTransform>();
        view.RectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        view.RectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        view.RectTransform.pivot = new Vector2(0.5f, 0.5f);
        view.RectTransform.sizeDelta = new Vector2(130f, 182f);
        Image rootImage = cardObject.GetComponent<Image>();
        rootImage.color = new Color(0f, 0f, 0f, 0f);
        rootImage.raycastTarget = false;   // clicks are received by the tracking hit area created below

        GameObject visualObject = new GameObject("Visual", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        visualObject.transform.SetParent(cardObject.transform, false);
        view.mVisualTransform = visualObject.GetComponent<RectTransform>();
        view.mVisualTransform.anchorMin = new Vector2(0.5f, 0.5f);
        view.mVisualTransform.anchorMax = new Vector2(0.5f, 0.5f);
        view.mVisualTransform.pivot = new Vector2(0.5f, 0.5f);
        view.mVisualTransform.anchoredPosition = Vector2.zero;
        view.mVisualTransform.sizeDelta = new Vector2(130f, 182f);
        view.mBorder = visualObject.GetComponent<Image>();
        view.mBorder.sprite = GameUITheme.GetCardFrameSprite();
        view.mBorder.type = Image.Type.Simple;
        view.mBorder.preserveAspect = false;
        view.mBorder.color = Color.white;
        view.mBorder.raycastTarget = false;

        // GoldCardFrame already contains the complete white face and gold border,
        // so no separate face image is overlaid (it would cover the decoration).
        view.mFace = view.mBorder;

        // Clickable hit area that follows the visible card (a child of the visual) and grows
        // taller when the card is raised, so the whole visible card is clickable — not just the
        // small on-screen sliver of the resting card.
        GameObject hitAreaObject = new GameObject("Hit Area", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        hitAreaObject.transform.SetParent(visualObject.transform, false);
        RectTransform hitAreaRect = hitAreaObject.GetComponent<RectTransform>();
        hitAreaRect.anchorMin = new Vector2(0.5f, 0.5f);
        hitAreaRect.anchorMax = new Vector2(0.5f, 0.5f);
        hitAreaRect.pivot = new Vector2(0.5f, 0.5f);
        hitAreaRect.anchoredPosition = Vector2.zero;
        hitAreaRect.sizeDelta = new Vector2(130f, 182f);
        view.mHitArea = hitAreaObject.GetComponent<Image>();
        view.mHitArea.color = new Color(0f, 0f, 0f, 0f);

        view.mTitle = CreateText("Title", visualObject.transform, font, 17, FontStyle.Bold, TextAnchor.UpperCenter, new Vector2(8f, 132f), new Vector2(114f, 40f));
        view.mDescription = CreateText("Description", visualObject.transform, font, 13, FontStyle.Normal, TextAnchor.MiddleCenter, new Vector2(8f, 58f), new Vector2(114f, 72f));
        view.mLocation = CreateText("Location", visualObject.transform, font, 12, FontStyle.Bold, TextAnchor.LowerCenter, new Vector2(8f, 30f), new Vector2(114f, 28f));
        view.mController = controller;
        view.Card = card;
        view.SetTexts(controller.UseChinese);
        return view;
    }

    // 更新卡牌标题、说明和地点类型。
    public void SetTexts(bool useChinese)
    {
        if (IntegrationPlaceholderMode.Enabled)
        {
            mTitle.text = "";
            mDescription.text = "";
            mLocation.text = "";
            return;
        }

        mTitle.text = useChinese ? Card.NameChinese : Card.NameEnglish;
        mDescription.text = useChinese ? Card.DescriptionChinese : Card.DescriptionEnglish;
        string locationLabel;
        if (Card.IsRoyal)
        {
            locationLabel = useChinese ? "皇家牌" : "ROYAL";
        }
        else if (useChinese)
        {
            locationLabel = Card.Location == LocationType.Economy ? "经济" : Card.Location == LocationType.Military ? "军事" : Card.Location == LocationType.Administration ? "行政" : Card.Location == LocationType.Diplomacy ? "外交" : "任意";
        }
        else
        {
            locationLabel = Card.Location.ToString().ToUpperInvariant();
        }

        // 在地点类型后附上揭示阶段的收益：威严/战斗力，例如 "Economy +1/+0"。
        mLocation.text = locationLabel + " " + Sign(Card.MajestyGain) + "/" + Sign(Card.FightGain);
    }

    // 带符号显示（+0、+1、-1）。
    private static string Sign(int v) { return (v >= 0 ? "+" : "") + v; }

    // 保存卡牌在扇形手牌中的基础位置和角度。
    public void SetLayout(Vector2 position, float angle)
    {
        mLayoutPosition = position;
        mLayoutAngle = angle;
        mLayoutSiblingIndex = transform.GetSiblingIndex();
        RefreshTransform();
    }

    // 设置选中状态；选中牌上移、放大并置于最上层。
    public void SetSelected(bool selected)
    {
        mSelected = selected;
        if (selected)
        {
            transform.SetAsLastSibling();
        }
        RefreshTransform();
    }

    // 第一次点击后让卡牌持续跟随鼠标，并保持在最上层。
    public void BeginFollowingPointer()
    {
        mFollowingPointer = true;
        mHovered = false;
        transform.SetAsLastSibling();
    }

    // 停止跟随鼠标，并按当前选择状态恢复位置。
    public void StopFollowingPointer()
    {
        mFollowingPointer = false;
        RefreshTransform();
    }

    // 动画期间关闭卡牌点击检测。
    public void SetInteractable(bool interactable)
    {
        mHitArea.raycastTarget = interactable;
    }

    // 将卡牌恢复为可由动画直接控制的状态。
    public void PrepareForAnimation()
    {
        mSelected = false;
        mHovered = false;
        mFollowingPointer = false;
        RectTransform.localScale = Vector3.one;
        RectTransform.localRotation = Quaternion.identity;
        mVisualTransform.anchoredPosition = Vector2.zero;
        mVisualTransform.localScale = Vector3.one;
        mVisualTransform.localRotation = Quaternion.identity;
        SetHitAreaHeight(182f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        mController.OnHandCardClicked(this, eventData.button);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!mFollowingPointer && mController.CanHoverCard(this))
        {
            mHovered = true;
            transform.SetAsLastSibling();
            RefreshTransform();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!mFollowingPointer)
        {
            mHovered = false;
            if (!mSelected)
            {
                transform.SetSiblingIndex(Mathf.Min(mLayoutSiblingIndex, transform.parent.childCount - 1));
            }
            RefreshTransform();
        }
    }

    private void Update()
    {
        if (!mFollowingPointer)
        {
            return;
        }

        RectTransform parentRect = RectTransform.parent as RectTransform;
        if (parentRect != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, Input.mousePosition, null, out Vector2 localPoint))
        {
            RectTransform.anchoredPosition = localPoint + new Vector2(0f, 24f);
            RectTransform.localRotation = Quaternion.identity;
            RectTransform.localScale = Vector3.one;
            mVisualTransform.anchoredPosition = Vector2.zero;
            mVisualTransform.localRotation = Quaternion.identity;
            mVisualTransform.localScale = Vector3.one * 1.14f;
            SetHitAreaHeight(260f);
            transform.SetAsLastSibling();
        }
    }

    private void RefreshTransform()
    {
        if (mFollowingPointer)
        {
            return;
        }

        bool raised = mSelected || mHovered;
        // The root remains at its resting fan position and receives pointer events. Only the
        // visual child lifts, enlarges and straightens, so edge cards keep a stable hit area.
        float scale = mSelected ? 1.1f : mHovered ? 1.07f : 1f;
        float visualRaiseAmount = mSelected ? 65f : mHovered ? 20f : 0f;
        RectTransform.anchoredPosition = mLayoutPosition;
        RectTransform.localRotation = Quaternion.Euler(0f, 0f, mLayoutAngle);
        RectTransform.localScale = Vector3.one;
        mVisualTransform.anchoredPosition = GetVisibleVisualOffset(visualRaiseAmount, scale);
        mVisualTransform.localRotation = Quaternion.Euler(0f, 0f, raised ? -mLayoutAngle : 0f);
        mVisualTransform.localScale = Vector3.one * scale;
        // A raised card lifts away from the cursor; a taller hit area bridges back down to it,
        // so the whole visible card stays clickable and hover does not flicker.
        SetHitAreaHeight(mSelected ? 290f : mHovered ? 210f : 182f);
    }

    private void SetHitAreaHeight(float height)
    {
        if (mHitArea == null)
        {
            return;
        }
        RectTransform hitRect = mHitArea.rectTransform;
        hitRect.sizeDelta = new Vector2(130f, height);
        hitRect.anchoredPosition = Vector2.zero;
    }

    private Vector2 GetVisibleVisualOffset(float requestedRaise, float scale)
    {
        RectTransform handRect = RectTransform.parent as RectTransform;
        if (handRect == null || (!mHovered && !mSelected))
        {
            return new Vector2(0f, requestedRaise);
        }

        // The visual card is upright while hovered. Clamp its centre within the hand canvas
        // rather than relying on a fixed lift, so side cards and larger hands stay readable.
        const float margin = 12f;
        float halfWidth = mVisualTransform.sizeDelta.x * scale * 0.5f;
        float halfHeight = mVisualTransform.sizeDelta.y * scale * 0.5f;
        Rect bounds = handRect.rect;
        float targetX = Mathf.Clamp(mLayoutPosition.x, bounds.xMin + halfWidth + margin, bounds.xMax - halfWidth - margin);
        float targetY = Mathf.Clamp(mLayoutPosition.y + requestedRaise, bounds.yMin + halfHeight + margin, bounds.yMax - halfHeight - margin);
        return new Vector2(targetX - mLayoutPosition.x, targetY - mLayoutPosition.y);
    }

    private static Text CreateText(string name, Transform parent, Font font, int fontSize, FontStyle style, TextAnchor alignment, Vector2 position, Vector2 size)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = Vector2.zero;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Text text = textObject.GetComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = new Color(0.12f, 0.11f, 0.09f);
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;
        return text;
    }
}
