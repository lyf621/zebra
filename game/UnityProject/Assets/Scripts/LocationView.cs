using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LocationView : MonoBehaviour, IPointerClickHandler
{
    private ZebraGameController mController;
    private Image mBorder;
    private Image mFace;
    private Text mTitle;
    private Text mEffect;
    private Text mMinisterMarker;
    private Color mBaseColor;

    public LocationType Type { get; private set; }
    public bool IsOccupied { get; private set; }
    public RectTransform RectTransform { get; private set; }

    // 创建一个可放置大臣的地点视图。
    public static LocationView Create(Transform parent, Font font, ZebraGameController controller, LocationType type, string title, string effect, Vector2 position, Color color)
    {
        GameObject locationObject = new GameObject(title, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LocationView));
        locationObject.transform.SetParent(parent, false);
        LocationView view = locationObject.GetComponent<LocationView>();
        view.RectTransform = locationObject.GetComponent<RectTransform>();
        view.RectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        view.RectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        view.RectTransform.pivot = new Vector2(0.5f, 0.5f);
        view.RectTransform.anchoredPosition = position;
        view.RectTransform.sizeDelta = new Vector2(220f, 142f);
        view.mBorder = locationObject.GetComponent<Image>();
        view.mBorder.color = new Color(0.1f, 0.1f, 0.09f);
        view.mBaseColor = color;

        GameObject faceObject = new GameObject("Face", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        faceObject.transform.SetParent(locationObject.transform, false);
        RectTransform faceRect = faceObject.GetComponent<RectTransform>();
        faceRect.anchorMin = Vector2.zero;
        faceRect.anchorMax = Vector2.one;
        faceRect.offsetMin = new Vector2(4f, 4f);
        faceRect.offsetMax = new Vector2(-4f, -4f);
        view.mFace = faceObject.GetComponent<Image>();
        view.mFace.color = color;
        view.mFace.raycastTarget = false;

        view.mTitle = CreateText("Title", locationObject.transform, font, 22, FontStyle.Bold, TextAnchor.UpperCenter, new Vector2(12f, 86f), new Vector2(196f, 42f));
        view.mEffect = CreateText("Effect", locationObject.transform, font, 16, FontStyle.Normal, TextAnchor.MiddleCenter, new Vector2(12f, 30f), new Vector2(196f, 58f));
        view.mMinisterMarker = CreateText("Minister", locationObject.transform, font, 13, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(58f, 6f), new Vector2(104f, 26f));
        view.mMinisterMarker.color = new Color(0.98f, 0.85f, 0.32f);
        view.mMinisterMarker.text = "";
        view.mController = controller;
        view.Type = type;
        view.mTitle.text = title;
        view.mEffect.text = effect;
        return view;
    }

    // 高亮当前卡牌允许选择的地点。
    public void SetHighlighted(bool highlighted)
    {
        mBorder.color = highlighted ? new Color(0.95f, 0.72f, 0.18f) : new Color(0.1f, 0.1f, 0.09f);
        mFace.color = IsOccupied ? new Color(0.34f, 0.33f, 0.31f) : mBaseColor;
    }

    // 放置或回收大臣，并更新地点外观。
    public void SetOccupied(bool occupied)
    {
        IsOccupied = occupied;
        mMinisterMarker.text = occupied ? "MINISTER" : "";
        mFace.color = occupied ? new Color(0.34f, 0.33f, 0.31f) : mBaseColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        mController.OnLocationClicked(this);
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
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }
}
