using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[ExecuteAlways]
public class KitchenBootstrap : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("KitchenBootstrap Start. Playing: " + Application.isPlaying);
        if (Application.isPlaying) SetupScene();
    }

    private void Reset() 
    { 
        Debug.Log("KitchenBootstrap Reset called in Editor.");
        SetupScene(); 
    }

    [ContextMenu("!!! FORCE GENERATE RESTAURANT !!!")]
    public void SetupScene()
    {
        Debug.Log("URGENTE: Iniciando generación forzada del restaurante...");

        // Usar un nombre único para evitar confusiones
        string containerName = "RESTAURANTE_GENERADO_AUTOMATICAMENTE";
        
        // Limpieza agresiva de cualquier resto anterior
        GameObject old = GameObject.Find(containerName);
        if (old != null)
        {
            if (Application.isPlaying) Destroy(old);
            else DestroyImmediate(old);
        }

        GameObject container = new GameObject(containerName);
        container.transform.position = Vector3.zero;

        // 1. Cámara y Luz (Si no hay)
        if (Camera.main == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.transform.SetParent(container.transform);
            camObj.AddComponent<Camera>();
            camObj.transform.position = new Vector3(0, 15, -12);
            camObj.transform.rotation = Quaternion.Euler(55, 0, 0);
            camObj.tag = "MainCamera";
        }

        // Luz direccional básica
        GameObject lightObj = new GameObject("Luz");
        lightObj.transform.SetParent(container.transform);
        Light l = lightObj.AddComponent<Light>();
        l.type = LightType.Directional;
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

        // 2. Arquitectura (Suelo y Paredes)
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Suelo";
        floor.transform.SetParent(container.transform);
        floor.transform.localScale = new Vector3(5, 1, 5);
        floor.GetComponent<Renderer>().material.color = new Color(0.2f, 0.2f, 0.2f);

        CreateWall("Pared_Fondo", new Vector3(0, 2, 25), new Vector3(50, 4, 1), container.transform);
        CreateWall("Pared_Derecha", new Vector3(25, 2, 0), new Vector3(1, 4, 50), container.transform);
        CreateWall("Pared_Izquierda", new Vector3(-25, 2, 0), new Vector3(1, 4, 50), container.transform);
        CreateWall("Barra_Cocina", new Vector3(0, 1.5f, 0), new Vector3(50, 3, 0.5f), container.transform, new Color(0.4f, 0.2f, 0.1f));

        // 3. Manager de Juego
        GameObject gmObj = new GameObject("GameManager");
        gmObj.transform.SetParent(container.transform);
        gmObj.AddComponent<GameManager>();

        // 4. El Cocinero (Jugador) con GORRO
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Cocinero";
        player.transform.SetParent(container.transform);
        player.transform.position = new Vector3(0, 1.1f, 5); // Empezar en la cocina
        player.AddComponent<PlayerController>();
        player.AddComponent<PlayerInput>();
        
        // Gorrito blanco
        GameObject hat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hat.name = "GorroChef";
        hat.transform.SetParent(player.transform);
        hat.transform.localPosition = new Vector3(0, 0.8f, 0);
        hat.transform.localScale = new Vector3(0.6f, 0.4f, 0.6f);
        hat.GetComponent<Renderer>().material.color = Color.white;

        // 5. Estaciones Funcionales
        CreateStation("Tomate", new Vector3(-5, 1, 2), Color.red, typeof(DispenserStation), container.transform, "Tomato");
        CreateStation("Lechuga", new Vector3(-1, 1, 2), Color.green, typeof(DispenserStation), container.transform, "Lettuce");
        CreateStation("Tabla de Cortar", new Vector3(4, 1, 2), Color.gray, typeof(CuttingStation), container.transform);
        CreateStation("Basura", new Vector3(8, 1, 2), Color.black, typeof(TrashStation), container.transform);
        CreateStation("Ventanilla Entrega", new Vector3(0, 1.5f, 0.5f), Color.blue, typeof(DeliveryStation), container.transform);

        // 6. Comedor (Mesas y Clientes)
        for (int i = -1; i <= 1; i++)
        {
            Vector3 tablePos = new Vector3(i * 12, 0.8f, -10);
            GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "Mesa_" + i;
            table.transform.SetParent(container.transform);
            table.transform.position = tablePos;
            table.transform.localScale = new Vector3(4, 0.2f, 4);
            table.GetComponent<Renderer>().material.color = new Color(0.3f, 0.15f, 0.1f);

            GameObject customer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            customer.name = "Cliente_" + i;
            customer.transform.SetParent(container.transform);
            customer.transform.position = tablePos + Vector3.up * 1.5f;
            customer.GetComponent<Renderer>().material.color = Color.magenta;
        }

        Debug.Log("URGENTE: Restaurante generado al 100%. Revisa la escena.");
    }

    private void CreateWall(string n, Vector3 p, Vector3 s, Transform par, Color? c = null)
    {
        GameObject w = GameObject.CreatePrimitive(PrimitiveType.Cube);
        w.name = n; w.transform.SetParent(par); w.transform.position = p; w.transform.localScale = s;
        w.GetComponent<Renderer>().material.color = c ?? Color.gray;
    }

    private void CreateStation(string n, Vector3 p, Color c, System.Type t, Transform par, string ing = "")
    {
        GameObject s = GameObject.CreatePrimitive(PrimitiveType.Cube);
        s.name = n; s.transform.SetParent(par); s.transform.position = p; s.transform.localScale = new Vector3(1.5f, 1, 1.5f);
        s.GetComponent<Renderer>().material.color = c;
        s.AddComponent(t);
        if (t == typeof(DispenserStation))
        {
            DispenserStation ds = s.GetComponent<DispenserStation>();
            ds.ingredient = ScriptableObject.CreateInstance<IngredientSO>();
            ds.ingredient.ingredientName = ing;
            ds.ingredient.canBeCut = true;
        }
    }
}
