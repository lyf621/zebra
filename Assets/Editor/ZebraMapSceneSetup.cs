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

    // Pixel rectangles traced from ZebraWorldMap. Their edges meet at the roads, rather
    // than treating the illustrated districts as a uniform four-by-three grid.
    private static readonly Dictionary<string, Rect> LocationRects = new Dictionary<string, Rect>
    {
        { "royalgrace", new Rect(0, 0, 435, 331) },
        { "bureaucracy", new Rect(435, 0, 393, 331) },
        { "farm", new Rect(828, 0, 417, 331) },
        { "barrack", new Rect(1245, 0, 427, 331) },
        { "generousdonation", new Rect(0, 331, 411, 274) },
        { "ceremony", new Rect(411, 331, 391, 274) },
        { "guild", new Rect(802, 331, 412, 274) },
        { "arsenal", new Rect(1214, 331, 458, 274) },
        { "alliance", new Rect(0, 605, 467, 336) },
        { "patrol", new Rect(467, 605, 294, 336) },
        { "market", new Rect(761, 605, 435, 336) },
        { "mobilization", new Rect(1196, 605, 476, 336) }
    };

    private const float MapPixelWidth = 1672f;
    private const float MapPixelHeight = 941f;

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
        List<string> unmapped = new List<string>();

        foreach (ClickOnLocation location in UnityEngine.Object.FindObjectsByType<ClickOnLocation>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            string key = Normalize(location.name);
            if (!LocationRects.TryGetValue(key, out Rect rect))
            {
                unmapped.Add(location.name);
                continue;
            }

            location.transform.SetParent(null, true);
            location.transform.position = new Vector3(
                -MapWidth * 0.5f + (rect.x + rect.width * 0.5f) / MapPixelWidth * MapWidth,
                MapCenterY + mapHeight * 0.5f - (rect.y + rect.height * 0.5f) / MapPixelHeight * mapHeight,
                0f);
            location.transform.localScale = new Vector3(
                rect.width / MapPixelWidth * MapWidth,
                rect.height / MapPixelHeight * mapHeight,
                1f);

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
