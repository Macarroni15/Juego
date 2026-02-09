using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(KitchenBootstrap))]
public class KitchenBootstrapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Dibujar el inspector por defecto (variables públicas, etc.)
        DrawDefaultInspector();

        KitchenBootstrap script = (KitchenBootstrap)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Editor de Mapa", EditorStyles.boldLabel);

        if (GUILayout.Button("GENERAR MAPA (RESTAURANTE)", GUILayout.Height(40)))
        {
            script.GenerarRestaurante();
            EditorUtility.SetDirty(script);
        }

        if (script.restauranteContainer != null)
        {
            EditorGUILayout.HelpBox("Mapa generado. Puedes editar los objetos en la jerarquía. El mapa se guardará con la escena.", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("No hay mapa en la escena. Pulsa el botón para generarlo y poder editarlo.", MessageType.Warning);
        }
    }
}
