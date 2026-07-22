using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ZebraMapSceneSetup
{
    private const string ScenePath = "Assets/Scenes/MainMap.unity";
    private const string MapPath = "Assets/Art/Maps/ZebraWorldMap.png";
    private const float MapWidth = 200f;
    private const float MapCenterY = 8f;

    private static readonly Dictionary<string, Vector2Int> LocationCells = new Dictionary<string, Vector2Int>
    {
        { "royalgrace", new Vector2Int(0, 0) },
        { "bureaucracy", new Vector2Int(1, 0) },
        { "farm", new Vector2Int(2, 0) },
        { "barrack", new Vector2Int(3, 0) },
        { "generousdonation", new Vector2Int(0, 1) },
        { "ceremony", new Vector2Int(1, 1) },
        { "guild", new Vector2Int(2, 1) },
        { "arsenal", new Vector2Int(3, 1) },
        { "alliance", new Vector2Int(0, 2) },
        { "patrol", new Vector2Int(1, 2) },
        { "market", new Vector2Int(2, 2) },
        { "mobilization", new Vector2Int(3, 2) }
    };

    [MenuItem("Zebra/Apply Unified Map Layout")]
    public static void Apply()
    {
        ConfigureMapImport();
        Sprite mapSprite = AssetDatabase.LoadAssetAtPath<Sprite>(MapPath);
        if (mapSprite == null)
        {
            throw new InvalidOperationException("Map sprite was not imported: " + MapPath);
        }

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        CreateOrUpdateMapBackground(mapSprite);
        LayoutLocations(mapSprite.bounds.size.x / mapSprite.bounds.size.y);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Zebra unified map layout applied to MainMap.");
    }

    public static void DiagnoseCanvasLayers()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        foreach (Image image in UnityEngine.Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            RectTransform rect = image.rectTransform;
            Debug.Log("UI image: " + image.name + " active=" + image.gameObject.activeInHierarchy +
                      " anchors=" + rect.anchorMin + "-" + rect.anchorMax + " color=" + image.color);
        }

        foreach (SpriteRenderer sprite in UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Debug.Log("Sprite: " + sprite.name + " enabled=" + sprite.enabled + " order=" + sprite.sortingOrder +
                      " sprite=" + (sprite.sprite != null ? sprite.sprite.name : "none"));
        }
    }

    private static void ConfigureMapImport()
    {
        TextureImporter importer = AssetImporter.GetAtPath(MapPath) as TextureImporter;
        if (importer == null)
        {
            throw new InvalidOperationException("Map image was not found: " + MapPath);
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = false;
        importer.textureCompression = TextureImporterCompression.Compressed;
        importer.maxTextureSize = 2048;
        importer.SaveAndReimport();
    }

    private static void CreateOrUpdateMapBackground(Sprite mapSprite)
    {
        GameObject background = GameObject.Find("Zebra World Map");
        if (background == null)
        {
            background = new GameObject("Zebra World Map");
        }

        SpriteRenderer renderer = background.GetComponent<SpriteRenderer>();
        if (renderer == null) renderer = background.AddComponent<SpriteRenderer>();
        renderer.sprite = mapSprite;
        renderer.sortingOrder = -100;
        renderer.color = Color.white;
        background.transform.position = new Vector3(0f, MapCenterY, 0f);
        float scale = MapWidth / mapSprite.bounds.size.x;
        background.transform.localScale = new Vector3(scale, scale, 1f);

        // The old scene used a full-screen Square sprite as its background. It sits above
        // the new map in sorting order, so disable it instead of letting it mask the artwork.
        foreach (SpriteRenderer existing in UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (existing != renderer && existing.gameObject.name == "Square")
            {
                existing.enabled = false;
                EditorUtility.SetDirty(existing);
            }
        }
    }

    private static void LayoutLocations(float mapAspect)
    {
        float mapHeight = MapWidth / mapAspect;
        float cellWidth = MapWidth / 4f;
        float cellHeight = mapHeight / 3f;
        List<string> unmapped = new List<string>();

        foreach (ClickOnLocation location in UnityEngine.Object.FindObjectsByType<ClickOnLocation>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            string key = Normalize(location.name);
            if (!LocationCells.TryGetValue(key, out Vector2Int cell))
            {
                unmapped.Add(location.name);
                continue;
            }

            location.transform.SetParent(null, true);
            location.transform.position = new Vector3(
                -MapWidth * 0.5f + cellWidth * (cell.x + 0.5f),
                MapCenterY + mapHeight * 0.5f - cellHeight * (cell.y + 0.5f),
                0f);
            location.transform.localScale = new Vector3(cellWidth, cellHeight, 1f);

            BoxCollider2D collider = location.GetComponent<BoxCollider2D>();
            if (collider != null)
            {
                collider.size = Vector2.one;
                collider.offset = Vector2.zero;
            }

            SpriteRenderer artwork = location.GetComponent<SpriteRenderer>();
            if (artwork != null) artwork.enabled = false;
            EditorUtility.SetDirty(location.gameObject);
        }

        if (unmapped.Count > 0)
        {
            Debug.LogWarning("Map layout skipped locations: " + string.Join(", ", unmapped));
        }
    }

    private static string Normalize(string value)
    {
        return new string(value.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
    }
}
