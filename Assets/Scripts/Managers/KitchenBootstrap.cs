using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

// ESTE SCRIPT HACE TODO: CARGA, MENU Y JUEGO.
public class KitchenBootstrap : MonoBehaviour
{
    private GameObject menuCanvas;
    private GameObject currentMenuPanel;

    private void Start()
    {
        // Solo ejecutar lógica de juego si estamos dando al Play
        if (Application.isPlaying)
        {
            // 1. Asegurar que hay cámara para ver el menú
            if (Camera.main == null)
            {
                GameObject cam = new GameObject("Main Camera");
                cam.AddComponent<Camera>();
                cam.tag = "MainCamera";
                cam.transform.position = new Vector3(0, 1, -10);
            }

            // 2. Asegurar Sistema de Eventos (Clics)
            if (FindObjectOfType<EventSystem>() == null)
            {
                GameObject ev = new GameObject("EventSystem");
                ev.AddComponent<EventSystem>();
                ev.AddComponent<InputSystemUIInputModule>();
            }

            // 3. Empezar con la Pantalla de Carga
            StartCoroutine(SecuenciaInicio());
        }
    }

    // --- SECUENCIA DE INICIO (Carga -> Menu) ---
    IEnumerator SecuenciaInicio()
    {
        CrearCanvas();

        // A) PANTALLA DE CARGA
        GameObject panelCarga = CrearPanel(Color.black);
        CrearTexto(panelCarga, "CARGANDO RESTAURANTE...", 0, 0, 50, Color.white);
        GameObject barra = CrearBarraCarga(panelCarga);
        
        Slider slider = barra.GetComponent<Slider>();
        float tiempo = 0;
        while(tiempo < 2f) // 2 segundos de carga falsa
        {
            tiempo += Time.deltaTime;
            slider.value = tiempo / 2f;
            yield return null;
        }

        Destroy(panelCarga);

        // B) MENU PRINCIPAL
        MostrarMenuPrincipal();
    }

    void MostrarMenuPrincipal()
    {
        if (currentMenuPanel != null) Destroy(currentMenuPanel);

        currentMenuPanel = CrearPanel(new Color(0.1f, 0.1f, 0.2f)); // Fondo Azul Oscuro
        CrearTexto(currentMenuPanel, "COCINA SIMULATOR", 0, 150, 80, Color.yellow);

        // Botones
        CrearBoton(currentMenuPanel, "INICIAR JUEGO", 0, 30, Color.green, () => {
            Destroy(menuCanvas); // Adios menú
            GenerarRestaurante(); // HOLA JUEGO
        });

        CrearBoton(currentMenuPanel, "TUTORIAL", 0, -50, Color.cyan, mostrarTutorial);

        CrearBoton(currentMenuPanel, "SALIR", 0, -130, Color.red, () => {
             Application.Quit();
             #if UNITY_EDITOR
             UnityEditor.EditorApplication.isPlaying = false;
             #endif
        });
    }

    void mostrarTutorial()
    {
        if (currentMenuPanel != null) Destroy(currentMenuPanel);
        currentMenuPanel = CrearPanel(new Color(0.2f, 0.3f, 0.2f)); // Fondo Verde Oscuro

        CrearTexto(currentMenuPanel, "COMO JUGAR", 0, 180, 60, Color.white);
        CrearTexto(currentMenuPanel, 
            "1. Mira la situación del cliente (Dolor, Deporte...).\n" +
            "2. Ve a los dispensadores y coge ingredientes.\n" +
            "3. Cocina en la tabla si es necesario.\n" +
            "4. Entrega el plato en la ventanilla.", 
            0, 0, 35, Color.white);

        CrearBoton(currentMenuPanel, "VOLVER", 0, -180, Color.white, MostrarMenuPrincipal);
    }

    // --- GENERADOR DE UI (Simplificado al máximo) ---
    void CrearCanvas()
    {
        menuCanvas = new GameObject("CanvasMenu");
        Canvas c = menuCanvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        menuCanvas.AddComponent<CanvasScaler>();
        menuCanvas.AddComponent<GraphicRaycaster>();
    }

    GameObject CrearPanel(Color c)
    {
        GameObject p = new GameObject("Panel");
        p.transform.SetParent(menuCanvas.transform, false);
        RectTransform r = p.AddComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.sizeDelta = Vector2.zero;
        Image i = p.AddComponent<Image>(); i.color = c;
        return p;
    }

    void CrearBoton(GameObject parent, string texto, float x, float y, Color colorFondo, UnityEngine.Events.UnityAction accion)
    {
        GameObject b = new GameObject("Btn_" + texto);
        b.transform.SetParent(parent.transform, false);
        Image img = b.AddComponent<Image>(); img.color = colorFondo;
        Button btn = b.AddComponent<Button>(); btn.onClick.AddListener(accion);
        RectTransform r = b.GetComponent<RectTransform>();
        r.sizeDelta = new Vector2(300, 60); r.anchoredPosition = new Vector2(x, y);

        GameObject t = new GameObject("Text"); t.transform.SetParent(b.transform, false);
        Text txt = t.AddComponent<Text>(); txt.text = texto; txt.alignment = TextAnchor.MiddleCenter;
        txt.fontSize = 24; txt.color = Color.black; 
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        RectTransform tr = t.GetComponent<RectTransform>(); tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.sizeDelta = Vector2.zero;
    }

