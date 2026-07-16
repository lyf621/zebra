using UnityEngine;
using UnityEngine.EventSystems;

public class OneClick2DObject : MonoBehaviour, IPointerClickHandler
{
    [Header("点击效果（在 Inspector 中绑定）")]
    [SerializeField] private UnityEngine.Events.UnityEvent onFirstClick;

    [Header("外观反馈（可选）")]
    [SerializeField] private Color clickedColor = Color.gray;
    [SerializeField] private Color defaultColor = Color.white;

    private Collider2D collider2D;
    private SpriteRenderer spriteRenderer;
    private bool hasBeenClicked = false;

    private void Start()
    {
        // 获取必要组件
        collider2D = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (collider2D == null)
            Debug.LogError("物体缺少 Collider2D，无法接收点击！");
    }

    // IPointerClickHandler 接口实现
    public void OnPointerClick(PointerEventData eventData)
    {
        if (hasBeenClicked) return; // 已经触发过，忽略

        // 1. 执行你绑定的效果（如获得金币、播放音效等）
        
        DisableObject();
        
        onFirstClick?.Invoke();
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
}