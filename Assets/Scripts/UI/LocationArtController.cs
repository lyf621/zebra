using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Presents all gameplay locations as connected regions of one map. A future full-map
/// illustration can be supplied at Resources/Art/Map/MainMap without changing gameplay.
/// </summary>
public sealed class LocationArtController : MonoBehaviour
{
    private const int Columns = 3;
    private const float MapSidePadding = 12f;
    private const float MapVerticalPadding = 16f;

    private readonly List<TextMesh> labels = new List<TextMesh>();
    private ZebraGameController cards;
    private Sprite borderSprite;

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
        cards = FindAnyObjectByType<ZebraGameController>();
        if (cards != null) cards.LanguageChanged += RefreshLabels;
        BuildMap();
    }

    private void OnDestroy()
    {
        if (cards != null) cards.LanguageChanged -= RefreshLabels;
    }

    private void BuildMap()
    {
        ClickOnLocation[] locations = FindObjectsByType<ClickOnLocation>(FindObjectsSortMode.None);
        if (locations == null || locations.Length == 0) return;

        System.Array.Sort(locations, CompareLocations);
        SpriteRenderer firstRenderer = locations[0].GetComponent<SpriteRenderer>();
        if (firstRenderer == null || firstRenderer.sprite == null) return;

        Vector2 tileSize = firstRenderer.sprite.bounds.size;
        Vector3 firstScale = firstRenderer.transform.lossyScale;
        float tileWidth = tileSize.x * Mathf.Abs(firstScale.x);
        float tileHeight = tileSize.y * Mathf.Abs(firstScale.y);
        int rows = Mathf.CeilToInt(locations.Length / (float)Columns);
        float mapWidth = tileWidth * Columns;
        float mapHeight = tileHeight * rows;

        CreateMapBackground(mapWidth, mapHeight);
        for (int i = 0; i < locations.Length; i++)
        {
            ClickOnLocation location = locations[i];
            if (location == null) continue;
            SpriteRenderer renderer = location.GetComponent<SpriteRenderer>();
            if (renderer == null || renderer.sprite == null) continue;

            int column = i % Columns;
            int row = i / Columns;
            location.transform.position = new Vector3(
                (column - (Columns - 1) * 0.5f) * tileWidth,
                ((rows - 1) * 0.5f - row) * tileHeight,
                location.transform.position.z);

            // Tiles become transparent click regions over the single map image.
            renderer.color = new Color(1f, 1f, 1f, 0.06f);
            SpriteRenderer border = CreateBorder(location.transform, renderer.sprite.bounds.size);
            location.SetStatusRenderer(border);
            labels.Add(CreateTypeLabel(location, renderer.sprite.bounds.size));
        }

        FrameMap(mapWidth, mapHeight);
    }

    private int CompareLocations(ClickOnLocation a, ClickOnLocation b)
    {
        return GetSortKey(a).CompareTo(GetSortKey(b));
    }

    private string GetSortKey(ClickOnLocation location)
    {
        if (location == null) return string.Empty;
        // Stable category-first order leaves useful neighbourhoods for the map artist.
        return location.GetLocationType().ToString() + "-" + location.gameObject.name;
    }

    private void CreateMapBackground(float mapWidth, float mapHeight)
    {
        GameObject backgroundObject = new GameObject("Map Background", typeof(SpriteRenderer));
        backgroundObject.transform.position = Vector3.zero;
        SpriteRenderer background = backgroundObject.GetComponent<SpriteRenderer>();
        background.sortingOrder = -20;

        Sprite mapArt = Resources.Load<Sprite>("Art/Map/MainMap");
        if (mapArt != null)
        {
            float pixelsPerUnit = mapArt.rect.width / mapWidth;
            background.sprite = Sprite.Create(mapArt.texture, mapArt.rect, new Vector2(0.5f, 0.5f), pixelsPerUnit, 0, SpriteMeshType.FullRect);
            background.color = Color.white;
            return;
        }

        Texture2D fallbackTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        fallbackTexture.SetPixel(0, 0, new Color(0.18f, 0.31f, 0.22f, 1f));
        fallbackTexture.Apply();
        background.sprite = Sprite.Create(fallbackTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        background.transform.localScale = new Vector3(mapWidth, mapHeight, 1f);
    }

    private SpriteRenderer CreateBorder(Transform parent, Vector2 localTileSize)
    {
        GameObject borderObject = new GameObject("Location Border", typeof(SpriteRenderer));
        borderObject.transform.SetParent(parent, false);
        borderObject.transform.localPosition = new Vector3(0f, 0f, -0.1f);
        borderObject.transform.localScale = new Vector3(localTileSize.x, localTileSize.y, 1f);
        SpriteRenderer border = borderObject.GetComponent<SpriteRenderer>();
        border.sprite = GetBorderSprite();
        border.sortingOrder = 10;
        return border;
    }

    private TextMesh CreateTypeLabel(ClickOnLocation location, Vector2 localTileSize)
    {
        GameObject labelObject = new GameObject("Location Type Label", typeof(TextMesh));
        labelObject.transform.SetParent(location.transform, false);
        labelObject.transform.localPosition = new Vector3(0f, localTileSize.y * 0.38f, -0.2f);
        Vector3 scale = location.transform.lossyScale;
        labelObject.transform.localScale = new Vector3(1f / Mathf.Max(Mathf.Abs(scale.x), Mathf.Epsilon), 1f / Mathf.Max(Mathf.Abs(scale.y), Mathf.Epsilon), 1f);

        TextMesh label = labelObject.GetComponent<TextMesh>();
        label.font = GameUITheme.GetLegacyFont();
        label.fontSize = 38;
        label.characterSize = 0.045f;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.color = Color.white;
        label.text = GetLocationTypeText(location.GetLocationType(), cards != null && cards.UseChinese);
        label.GetComponent<MeshRenderer>().sortingOrder = 12;
        return label;
    }

    private Sprite GetBorderSprite()
    {
        if (borderSprite != null) return borderSprite;
        const int size = 64;
        const int thickness = 3;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool edge = x < thickness || x >= size - thickness || y < thickness || y >= size - thickness;
                texture.SetPixel(x, y, edge ? Color.white : Color.clear);
            }
        }
        texture.Apply();
        borderSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        return borderSprite;
    }

    private void FrameMap(float mapWidth, float mapHeight)
    {
        Camera camera = Camera.main;
        if (camera == null || !camera.orthographic) return;
        float horizontalFit = (mapWidth + MapSidePadding) / (2f * camera.aspect);
        float verticalFit = (mapHeight + MapVerticalPadding) * 0.5f;
        camera.orthographicSize = Mathf.Max(horizontalFit, verticalFit);
        Vector3 position = camera.transform.position;
        camera.transform.position = new Vector3(0f, 0f, position.z);
    }

    private void RefreshLabels(bool chinese)
    {
        ClickOnLocation[] locations = FindObjectsByType<ClickOnLocation>(FindObjectsSortMode.None);
        System.Array.Sort(locations, CompareLocations);
        int count = Mathf.Min(locations.Length, labels.Count);
        for (int i = 0; i < count; i++)
        {
            if (labels[i] != null)
                labels[i].text = GetLocationTypeText(locations[i].GetLocationType(), chinese);
        }
    }

    private string GetLocationTypeText(LocationType type, bool chinese)
    {
        if (!chinese) return type.ToString();
        if (type == LocationType.Economy) return "经济";
        if (type == LocationType.Military) return "军事";
        if (type == LocationType.Administration) return "行政";
        if (type == LocationType.Diplomacy) return "外交";
        return "通用";
    }
}
