using UnityEngine;
using UnityEngine.EventSystems;

public class ClickOnLocation : MonoBehaviour, IPointerClickHandler
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

    private Collider2D collider2D;
    private SpriteRenderer spriteRenderer;
    private bool hasBeenClicked = false;

    private void Start()
    {
        // 获取必要组件
        collider2D = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (Cards == null) Cards = FindAnyObjectByType<ZebraGameController>();

        if (collider2D == null)
            Debug.LogError("物体缺少 Collider2D，无法接收点击！");
    }

    // IPointerClickHandler 接口实现
    public void OnPointerClick(PointerEventData eventData)
    {
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
        if (spriteRenderer != null)
            spriteRenderer.color = defaultColor;
    }

    public void DisableObject()
    {
        hasBeenClicked = true;

        // 2. 视觉反馈：改变颜色以示不可交互
        if (spriteRenderer != null)
            spriteRenderer.color = clickedColor;

        // 3. 禁用碰撞器，阻止后续射线检测（彻底关闭交互）
        if (collider2D != null)
            collider2D.enabled = false;
    }

    // 高亮当前所选卡牌可以放置的地点（由卡牌系统调用）。
    public void SetHighlighted(bool highlighted)
    {
        if (spriteRenderer == null || hasBeenClicked) return;
        spriteRenderer.color = highlighted ? highlightColor : defaultColor;
    }

    public LocationType GetLocationType() { return locationType; }
    public bool IsAvailable() { return !hasBeenClicked; }

    public StatManager VisitStats() { return Stats; }
    public TurnController VisitTurns() { return Turns; }
}
