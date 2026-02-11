using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
[InitializeOnLoad]
public class DevAssetGenerator
{
    static DevAssetGenerator()
    {
        // GenerateMissingAssets(); // Removido para evitar generación automática al cargar el editor
    }

    private static void GenerateMissingAssets()
    {
        // 1. Ensure directories exist
        string resourcesPath = Application.dataPath + "/Resources";
        string foodPath = resourcesPath + "/Food";
        string charPath = resourcesPath + "/Characters";

        if (!Directory.Exists(resourcesPath)) Directory.CreateDirectory(resourcesPath);
        if (!Directory.Exists(foodPath)) Directory.CreateDirectory(foodPath);
        if (!Directory.Exists(charPath)) Directory.CreateDirectory(charPath);

        // 2. Define assets to generate (List of relative paths)
        var assetsToGen = new Dictionary<string, Color>();
        assetsToGen.Add("Assets/Resources/Characters/Cocinera.png", Color.magenta);
        assetsToGen.Add("Assets/Resources/Food/Burger.png", new Color(0.6f, 0.3f, 0f));
        assetsToGen.Add("Assets/Resources/Food/Toast.png", new Color(0.9f, 0.8f, 0.4f));
        assetsToGen.Add("Assets/Resources/Food/Fish.png", new Color(0.4f, 0.6f, 1f));
        assetsToGen.Add("Assets/Resources/Food/Pasta.png", new Color(1f, 0.8f, 0.2f));
        assetsToGen.Add("Assets/Resources/Food/Salad.png", Color.green);
        assetsToGen.Add("Assets/Resources/Food/Stew.png", new Color(0.5f, 0f, 0f));
        assetsToGen.Add("Assets/Resources/Food/Rice.png", Color.white);
        assetsToGen.Add("Assets/Resources/Food/Juice.png", new Color(1f, 0.5f, 0f));
        assetsToGen.Add("Assets/Resources/Food/Sandwich.png", new Color(0.8f, 0.7f, 0.5f));

        bool createdAny = false;

        foreach (var kvp in assetsToGen)
        {
            string relativePath = kvp.Key;
            string fullPath = Application.dataPath + relativePath.Substring("Assets".Length);

            if (!File.Exists(fullPath))
            {
                CreateTextureFile(fullPath, kvp.Value);
                createdAny = true;
            }
        }

        if (createdAny)
        {
            AssetDatabase.Refresh(); // The Postprocessor will handle the import settings automatically now
        }
    }

    [MenuItem("Tools/Regenerate Dev Assets")]
    public static void ManualGenerate()
    {
        GenerateMissingAssets();
    }

    private static void CreateTextureFile(string fullPath, Color color)
    {
        int size = 256;
        Texture2D texture = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        for (int i = 0; i < colors.Length; i++) colors[i] = color;
        texture.SetPixels(colors);
        // Border
        for (int x = 0; x < size; x++) {
            for (int y = 0; y < size; y++) {
                if (x < 10 || x > size - 10 || y < 10 || y > size - 10) texture.SetPixel(x, y, Color.black);
            }
        }
        texture.Apply();
        File.WriteAllBytes(fullPath, texture.EncodeToPNG());
    }
}
#endif
