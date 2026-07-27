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

    // Pixel outlines traced from the final illustrated map. Keys deliberately follow
    // the visible labels in MainMap: gameplay identity, tooltip, collider and highlight
    // must all refer to the building beneath the same label.
    private static readonly Dictionary<string, Vector2[]> LocationPolygons = new Dictionary<string, Vector2[]>
    {
        { "mobilization", Points(0, 250, 64, 216, 163, 191, 257, 222, 272, 283, 223, 350, 118, 365, 25, 330) },
        { "generousdonation", Points(300, 232, 355, 193, 421, 205, 460, 257, 451, 326, 394, 354, 329, 337, 294, 290) },
        { "bureaucracy", Points(576, 214, 637, 202, 691, 243, 695, 322, 651, 365, 588, 351, 560, 300) },
        { "patrol", Points(750, 178, 835, 163, 936, 187, 977, 239, 957, 302, 886, 323, 795, 305, 740, 251) },
        { "farm", Points(1012, 172, 1090, 192, 1190, 229, 1357, 275, 1357, 688, 1293, 671, 1205, 650, 1127, 625, 1066, 576, 1030, 516, 1077, 465, 1033, 420, 1075, 373, 1016, 328) },
        { "royalgrace", Points(94, 418, 158, 388, 228, 400, 274, 451, 278, 510, 235, 542, 154, 529, 100, 481) },
        { "barrack", Points(344, 382, 395, 357, 451, 380, 467, 430, 438, 472, 376, 470, 341, 429) },
        { "arsenal", Points(478, 366, 544, 338, 610, 360, 637, 412, 620, 466, 548, 478, 487, 449) },
        { "guild", Points(803, 392, 858, 367, 925, 393, 948, 444, 923, 493, 856, 502, 808, 461) },
        { "ceremony", Points(753, 324, 811, 304, 867, 329, 880, 381, 850, 414, 791, 405, 749, 368) },
        { "market", Points(648, 518, 737, 493, 837, 523, 929, 583, 942, 669, 871, 719, 759, 713, 667, 661, 629, 589) },
        { "alliance", Points(350, 562, 437, 520, 541, 519, 617, 566, 635, 640, 582, 689, 457, 711, 366, 667, 336, 610) }
    };

    private static readonly Dictionary<string, LocationType> ExpectedLocationTypes = new Dictionary<string, LocationType>
    {
        { "farm", LocationType.Economy },
        { "guild", LocationType.Economy },
        { "market", LocationType.Economy },
        { "arsenal", LocationType.Military },
        { "barrack", LocationType.Military },
        { "mobilization", LocationType.Military },
        { "bureaucracy", LocationType.Administration },
        { "ceremony", LocationType.Administration },
        { "patrol", LocationType.Administration },
        { "alliance", LocationType.Diplomacy },
        { "generousdonation", LocationType.Diplomacy },
        { "royalgrace", LocationType.Diplomacy }
    };

    // ---- District name labels -------------------------------------------------------------
    // Each district carries its own label as a "Label" child of Location.prefab, so the label
    // inherits the district's position and, through it, the map's runtime scaling from
    // MapViewportFitter. Nothing needs to stay in sync by hand.
    private const string LabelChildName = "Label";
    private const string LabelResourceFolder = "Art/LocationLabels/";
    private const string EnglishLabelResourceFolder = LabelResourceFolder + "English/";
    private const string ChineseLabelResourceFolder = LabelResourceFolder + "Chinese/";
    private const string LabelAssetFolder = "Assets/Resources/Art/LocationLabels";
    private const float LabelPixelsPerUnit = 300f;
    private const string LegacyLabelContainerName = "LocationLabel";

    // The labels previously lived on a root object at world scale 15. Parented under a district
    // (localScale 1) under the map (~14.73), this reproduces their original on-screen size.
    private const float LabelWorldScale = 8f;

    // Per-district nudge applied to the polygon centre, in MAP PIXELS — same convention as
    // LocationPolygons, so +x is right and +y is DOWN. Zero centres the label on its district;
    // raise or lower a value here if a label sits awkwardly over its building.
    private static readonly Dictionary<string, Vector2> LabelPixelOffsets = new Dictionary<string, Vector2>
    {
        { "mobilization", Vector2.zero },
        { "generousdonation", new Vector2(0, 55f) },
        { "bureaucracy", new Vector2(0, 40f) },
        { "patrol", Vector2.zero },
        { "farm", Vector2.zero },
        { "royalgrace", new Vector2(20f, 60f) },
        { "barrack", new Vector2(0, 50f) },
        { "arsenal", new Vector2(0, 30f) },
        { "guild", new Vector2(0, 50f) },
        { "ceremony", new Vector2(-30f, 20f) },
        { "market", Vector2.zero },
        { "alliance", new Vector2(0, 40f) }
    };

    private const float MapPixelWidth = 1358f;
    private const float MapPixelHeight = 818f;

    [MenuItem("Zebra/Apply Unified Map Layout")]
    public static void Apply()
    {
        ConfigureMapImport();
        ConfigureHighlightImports();
        ConfigureLabelImports();
        Sprite mapSprite = AssetDatabase.LoadAssetAtPath<Sprite>(MapPath);
        if (mapSprite == null)
        {
            throw new InvalidOperationException("Map sprite was not imported: " + MapPath);
        }

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Transform mapTransform = CreateOrUpdateMapBackground(mapSprite);
        LayoutLocations(mapSprite, mapTransform);
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
        importer.spritesheet = Array.Empty<SpriteMetaData>();
        TextureImporterSettings mapSettings = new TextureImporterSettings();
        importer.ReadTextureSettings(mapSettings);
        mapSettings.spriteMeshType = SpriteMeshType.FullRect;
        mapSettings.spriteAlignment = (int)SpriteAlignment.Center;
        mapSettings.spritePivot = new Vector2(0.5f, 0.5f);
        importer.SetTextureSettings(mapSettings);
        importer.spritePixelsPerUnit = 100;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.textureCompression = TextureImporterCompression.Compressed;
        importer.maxTextureSize = 2048;
        importer.SaveAndReimport();
    }

    private static void ConfigureHighlightImports()
    {
        const string highlightFolder = "Assets/Resources/Art/MapHighlights";
        if (!AssetDatabase.IsValidFolder(highlightFolder)) return;

        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { highlightFolder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritesheet = Array.Empty<SpriteMetaData>();
            TextureImporterSettings highlightSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(highlightSettings);
            highlightSettings.spriteMeshType = SpriteMeshType.FullRect;
            highlightSettings.spriteAlignment = (int)SpriteAlignment.Center;
            highlightSettings.spritePivot = new Vector2(0.5f, 0.5f);
            importer.SetTextureSettings(highlightSettings);
            importer.spritePixelsPerUnit = 100;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
        }
    }

    private static void ConfigureLabelImports()
    {
        if (!AssetDatabase.IsValidFolder(LabelAssetFolder)) return;

        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { LabelAssetFolder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritesheet = Array.Empty<SpriteMetaData>();
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            settings.spritePivot = new Vector2(0.5f, 0.5f);
            importer.SetTextureSettings(settings);
            importer.spritePixelsPerUnit = LabelPixelsPerUnit;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 1024;
            importer.SaveAndReimport();
        }
    }

    private static Transform CreateOrUpdateMapBackground(Sprite mapSprite)
    {
        GameObject background = GameObject.Find("Zebra World Map");
        if (background == null)
        {
            background = new GameObject("Zebra World Map");
        }

        SpriteRenderer renderer = background.GetComponent<SpriteRenderer>();
        if (renderer == null) renderer = background.AddComponent<SpriteRenderer>();
        renderer.sprite = mapSprite;
        renderer.drawMode = SpriteDrawMode.Simple;
        renderer.size = mapSprite.bounds.size;
        renderer.sortingOrder = -100;
        renderer.color = Color.white;
        background.transform.position = new Vector3(0f, MapCenterY, 0f);
        float scale = MapWidth / mapSprite.bounds.size.x;
        background.transform.localScale = new Vector3(scale, scale, 1f);
        MapViewportFitter fitter = background.GetComponent<MapViewportFitter>();
        if (fitter == null) fitter = background.AddComponent<MapViewportFitter>();
        fitter.SetBaseScale(scale);

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

        return background.transform;
    }

    private static void LayoutLocations(Sprite mapSprite, Transform mapTransform)
    {
        List<string> unmapped = new List<string>();
        List<string> missingLabelArt = new List<string>();

        // Recomputed rather than read off the transform: MapViewportFitter raises the map's
        // scale above this at runtime, and the label scale must be relative to the base.
        float mapScale = MapWidth / mapSprite.bounds.size.x;

        foreach (ClickOnLocation location in UnityEngine.Object.FindObjectsByType<ClickOnLocation>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            string key = Normalize(location.name);
            if (!LocationPolygons.TryGetValue(key, out Vector2[] pixelPoints))
            {
                unmapped.Add(location.name);
                continue;
            }

            if (!ExpectedLocationTypes.TryGetValue(key, out LocationType expectedType)
                || location.GetLocationType() != expectedType
                || Normalize(location.GetInfoName(false)) != key)
            {
                throw new InvalidOperationException(
                    $"Location identity mismatch for {location.name}: "
                    + $"type={location.GetLocationType()}, info={location.GetInfoName(false)}");
            }

            location.transform.SetParent(mapTransform, false);
            Vector2 centerPixel = pixelPoints.Aggregate(Vector2.zero, (sum, point) => sum + point) / pixelPoints.Length;
            Vector2 centerLocal = PixelToMapLocal(centerPixel, mapSprite.bounds.size);
            location.transform.localPosition = new Vector3(centerLocal.x, centerLocal.y, 0f);
            location.transform.localScale = Vector3.one;

            PolygonCollider2D collider = location.GetComponent<PolygonCollider2D>();
            if (collider == null) collider = location.gameObject.AddComponent<PolygonCollider2D>();
            foreach (BoxCollider2D oldCollider in location.GetComponents<BoxCollider2D>())
                UnityEngine.Object.DestroyImmediate(oldCollider);
            collider.points = pixelPoints
                .Select(point => PixelToMapLocal(point, mapSprite.bounds.size) - centerLocal)
                .ToArray();
            collider.enabled = true;

            SpriteRenderer artwork = location.GetComponent<SpriteRenderer>();
            if (artwork != null) artwork.enabled = false;

            if (!ApplyLabel(location, key, mapSprite, centerLocal, centerPixel, mapScale))
            {
                missingLabelArt.Add(location.name);
            }

            EditorUtility.SetDirty(location.gameObject);
        }

        RetireLegacyLabels();

        if (unmapped.Count > 0)
        {
            Debug.LogWarning("Map layout skipped locations: " + string.Join(", ", unmapped));
        }

        if (missingLabelArt.Count > 0)
        {
            Debug.LogWarning("No label art in Resources/" + LabelResourceFolder + " for: "
                             + string.Join(", ", missingLabelArt) + " (their labels are hidden).");
        }
    }

    /// <summary>
    /// Points the district's "Label" child at its name art and places it relative to the
    /// district. Returns false when no label sprite exists for this district — the tutorial's
    /// Palace and Embassy share Location.prefab but have no map label, so their renderer is
    /// simply switched off rather than drawing an empty quad.
    /// </summary>
    private static bool ApplyLabel(ClickOnLocation location, string key, Sprite mapSprite,
                                   Vector2 centerLocal, Vector2 centerPixel, float mapScale)
    {
        Transform label = location.transform.Find(LabelChildName);
        if (label == null)
        {
            throw new InvalidOperationException(
                $"{location.name} has no '{LabelChildName}' child. Add an empty child called "
                + $"'{LabelChildName}' with a SpriteRenderer to Assets/Resources/Prefabs/Location.prefab.");
        }

        SpriteRenderer renderer = label.GetComponent<SpriteRenderer>();
        if (renderer == null) renderer = label.gameObject.AddComponent<SpriteRenderer>();

        string assetKey = AssetKey(location.name);
        Sprite englishArt = Resources.Load<Sprite>(EnglishLabelResourceFolder + assetKey);
        Sprite chineseArt = Resources.Load<Sprite>(ChineseLabelResourceFolder + assetKey);
        renderer.sprite = englishArt;
        renderer.enabled = englishArt != null || chineseArt != null;
        renderer.sortingOrder = 0;   // map -100, highlight overlays -90, labels 0, selection frames 50

        LocationLabelLocalizer localizer = label.GetComponent<LocationLabelLocalizer>();
        if (localizer == null) localizer = label.gameObject.AddComponent<LocationLabelLocalizer>();
        localizer.Configure(englishArt, chineseArt);

        // The offset is applied in pixel space and converted as a delta, so the +y-is-down
        // convention of LocationPolygons carries over without a sign flip here.
        LabelPixelOffsets.TryGetValue(key, out Vector2 offsetPixels);
        Vector2 labelLocal = PixelToMapLocal(centerPixel + offsetPixels, mapSprite.bounds.size);
        label.localPosition = new Vector3(labelLocal.x - centerLocal.x, labelLocal.y - centerLocal.y, 0f);

        // The district sits at localScale 1 under the map, so dividing by the map's base scale
        // leaves the label at its original world size — and it now rides the fitter's rescaling.
        label.localScale = Vector3.one * (LabelWorldScale / mapScale);

        EditorUtility.SetDirty(label.gameObject);
        return englishArt != null && chineseArt != null;
    }

    /// <summary>
    /// The labels used to be 12 sprites under a root "LocationLabel" object, outside the map
    /// hierarchy — which is why they never followed MapViewportFitter. They are superseded by
    /// the per-district Label children, so hide them here. Disabled rather than deleted so the
    /// change is reversible; the container can be removed from the scene by hand afterwards.
    /// </summary>
    private static void RetireLegacyLabels()
    {
        GameObject legacy = GameObject.Find(LegacyLabelContainerName);
        if (legacy == null) return;

        foreach (SpriteRenderer renderer in legacy.GetComponentsInChildren<SpriteRenderer>(true))
        {
            renderer.enabled = false;
            EditorUtility.SetDirty(renderer.gameObject);
        }

        Debug.Log($"Disabled the legacy '{LegacyLabelContainerName}' sprites — districts now carry "
                  + "their own labels. The container can be deleted from the scene.");
    }

    private static string Normalize(string value)
    {
        return new string(value.Where(char.IsLetterOrDigit).ToArray())
            .TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9')
            .ToLowerInvariant();
    }

    /// <summary>
    /// Normalize without the lowercasing: "RoyalGrace_0" -> "RoyalGrace". The label art in
    /// Resources/Art/Locations is PascalCase while the highlight art is lowercase, and
    /// Resources.Load is case-insensitive in the Editor but not on every build target — so the
    /// asset name has to be reproduced exactly rather than lowercased.
    /// </summary>
    private static string AssetKey(string value)
    {
        return new string(value.Where(char.IsLetterOrDigit).ToArray())
            .TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
    }

    private static Vector2[] Points(params float[] values)
    {
        if (values.Length % 2 != 0) throw new ArgumentException("Polygon points must be x/y pairs.");
        Vector2[] points = new Vector2[values.Length / 2];
        for (int index = 0; index < values.Length; index += 2)
            points[index / 2] = new Vector2(values[index], values[index + 1]);
        return points;
    }

    private static Vector2 PixelToMapLocal(Vector2 pixel, Vector2 spriteSize)
    {
        return new Vector2(
            -spriteSize.x * 0.5f + pixel.x / MapPixelWidth * spriteSize.x,
            spriteSize.y * 0.5f - pixel.y / MapPixelHeight * spriteSize.y);
    }
}
