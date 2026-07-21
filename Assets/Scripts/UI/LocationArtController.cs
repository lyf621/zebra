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
            renderer.sprite = sprite;
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.size = new Vector2(1f, 1f);
            renderer.color = Color.white;
        }
    }
}
