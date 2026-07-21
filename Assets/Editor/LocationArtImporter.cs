using UnityEditor;

public sealed class LocationArtImporter : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith("Assets/Resources/Art/Locations/") || !assetPath.Contains("-v2."))
        {
            return;
        }

        TextureImporter importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.maxTextureSize = 2048;
        importer.textureCompression = TextureImporterCompression.Compressed;
    }
}
