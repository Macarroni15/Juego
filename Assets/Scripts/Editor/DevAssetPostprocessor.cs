using UnityEngine;
using UnityEditor;

public class DevAssetPostprocessor : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (assetPath.Contains("Resources/Food") || assetPath.Contains("Resources/Characters"))
        {
            TextureImporter importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            // importer.alphaIsTransparency = true; // Optional, usually safe to skip for simple placeholders
        }
    }
}
