using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class AutoInstaller
{
    static AutoInstaller()
    {
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
    }

    private static void OnHierarchyChanged()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        if (Object.FindFirstObjectByType<KitchenBootstrap>() == null)
        {
            GameObject go = new GameObject("--- RESTAURANT GENERATOR ---");
            go.AddComponent<KitchenBootstrap>();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
}
#endif
