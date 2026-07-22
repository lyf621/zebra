using UnityEngine;

/// <summary>
/// Applies the environment-first location illustrations at runtime without changing
/// the scene layout or the location gameplay scripts.
/// </summary>
public sealed class LocationArtController : MonoBehaviour
{
    public static LocationArtController EnsureExists()
    {
        LocationArtController controller = FindAnyObjectByType<LocationArtController>();
        if (controller == null)
        {
            controller = new GameObject("Location Art Controller").AddComponent<LocationArtController>();
        }
        return controller;
    }

    private void Start()
    {
        ApplyLocationArt();
    }

    private void ApplyLocationArt()
    {
        ClickOnLocation[] locations = FindObjectsByType<ClickOnLocation>(FindObjectsSortMode.None);
        foreach (ClickOnLocation location in locations)
        {
            if (location == null) continue;

            string locationName = location.gameObject.name;
            if (string.IsNullOrEmpty(locationName)) continue;
            Sprite sprite = Resources.Load<Sprite>("Art/Locations/" + locationName + "-v2");
            if (sprite == null)
            {
                sprite = Resources.Load<Sprite>("Art/Locations/" + locationName);
            }
            if (sprite == null)
            {
                Debug.LogWarning("LocationArtController: no art found for " + locationName);
                continue;
            }

            SpriteRenderer renderer = location.GetComponent<SpriteRenderer>();
            if (renderer == null) continue;

            Sprite originalSprite = renderer.sprite;
            if (originalSprite == null) continue;

            // The prefab uses a 1 x 1 local sprite with a large transform scale. Imported
            // artwork has a much larger native Sprite size, so create a fitted runtime sprite
            // whose full aspect-ratio image stays within the original tile bounds.
            Vector2 originalSize = originalSprite.bounds.size;
            float pixelsPerUnit = Mathf.Max(
                sprite.rect.width / Mathf.Max(originalSize.x, Mathf.Epsilon),
                sprite.rect.height / Mathf.Max(originalSize.y, Mathf.Epsilon));
            Vector2 pivot = new Vector2(
                sprite.pivot.x / sprite.rect.width,
                sprite.pivot.y / sprite.rect.height);

            renderer.drawMode = SpriteDrawMode.Simple;
            renderer.sprite = Sprite.Create(sprite.texture, sprite.rect, pivot, pixelsPerUnit, 0, SpriteMeshType.FullRect);
            renderer.color = Color.white;
        }
    }
}
