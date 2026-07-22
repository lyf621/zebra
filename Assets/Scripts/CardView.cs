using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private ZebraGameController mController;
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

    public CardModel Card { get; private set; }
    public RectTransform RectTransform { get; private set; }
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
        view.RectTransform.sizeDelta = new Vector2(100f, 140f);
        view.mBorder = cardObject.GetComponent<Image>();
        view.mBorder.color = card.IsRoyal ? new Color(0.86f, 0.64f, 0.12f) : new Color(0.11f, 0.11f, 0.1f);

        GameObject faceObject = new GameObject("Face", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        faceObject.transform.SetParent(cardObject.transform, false);
        RectTransform faceRect = faceObject.GetComponent<RectTransform>();
        faceRect.anchorMin = Vector2.zero;
        faceRect.anchorMax = Vector2.one;
        faceRect.offsetMin = new Vector2(4f, 4f);
        faceRect.offsetMax = new Vector2(-4f, -4f);
        view.mFace = faceObject.GetComponent<Image>();
        view.mFace.color = card.IsRoyal ? new Color(1f, 0.95f, 0.7f) : new Color(0.96f, 0.95f, 0.9f);
        view.mFace.raycastTarget = false;

        view.mTitle = CreateText("Title", cardObject.transform, font, 14, FontStyle.Bold, TextAnchor.UpperCenter, new Vector2(6f, 100f), new Vector2(88f, 32f));
        view.mDescription = CreateText("Description", cardObject.transform, font, 10, FontStyle.Normal, TextAnchor.MiddleCenter, new Vector2(6f, 34f), new Vector2(88f, 64f));
        view.mLocation = CreateText("Location", cardObject.transform, font, 10, FontStyle.Bold, TextAnchor.LowerCenter, new Vector2(6f, 7f), new Vector2(88f, 24f));
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
        mBorder.raycastTarget = interactable;
    }

    // 将卡牌恢复为可由动画直接控制的状态。
    public void PrepareForAnimation()
    {
        mSelected = false;
        mHovered = false;
        mFollowingPointer = false;
        RectTransform.localScale = Vector3.one;
        RectTransform.localRotation = Quaternion.identity;
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
            RectTransform.localScale = Vector3.one * 1.14f;
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
        float raiseAmount = mSelected ? 54f : mHovered ? 34f : 0f;
        float scale = mSelected ? 1.1f : mHovered ? 1.07f : 1f;
        RectTransform.anchoredPosition = mLayoutPosition + new Vector2(0f, raiseAmount);
        RectTransform.localRotation = Quaternion.Euler(0f, 0f, raised ? 0f : mLayoutAngle);
        RectTransform.localScale = Vector3.one * scale;
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
