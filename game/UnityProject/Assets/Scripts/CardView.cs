using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardView : MonoBehaviour, IPointerClickHandler
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

    public CardModel Card { get; private set; }
    public RectTransform RectTransform { get; private set; }

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

        view.mTitle = CreateText("Title", cardObject.transform, font, 17, FontStyle.Bold, TextAnchor.UpperCenter, new Vector2(8f, 132f), new Vector2(114f, 40f));
        view.mDescription = CreateText("Description", cardObject.transform, font, 13, FontStyle.Normal, TextAnchor.MiddleCenter, new Vector2(8f, 48f), new Vector2(114f, 82f));
        view.mLocation = CreateText("Location", cardObject.transform, font, 12, FontStyle.Bold, TextAnchor.LowerCenter, new Vector2(8f, 10f), new Vector2(114f, 32f));
        view.mController = controller;
        view.Card = card;
        view.SetTexts(false);
        return view;
    }

    // 更新卡牌标题、说明和地点类型。
    public void SetTexts(bool useChinese)
    {
        mTitle.text = useChinese ? Card.NameChinese : Card.NameEnglish;
        mDescription.text = useChinese ? Card.DescriptionChinese : Card.DescriptionEnglish;
        mLocation.text = Card.IsRoyal ? "ROYAL" : Card.Location.ToString().ToUpperInvariant();
    }

    // 保存卡牌在扇形手牌中的基础位置和角度。
    public void SetLayout(Vector2 position, float angle)
    {
        mLayoutPosition = position;
        mLayoutAngle = angle;
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

    // 动画期间关闭卡牌点击检测。
    public void SetInteractable(bool interactable)
    {
        mBorder.raycastTarget = interactable;
    }

    // 将卡牌恢复为可由动画直接控制的状态。
    public void PrepareForAnimation()
    {
        mSelected = false;
        RectTransform.localScale = Vector3.one;
        RectTransform.localRotation = Quaternion.identity;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        mController.OnHandCardClicked(this);
    }

    private void RefreshTransform()
    {
        RectTransform.anchoredPosition = mLayoutPosition + (mSelected ? new Vector2(0f, 54f) : Vector2.zero);
        RectTransform.localRotation = Quaternion.Euler(0f, 0f, mSelected ? 0f : mLayoutAngle);
        RectTransform.localScale = mSelected ? Vector3.one * 1.08f : Vector3.one;
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