    void CrearTexto(GameObject parent, string contenido, float x, float y, int tamano, Color color)
    {
        GameObject t = new GameObject("Txt"); t.transform.SetParent(parent.transform, false);
        Text txt = t.AddComponent<Text>(); txt.text = contenido; txt.alignment = TextAnchor.MiddleCenter;
        txt.fontSize = tamano; txt.color = color;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        RectTransform r = t.GetComponent<RectTransform>(); r.sizeDelta = new Vector2(800, 400); r.anchoredPosition = new Vector2(x,y);
    }

    GameObject CrearBarraCarga(GameObject parent)
    {
        GameObject b = new GameObject("Barra"); b.transform.SetParent(parent.transform, false);
        Slider s = b.AddComponent<Slider>();
        RectTransform r = b.GetComponent<RectTransform>();
        r.sizeDelta = new Vector2(400, 20); r.anchoredPosition = new Vector2(0, -50);
        
        GameObject fondo = new GameObject("Fondo"); fondo.transform.SetParent(b.transform, false);
        Image imgF = fondo.AddComponent<Image>(); imgF.color = Color.gray;
        fondo.GetComponent<RectTransform>().anchorMin = Vector2.zero; fondo.GetComponent<RectTransform>().anchorMax = Vector2.one; fondo.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        
        GameObject fillArea = new GameObject("FillArea"); fillArea.transform.SetParent(b.transform, false);
        RectTransform faR = fillArea.AddComponent<RectTransform>(); faR.anchorMin = Vector2.zero; faR.anchorMax = Vector2.one; faR.sizeDelta = Vector2.zero;

        GameObject fill = new GameObject("Fill"); fill.transform.SetParent(fillArea.transform, false);
        Image imgFill = fill.AddComponent<Image>(); imgFill.color = Color.green;
        RectTransform fR = fill.GetComponent<RectTransform>(); fR.anchorMin = Vector2.zero; fR.anchorMax = Vector2.one; fR.sizeDelta = Vector2.zero;

        s.targetGraphic = imgF; s.fillRect = fR;
        return b;
    }

    // ==============================================================================================
    // AQUI EMPIEZA LA GENERACION DEL RESTAURANTE (EL CODIGO ORIGINAL QUE YA FUNCIONABA)
    // ==============================================================================================

