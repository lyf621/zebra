using UnityEngine;

/// <summary>
/// Keeps the map large enough to cover an orthographic camera at every window
/// aspect ratio. Location colliders are children of the same transform, so they
/// remain registered to the artwork as the window changes size.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public sealed class MapViewportFitter : MonoBehaviour
{
    private SpriteRenderer mapRenderer;
    private Camera targetCamera;
    [SerializeField, HideInInspector] private float baseScale = 1f;
    private float lastAspect = -1f;
    private float lastOrthographicSize = -1f;

    public void SetBaseScale(float value)
    {
        baseScale = value;
    }

    private void Awake()
    {
        mapRenderer = GetComponent<SpriteRenderer>();
        if (baseScale <= 0f)
            baseScale = transform.localScale.x;
        ApplyScale(true);
    }

    private void LateUpdate()
    {
        ApplyScale(false);
    }

    private void ApplyScale(bool force)
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null || !targetCamera.orthographic || mapRenderer == null || mapRenderer.sprite == null)
            return;

        float aspect = targetCamera.aspect;
        float orthographicSize = targetCamera.orthographicSize;
        if (!force && Mathf.Approximately(aspect, lastAspect) && Mathf.Approximately(orthographicSize, lastOrthographicSize))
            return;

        lastAspect = aspect;
        lastOrthographicSize = orthographicSize;
        Vector2 spriteSize = mapRenderer.sprite.bounds.size;
        float viewHeight = orthographicSize * 2f;
        float viewWidth = viewHeight * aspect;
        float baseWidth = spriteSize.x * baseScale;
        float baseHeight = spriteSize.y * baseScale;
        float coverMultiplier = Mathf.Max(viewWidth / baseWidth, viewHeight / baseHeight);
        float scale = baseScale * Mathf.Max(1f, coverMultiplier);
        transform.localScale = new Vector3(scale, scale, 1f);
    }
}
