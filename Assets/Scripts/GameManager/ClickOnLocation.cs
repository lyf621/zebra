using UnityEngine;
using UnityEngine.EventSystems;

public class ClickOnLocation : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("地点类型（决定哪些卡牌可放置）")]
    [SerializeField] private LocationType locationType = LocationType.Any;

    [Header("点击效果（在 Inspector 中绑定，来自 LocationEffects）")]
    [SerializeField] private UnityEngine.Events.UnityEvent onFirstClick;

    [Header("外观反馈（可选）")]
    [SerializeField] private Color clickedColor = Color.gray;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color highlightColor = new Color(0.95f, 0.72f, 0.18f);

    [Header("绑定Stats/Turn/Cards管理")]
    [SerializeField] private StatManager Stats;
    [SerializeField] private TurnController Turns;
    [SerializeField] private ZebraGameController Cards;

    [Header("右键信息浮窗（手动填写；中文留空则回退英文）")]
    [SerializeField] private string infoName;
    [SerializeField] private string infoNameChinese;
    [TextArea][SerializeField] private string infoDescription;
    [TextArea][SerializeField] private string infoDescriptionChinese;

    private Collider2D collider2D;
    private SpriteRenderer spriteRenderer;
    private LineRenderer highlightFrame;
    private bool hasBeenClicked = false;

    private void Start()
    {
        // 获取必要组件
        collider2D = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // The unified map supplies the artwork. Location sprites are now invisible hit areas.
            spriteRenderer.enabled = false;
        }
        CreateHighlightFrame();
        if (Cards == null) Cards = FindAnyObjectByType<ZebraGameController>();

        if (collider2D == null)
            Debug.LogError("物体缺少 Collider2D，无法接收点击！");
    }

    // IPointerClickHandler 接口实现
    public void OnPointerClick(PointerEventData eventData)
    {
        // 右键任意时刻显示地点信息浮窗（即使该地点已被占用）。
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            ShowInfoPopup(eventData.position);
            return;
        }

        if (hasBeenClicked) return; // 已经触发过，忽略

        // 卡牌驱动：只有当玩家正在打出一张匹配的卡牌时，这个地点才会响应。
        // TryPlayCardOnLocation 会校验待出牌、地点类型并消耗手牌。
        if (Cards != null)
        {
            if (!Cards.TryPlayCardOnLocation(this))
                return;
        }

        // 校验通过（或没有卡牌系统时的独立模式）：占用地点并触发地点效果。
        DisableObject();
        onFirstClick?.Invoke();   // 效果来自 LocationEffects 中的脚本
        if (Cards != null) Cards.ApplyPermanentLocationBonus(this);   // 叠加该地点已实施的永久政策
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Cards == null) Cards = FindAnyObjectByType<ZebraGameController>();
        if (Cards != null) Cards.PreviewLocationEffect(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Cards == null) Cards = FindAnyObjectByType<ZebraGameController>();
        if (Cards != null) Cards.ClearLocationEffectPreview(this);
    }

    /// 重置物体，使其可再次被点击。
    public void ResetObject()
    {
        // 重置点击标记
        hasBeenClicked = false;

        // 恢复碰撞器
        if (collider2D != null)
            collider2D.enabled = true;

        // 恢复外观（如果有 SpriteRenderer）
        SetFrameVisible(false, defaultColor);
    }

    public void DisableObject()
    {
        hasBeenClicked = true;

        // 2. 视觉反馈：改变颜色以示不可交互
        // Occupied districts remain available for right-click information, but frames
        // are reserved for valid card-placement highlights.
        SetFrameVisible(false, clickedColor);

        // 3. 保持碰撞器启用，使被占用的地点仍能被右键查看信息；
        //    再次出牌由 hasBeenClicked / IsAvailable() 拦截。
    }

    // 右键：在鼠标旁弹出地点信息浮窗（名称 + 描述）。
    private void ShowInfoPopup(Vector2 screenPos)
    {
        bool chinese = Cards != null && Cards.UseChinese;
        LocationInfoPopup.EnsureExists().Show(GetInfoName(chinese), GetInfoDescription(chinese), screenPos);
    }

    public string GetInfoName(bool chinese)
    {
        if (chinese && !string.IsNullOrEmpty(infoNameChinese)) return infoNameChinese;
        return infoName;
    }

    public string GetInfoDescription(bool chinese)
    {
        if (chinese && !string.IsNullOrEmpty(infoDescriptionChinese)) return infoDescriptionChinese;
        return infoDescription;
    }

    // 高亮当前所选卡牌可以放置的地点（由卡牌系统调用）。
    public void SetHighlighted(bool highlighted)
    {
        if (hasBeenClicked) return;
        SetFrameVisible(highlighted, highlightColor);
    }

    private void CreateHighlightFrame()
    {
        highlightFrame = GetComponent<LineRenderer>();
        if (highlightFrame == null)
        {
            highlightFrame = gameObject.AddComponent<LineRenderer>();
        }

        // Location transforms scale to the individual map districts. Draw the frame in
        // world space so its thickness remains consistent across all twelve regions.
        highlightFrame.useWorldSpace = true;
        highlightFrame.loop = false;
        highlightFrame.positionCount = 5;
        highlightFrame.startWidth = 0.42f;
        highlightFrame.endWidth = 0.42f;
        highlightFrame.numCornerVertices = 4;
        highlightFrame.numCapVertices = 2;
        highlightFrame.sortingOrder = 50;
        highlightFrame.material = new Material(Shader.Find("Sprites/Default"));
        UpdateFrameShape();
        highlightFrame.enabled = false;
    }

    private void UpdateFrameShape()
    {
        if (highlightFrame == null) return;
        Vector2 size = collider2D is BoxCollider2D box ? box.size : Vector2.one;
        float halfWidth = size.x * 0.5f;
        float halfHeight = size.y * 0.5f;
        highlightFrame.SetPositions(new[]
        {
            transform.TransformPoint(-halfWidth, -halfHeight, 0f),
            transform.TransformPoint(-halfWidth, halfHeight, 0f),
            transform.TransformPoint(halfWidth, halfHeight, 0f),
            transform.TransformPoint(halfWidth, -halfHeight, 0f),
            transform.TransformPoint(-halfWidth, -halfHeight, 0f)
        });
    }

    private void SetFrameVisible(bool visible, Color color)
    {
        if (highlightFrame == null) return;
        UpdateFrameShape();
        highlightFrame.startColor = color;
        highlightFrame.endColor = color;
        highlightFrame.enabled = visible;
    }

    public LocationType GetLocationType() { return locationType; }
    public bool IsAvailable() { return !hasBeenClicked; }

    public bool TryGetPreviewEffect(out StatModifier effect)
    {
        foreach (MonoBehaviour behaviour in GetComponents<MonoBehaviour>())
        {
            if (behaviour is ILocationEffectPreview previewProvider)
            {
                effect = previewProvider.GetPreviewEffect();
                return true;
            }
        }

        effect = default;
        return false;
    }

    public StatManager VisitStats() { return Stats; }
    public TurnController VisitTurns() { return Turns; }
}