    public void GenerarRestaurante()
    {
        Debug.Log(">>> GENERANDO RESTAURANTE... <<<<");

        string containerName = "RESTAURANTE_GENERADO_AUTOMATICAMENTE";
        GameObject old = GameObject.Find(containerName);
        if (old != null) Destroy(old);

        GameObject container = new GameObject(containerName);
        container.transform.position = Vector3.zero;

        // 1. Cámara y Luz
        if (Camera.main == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.transform.SetParent(container.transform);
            camObj.AddComponent<Camera>();
            camObj.transform.position = new Vector3(0, 45, -45);
            camObj.transform.rotation = Quaternion.Euler(50, 0, 0);
            camObj.tag = "MainCamera";
        }
        else // Si ya existe (del menú), la recolocamos
        {
            Camera.main.transform.SetParent(container.transform);
            Camera.main.transform.position = new Vector3(0, 45, -45);
            Camera.main.transform.rotation = Quaternion.Euler(50, 0, 0);
        }

        GameObject lightObj = new GameObject("Luz");
        lightObj.transform.SetParent(container.transform);
        Light l = lightObj.AddComponent<Light>(); l.type = LightType.Directional;
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

        // 2. Suelo
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Suelo"; floor.transform.SetParent(container.transform);
        floor.transform.localScale = new Vector3(10, 1, 10);
        floor.GetComponent<Renderer>().material.color = new Color(0.2f, 0.2f, 0.2f);

        // 3. INDICADOR DE COCINA
        GameObject kitchenArea = new GameObject("Zona_Cocina");
        kitchenArea.transform.SetParent(container.transform);
        GameObject kFloor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        kFloor.name = "Zona_Cocina_Suelo"; kFloor.transform.SetParent(kitchenArea.transform);
        kFloor.transform.position = new Vector3(8, 0.01f, 25); kFloor.transform.localScale = new Vector3(2.5f, 1, 2.5f);
        kFloor.GetComponent<Renderer>().material.color = new Color(0.4f, 0.4f, 0.4f, 0.5f);

        // 4. PAREDES
        CreateWall("Pared_Fondo", new Vector3(0, 4, 48), new Vector3(64, 8, 1), container.transform);
        CreateWall("Pared_Izq", new Vector3(-32, 4, 2.5f), new Vector3(1, 8, 92), container.transform);
        CreateWall("Pared_Der", new Vector3(32, 4, 2.5f), new Vector3(1, 8, 92), container.transform);
        CreateWall("Pared_Frontal", new Vector3(-18, 4, -43), new Vector3(28, 8, 1), container.transform);
        CreateWall("Pared_Frontal2", new Vector3(18, 4, -43), new Vector3(28, 8, 1), container.transform);

        // 5. COCINEROS (Controlables)
        Vector3[] chefPositions = { new Vector3(8, 1.1f, 15), new Vector3(4, 1.1f, 15), new Vector3(12, 1.1f, 15) };
        for (int i = 0; i < chefPositions.Length; i++)
        {
            GameObject chef = CreatePerson("Cocinero_" + i, chefPositions[i], Color.white, container.transform);
            chef.AddComponent<PlayerController>();
            // Importante: No añadimos PlayerInput aquí si ya existe uno global, pero por si acaso:
            chef.AddComponent<PlayerInput>(); 
            
            // Gorro
            Transform head = chef.transform.Find("Visuals/Head");
            if (head) {
                GameObject hat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                hat.transform.SetParent(head); hat.transform.localPosition = new Vector3(0, 0.4f, 0);
                hat.transform.localScale = new Vector3(0.8f, 0.6f, 0.8f);
            }
        }

        // 6. ESTACIONES
        CreateStation("Tomate", new Vector3(-5, 1, 35), Color.red, typeof(DispenserStation), kitchenArea.transform, "Tomato");
        CreateStation("Lechuga", new Vector3(-1, 1, 35), Color.green, typeof(DispenserStation), kitchenArea.transform, "Lettuce");
        CreateStation("Pan", new Vector3(3, 1, 35), new Color(0.8f, 0.5f, 0.2f), typeof(DispenserStation), kitchenArea.transform, "Bread");
        CreateStation("Ventanilla", new Vector3(0, 1.5f, -5.5f), Color.blue, typeof(DeliveryStation), container.transform);
        CreateStation("Basura", new Vector3(25, 1, 5), Color.black, typeof(TrashStation), kitchenArea.transform);
        CreateStation("MesaCortar", new Vector3(13, 1, 35), Color.gray, typeof(CuttingStation), kitchenArea.transform);

        // 7. MESAS
        int tablesCount = 0;
        for (int x = -2; x <= 2; x += 2) {
            for (int z = -1; z <= 1; z++) {
                Vector3 tablePos = new Vector3(x * 12, 0.8f, z * 10 - 25);
                CreateTableWithChairs("Mesa_" + (tablesCount++), tablePos, container.transform);
            }
        }
    }

    // --- FUNCIONES DE AYUDA (Helpers) DEL RESTAURANTE ---
    
    void CreateWall(string n, Vector3 p, Vector3 s, Transform par) {
        GameObject w = GameObject.CreatePrimitive(PrimitiveType.Cube);
        w.name = n; w.transform.SetParent(par); w.transform.position = p; w.transform.localScale = s;
    }

    void CreateStation(string n, Vector3 p, Color c, System.Type t, Transform par, string ing = "") {
        GameObject s = GameObject.CreatePrimitive(PrimitiveType.Cube);
        s.name = n; s.transform.SetParent(par); s.transform.position = p; s.transform.localScale = new Vector3(2, 1.2f, 2);
        s.GetComponent<Renderer>().material.color = c;
        s.AddComponent(t);
        if (t == typeof(DispenserStation)) {
            DispenserStation ds = s.GetComponent<DispenserStation>();
            ds.ingredient = ScriptableObject.CreateInstance<IngredientSO>();
            ds.ingredient.ingredientName = ing; ds.ingredient.canBeCut = true;
        }
    }

    void CreateTableWithChairs(string n, Vector3 p, Transform par) {
        GameObject t = GameObject.CreatePrimitive(PrimitiveType.Cube);
        t.name = n; t.transform.SetParent(par); t.transform.position = p; t.transform.localScale = new Vector3(5, 0.2f, 5);
        t.GetComponent<Renderer>().material.color = new Color(0.3f, 0.1f, 0.05f);
        // Sillas simples
        for(int i=-1;i<=1;i+=2) {
            GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.transform.SetParent(par); c.transform.position = p + new Vector3(i*3,0,0); c.transform.localScale = Vector3.one;
        }
    }

    GameObject CreatePerson(string n, Vector3 p, Color c, Transform par) {
        GameObject per = new GameObject(n); per.transform.SetParent(par); per.transform.position = p;
        per.AddComponent<CapsuleCollider>().height = 2;
        per.AddComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
        
        GameObject vis = new GameObject("Visuals"); vis.transform.SetParent(per.transform); vis.transform.localPosition = Vector3.zero;
        vis.AddComponent<CharacterAnimator>();
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.transform.SetParent(vis.transform); body.transform.localPosition = new Vector3(0,1,0); 
        body.transform.localScale = new Vector3(0.5f, 1, 0.5f); body.GetComponent<Renderer>().material.color = c;
        
        return per;
    }
}
