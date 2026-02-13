using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using System.Collections;
using System.Collections.Generic;

// ESTE SCRIPT HACE TODO: CARGA, MENU Y JUEGO.
// [ExecuteAlways] - Removido para evitar ejecución automática no deseada en el editor
public class KitchenBootstrap : MonoBehaviour
{
    public static KitchenBootstrap Instance;

    private GameObject menuCanvas;
    private GameObject currentMenuPanel;

    [Header("CONFIGURACIÓN DE DISEÑO")]
    public bool autoGenerateInEditor = false;
    
    [Header("PREFABS (Opcional - Reemplaza los cubos)")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject stovePrefab;
    public GameObject sinkPrefab;
    public GameObject prepTablePrefab;
    public GameObject trashPrefab;
    public GameObject deliveryPrefab;

    [Header("REFERENCIAS DINÁMICAS")]
    public GameObject restauranteContainer;

    private void DestroySafe(GameObject obj)
    {
        if (obj == null) return;
        if (Application.isPlaying) Destroy(obj);
        else DestroyImmediate(obj);
    }

    [ContextMenu("🧹 Limpiar Escena")]
    public void LimpiarCocina()
    {
        // Limpiar contenedores conocidos
        string[] containers = { "COCINA_2D", "RESTAURANTE_GENERADO_AUTOMATICAMENTE", "CanvasMenu", "--- GAME_UI ---" };
        foreach(var name in containers)
        {
            GameObject container = GameObject.Find(name);
            if (container == null) continue;

            // SEGURIDAD: Si este script está dentro del contenedor que vamos a borrar, lo sacamos primero
            if (transform.IsChildOf(container.transform))
            {
                transform.SetParent(null);
            }
            
            DestroySafe(container);
        }
        
        Debug.Log(">>> ESCENA LIMPIA (Estructuras y UI retiradas)");
    }

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
            if (Application.isPlaying) DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this) {
            if (Application.isPlaying)
            {
                Destroy(gameObject);
                return;
            }
        }

        // Eliminado auto-generate forzado para respetar control del usuario

    }

    private void OnEnable()
    {
        // Eliminado auto-generate en OnEnable para evitar procesos en segundo plano indeseados

    }

    void BuscarOReemplazarCocina()
    {
        if (restauranteContainer != null) return;
        
        restauranteContainer = GameObject.Find("COCINA_2D");
        // Solo generar si realmente no hay nada y el auto-generate está activo
        if (restauranteContainer == null && autoGenerateInEditor) {
            // Evitar recursividad si se llama durante la carga
            if (GameObject.Find("COCINA_2D") != null) return;
            GenerarCocina2D();
        }
    }

    [ContextMenu("🔗 Buscar Estaciones en Escena")]
    public void ConfigurarDesdeEscena()
    {
        Debug.Log(">>> ESCANEANDO ESCENA PARA VINCULAR LÓGICA MANUAL...");
        // Esta función busca en la jerarquía objetos que ya tengan componentes de estación
        // y se asegura de que estemos listos para jugar con ellos.
        if (restauranteContainer == null) restauranteContainer = GameObject.Find("COCINA_2D");
        
        var stations = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach(var s in stations) {
            if (s is CuttingStation || s is DispenserStation || s is TrashStation) {
                Debug.Log($"Viculada estación encontrada: {s.gameObject.name}");
            }
        }
    }

    [ContextMenu("✨ GENERAR COCINA (Base Architecture)")]
    public void GenerarArquitecturaBase()
    {
        GenerarCocina2D_Internal(true);
    }
    
    [ContextMenu("🚀 GENERAR TODO (Full Professional)")]
    public void GenerarTodoFull()
    {
        GenerarCocina2D_Internal(false);
    }

    // --- AUTO-EJECT (Asegura que el script corra sí o sí) ---
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoStartGlobal()
    {
        if (Object.FindFirstObjectByType<KitchenBootstrap>() == null)
        {
            GameObject obj = new GameObject("--- GAME MANAGER (AUTO) ---");
            obj.AddComponent<KitchenBootstrap>();
            DontDestroyOnLoad(obj);
        }
    }

    // --- VARIABLES DE JUEGO (Añadidas para corregir errores) ---
    private GameObject gamePanel;
    private int roundsSurvived;
    private int totalScore;
    private int consecutiveMediums;
    private List<Scenario> scenarios;

    // --- NUEVO SISTEMA DE CLIENTES AUTO ---
    public CustomerNPC currentCustomerServed;
    private List<GameObject> activeCustomers = new List<GameObject>();
    private float nextSpawnTime;
    private const float SPAWN_INTERVAL = 8f; // Segundos entre clientes
    private bool isGameActive = false;

    private void Start()
    {
        // Solo ejecutar lógica de juego si estamos dando al Play
        if (Application.isPlaying)
        {
            // 1. Camara
            if (Camera.main == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                Camera cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
                camObj.transform.position = new Vector3(0, 1, -10);
                cam.backgroundColor = Color.black; 
            }

            // 2. EventSystem
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject ev = new GameObject("EventSystem");
                ev.AddComponent<EventSystem>();
                ev.AddComponent<InputSystemUIInputModule>();
            }

            // 3. Inicializar escenarios
            InitializeScenarios();

            // 4. Empezar con la Pantalla de Carga
            StartCoroutine(SecuenciaInicio());
        }
    }

    // --- INICIALIZACIÓN DE ESCENARIOS ---
    void InitializeScenarios()
    {
        scenarios = new List<Scenario>();

        // Caso 1: ACIDEZ (Refinado)
        scenarios.Add(new Scenario {
            clientName = "Crítico Gastronómico con Reflujo",
            conditionDescription = "Esta semana he abusado de las catas de vino y cítricos. Mi esófago es un infierno. Necesito algo elegante pero que calme el fuego.",
            // Correcto 100: Alcalino y suave
            optionA_Text = "Velouté de Calabaza con Pollo Asado a Baja Temperatura", 
            optionA_Image = "Food/PumpkinSoup", 
            optionA_Score = 100,
            // Normal 50: Pan (ok pero seco)
            optionB_Text = "Tostadas de Masa Madre con Aceite de Oliva", 
            optionB_Image = "Food/ToastLuxury", 
            optionB_Score = 50,
            // Mal 0: Ácido/Grasa/Picante
            optionC_Text = "Ceviche Peruano con Leche de Tigre Picante", 
            optionC_Image = "Food/Ceviche", 
            optionC_Score = 0
        });

        // Caso 2: MIGRAÑAS (Refinado)
        scenarios.Add(new Scenario {
            clientName = "Empresaria con Cefalea",
            conditionDescription = "Llevo horas frente a la pantalla y la luz me mata. Siento que me va a estallar la cabeza. Evita cualquier cosa 'añeja' o excitante.",
            // Correcto 100: Rico en Magnesio/B2
            optionA_Text = "Salmón Noruego al Vapor con Espárragos Trigueros y Quinoa", 
            optionA_Image = "Food/SalmonAsparagus", 
            optionA_Score = 100,
            // Normal 50: Neutro
            optionB_Text = "Puré de Patatas Trufado", 
            optionB_Image = "Food/MashedPotatoes", 
            optionB_Score = 50,
            // Mal 0: Tiramina (Queso/Vino)
            optionC_Text = "Tabla de Quesos Azules y Tinto Reserva", 
            optionC_Image = "Food/BlueCheese", 
            optionC_Score = 0
        });

        // Caso 3: ATLETA (Refinado)
        scenarios.Add(new Scenario {
            clientName = "Ciclista de Élite",
            conditionDescription = "Mañana tengo la etapa reina del Tour. Mis depósitos de glucógeno están vacíos y necesito recuperación muscular inmediata.",
            // Correcto 100: Carbohidrato complejo + Proteina magra
            optionA_Text = "Pasta Fresca al Pesto con Pechuga de Pavo y Nueces", 
            optionA_Image = "Food/PestoPasta", 
            optionA_Score = 100,
            // Normal 50: Solo Hidratación
            optionB_Text = "Batido Isotónico de Recuperación Premium", 
            optionB_Image = "Food/IsotonicDrink", 
            optionB_Score = 50,
            // Mal 0: Grasa pesada (digestión lenta)
            optionC_Text = "Pizza 4 Quesos con Borde Relleno", 
            optionC_Image = "Food/Pizza4Cheese", 
            optionC_Score = 0
        });

        // Caso 4: CELIAQUÍA (Nuevo)
        scenarios.Add(new Scenario {
            clientName = "Joven Celíaca",
            conditionDescription = "Tengo intolerancia severa al gluten. Incluso una traza mínima me manda al hospital. Sorpréndeme, pero no me mates.",
            // Correcto 100: Gluten Free natural
            optionA_Text = "Risotto de Setas Silvestres con Parmesano Reggiano", 
            optionA_Image = "Food/MushroomRisotto", 
            optionA_Score = 100,
            // Normal 50: Sin gluten pero arriesgado (Contaminacion cruzada en fritos)
            optionB_Text = "Patatas Fritas en Aceite Común", 
            optionB_Image = "Food/Fries", 
            optionB_Score = 50,
            // Mal 0: Trigo directo
            optionC_Text = "Raviolis Artesanos de Trigo Duro", 
            optionC_Image = "Food/Ravioli", 
            optionC_Score = 0
        });

        // Caso 5: DIABÉTICO (Nuevo)
        scenarios.Add(new Scenario {
            clientName = "Señor con Diabetes Tipo 2",
            conditionDescription = "Mi médico me ha dicho que controle mis picos de insulina. Tengo mucha hambre, pero no quiero azúcares rápidos.",
            // Correcto 100: Bajo Índice Glucémico, Alta Fibra
            optionA_Text = "Lentejas Estofadas con Verduras de Temporada", 
            optionA_Image = "Food/LentilStew", 
            optionA_Score = 100,
            // Normal 50:
            optionB_Text = "Filete de Ternera a la Plancha (Sin guarnición)", 
            optionB_Image = "Food/Steak", 
            optionB_Score = 50,
            // Mal 0: Bomba de azúcar
            optionC_Text = "Pastel de Chocolate con Glaseado de Azúcar", 
            optionC_Image = "Food/ChocoCake", 
            optionC_Score = 0
        });

        // Caso 6: HIPERTENSIÓN (Nuevo)
        scenarios.Add(new Scenario {
            clientName = "Ejecutivo Hipertenso",
            conditionDescription = "Tengo la tensión por las nubes por el estrés. Necesito comer bien, pero el cardiólogo me ha prohibido la sal.",
            // Correcto 100: Dieta DASH, Especias en vez de sal
            optionA_Text = "Pollo al Limón con Romero y Ensalada de Tomate", 
            optionA_Image = "Food/LemonChicken", 
            optionA_Score = 100,
            // Normal 50: Poca sal pero procesado
            optionB_Text = "Sandwich de Pavo y Queso Light", 
            optionB_Image = "Food/Sandwich", 
            optionB_Score = 50,
            // Mal 0: Sodio puro
            optionC_Text = "Sopa de Sobre Instantánea y Embutidos", 
            optionC_Image = "Food/InstaSoup", 
            optionC_Score = 0
        });
    }

    // --- SECUENCIA DE INICIO PROFESIONAL ---
    IEnumerator SecuenciaInicio()
    {
        CrearCanvas();

        // 1. Fondo Elegante (Dark Charcoal)
        GameObject panelCarga = CrearPanel(new Color(0.1f, 0.1f, 0.12f));
        
        // 2. Título Flotante
        GameObject titulo = CrearTexto(panelCarga.transform, "CHEF SIMULATOR", 0, 100, 50, new Color(1f, 0.8f, 0.2f)); // Oro
        titulo.GetComponent<Text>().fontStyle = FontStyle.Bold;

        // 3. Barra de Carga Minimalista
        GameObject barraFondo = new GameObject("BarraFondo");
        barraFondo.transform.SetParent(panelCarga.transform, false);
        Image imgBarraFondo = barraFondo.AddComponent<Image>();
        imgBarraFondo.color = new Color(0.2f, 0.2f, 0.2f); // Gris oscuro
        RectTransform rtBarra = barraFondo.GetComponent<RectTransform>();
        rtBarra.sizeDelta = new Vector2(600, 6); // Muy fina y moderna
        rtBarra.anchoredPosition = new Vector2(0, -50);

        GameObject barraRelleno = new GameObject("BarraRelleno");
        barraRelleno.transform.SetParent(barraFondo.transform, false);
        Image imgRelleno = barraRelleno.AddComponent<Image>();
        imgRelleno.color = new Color(0.2f, 0.8f, 0.5f); // Verde esmeralda moderno
        RectTransform rtRelleno = barraRelleno.GetComponent<RectTransform>();
        rtRelleno.anchorMin = Vector2.zero; rtRelleno.anchorMax = Vector2.zero;
        rtRelleno.pivot = new Vector2(0, 0.5f);
        rtRelleno.sizeDelta = new Vector2(0, 6); 
        rtRelleno.anchoredPosition = Vector2.zero; 

        // 4. Texto dinámico de carga
        GameObject txtEstado = CrearTexto(panelCarga.transform, "Iniciando sistema...", 0, -80, 18, Color.gray);
        Text tEstado = txtEstado.GetComponent<Text>();
        tEstado.fontStyle = FontStyle.Italic;

        string[] tips = { 
            "Afilando los cuchillos...", 
            "Comprando ingredientes frescos...", 
            "Limpiando la estación de trabajo...", 
            "Precalentando los hornos...",
            "Revisando recetas..." 
        };

        float tiempoTotal = 4f;
        float tiempo = 0f;
        
        while(tiempo < tiempoTotal) 
        {
            tiempo += Time.deltaTime;
            float progreso = tiempo / tiempoTotal;
            
            // Animación Barra (Suave)
            rtRelleno.sizeDelta = new Vector2(600 * progreso, 6);

            // Animación Texto (Cambia cada 0.8s)
            int tipIndex = (int)((tiempo / tiempoTotal) * tips.Length);
            if(tipIndex < tips.Length) tEstado.text = tips[tipIndex];

            yield return null;
        }

        Destroy(panelCarga);
        MostrarMenuPrincipal();
    }

    void MostrarMenuPrincipal()
    {
        isGameActive = false; // Detener spawners al estar en el menú
        if (this.currentMenuPanel != null) Destroy(this.currentMenuPanel);

        // 1. Fondo Corporativo (Azul noche profundo o Gris oscuro)
        this.currentMenuPanel = CrearPanel(new Color(0.1f, 0.12f, 0.15f)); 
        
        // 2. HEADER - Título y Subtítulo
        GameObject headerContainer = new GameObject("Header");
        headerContainer.transform.SetParent(this.currentMenuPanel.transform, false);
        RectTransform rtHeader = headerContainer.AddComponent<RectTransform>();
        rtHeader.anchorMin = new Vector2(0.5f, 0.7f); rtHeader.anchorMax = new Vector2(0.5f, 0.9f);
        rtHeader.anchoredPosition = Vector2.zero; rtHeader.sizeDelta = new Vector2(800, 200);

        // Título Central
        GameObject titleT = CrearTexto(headerContainer.transform, "CHEF PROFESSIONAL", 0, 40, 70, new Color(1f, 0.85f, 0.3f)); // Dorado suave
        titleT.GetComponent<Text>().fontStyle = FontStyle.Bold;

        // Subtítulo
        CrearTexto(headerContainer.transform, "GESTIÓN GASTRONÓMICA AVANZADA", 0, -30, 20, new Color(0.7f, 0.75f, 0.8f));

        // 3. CENTER - Botones
        GameObject buttonContainer = new GameObject("ButtonContainer");
        buttonContainer.transform.SetParent(this.currentMenuPanel.transform, false);
        RectTransform rtContainer = buttonContainer.AddComponent<RectTransform>();
        // Anclar al centro de la pantalla
        rtContainer.anchorMin = new Vector2(0.5f, 0.5f); 
        rtContainer.anchorMax = new Vector2(0.5f, 0.5f);
        rtContainer.anchoredPosition = new Vector2(0, -50); 
        rtContainer.sizeDelta = new Vector2(500, 400);

        // --- OPCIÓN 1: INICIO DE JUEGO ---
        CrearBotonModerno(buttonContainer.transform, "INICIO DE JUEGO", 0, 100, new Color(0.2f, 0.6f, 0.3f), () => {
            EntrarAJuego();
        });

        // --- OPCIÓN 2: MANUAL DE INSTRUCCIONES ---
        CrearBotonModerno(buttonContainer.transform, "MANUAL DE INSTRUCCIONES", 0, 0, new Color(0.2f, 0.4f, 0.6f), MostrarInstrucciones);

        // --- OPCIÓN 3: CREAR USUARIO ---
        CrearBotonModerno(buttonContainer.transform, "CREAR USUARIO", 0, -100, new Color(0.5f, 0.3f, 0.6f), MostrarCrearUsuario);
        
        // Footer
        GameObject footer = CrearTexto(this.currentMenuPanel.transform, "© 2026 Enterprise Solutions", 0, -300, 12, Color.gray);
        RectTransform rtFooter = footer.GetComponent<RectTransform>();
        rtFooter.anchorMin = new Vector2(0.5f, 0.05f); rtFooter.anchorMax = new Vector2(0.5f, 0.05f);
        rtFooter.anchoredPosition = Vector2.zero;
    }

    void EntrarAJuego()
    {
        // Destruir el menú principal antes de entrar al juego
        if (this.menuCanvas != null) DestroySafe(this.menuCanvas);

        GameObject container = GameObject.Find("COCINA_2D");
    if (container == null) container = GameObject.Find("RESTAURANTE_ESTRUCTURA");

    if (container == null)
    {
        Debug.LogError("‼️ ERROR: No hay cocina ni restaurante generado. Por favor, genera uno desde el menú Contextual de KitchenBootstrap antes de entrar.");
        return; 
    }
        
        restauranteContainer = container;
        SetupCameraRealist(new Vector3(0, 18, -16), 42);
        
        // ASEGURAR JUGADOR Y ESTADO
        if (Object.FindFirstObjectByType<PlayerController>() == null)
        {
            SpawnPlayerTopDown(new Vector3(0, 0.1f, -6.0f));
        }
        
        isGameActive = true;
        nextSpawnTime = Time.time + 2f;
        Debug.Log(">>> EMPEZANDO JUEGO (Estado configurado)");
    }

    void MostrarInstrucciones()
    {
        if (this.currentMenuPanel != null) Destroy(this.currentMenuPanel);
        this.currentMenuPanel = CrearPanel(new Color(0.15f, 0.2f, 0.25f)); // Azul acero

        CrearTexto(this.currentMenuPanel.transform, "MANUAL OPERATIVO", 0, 200, 50, Color.white);

        string manual = 
            "- PROTOCOLO DE ACIDEZ: Servir alimentos alcalinos (Pollo, Avena).\n\n" +
            "- PROTOCOLO DE MIGRAÑA: Evitar tiramina. Priorizar Espinacas/Magnesio.\n\n" +
            "- PROTOCOLO DEPORTIVO: Requerimiento de Potasio y Carbohidratos (Plátano, Pasta).\n\n" +
            "OBJETIVO: Maximizar satisfacción del cliente (100 pts) para evitar sanciones.";

        GameObject txt = CrearTexto(this.currentMenuPanel.transform, manual, 0, 0, 24, new Color(0.9f, 0.9f, 0.9f));
        txt.GetComponent<RectTransform>().sizeDelta = new Vector2(900, 400); // Dar espacio para leer

        CrearBotonModerno(this.currentMenuPanel.transform, "VOLVER AL MENÚ", 0, -200, new Color(0.6f, 0.2f, 0.2f), MostrarMenuPrincipal);
    }

    void MostrarCrearUsuario()
    {
        if (this.currentMenuPanel != null) Destroy(this.currentMenuPanel);
        this.currentMenuPanel = CrearPanel(new Color(0.2f, 0.15f, 0.25f)); // Púrpura oscuro corporativo

        // Header
        GameObject header = CrearTexto(this.currentMenuPanel.transform, "REGISTRO DE PERSONAL", 0, 150, 50, Color.white);
        header.GetComponent<Text>().fontStyle = FontStyle.Bold;
        
        CrearTexto(this.currentMenuPanel.transform, "Identificación del Chef Ejecutivo", 0, 100, 20, new Color(0.8f, 0.8f, 0.8f));

        // -- INPUT FIELD CONTAINER --
        GameObject inputObj = new GameObject("InputField_Name");
        inputObj.transform.SetParent(this.currentMenuPanel.transform, false);
        
        // Background Image for Input
        Image img = inputObj.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.1f); // Blanco semitransparente moderno
        
        // RECT TRANSFORM (Ya existe por el Image, lo recuperamos)
        RectTransform rtInput = inputObj.GetComponent<RectTransform>();
        rtInput.sizeDelta = new Vector2(500, 60);
        rtInput.anchoredPosition = new Vector2(0, 20);

        // InputField Component (Funcional)
        InputField inputField = inputObj.AddComponent<InputField>();
        
        // 1. Child Text Component (Lo que escribe el usuario)
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(inputObj.transform, false);
        Text textComp = textObj.AddComponent<Text>();
        textComp.font = GetMyFont();
        textComp.fontSize = 28;
        textComp.color = Color.white;
        textComp.alignment = TextAnchor.MiddleCenter;
        RectTransform rtText = textObj.GetComponent<RectTransform>();
        rtText.anchorMin = Vector2.zero; rtText.anchorMax = Vector2.one;
        rtText.sizeDelta = new Vector2(-20, 0); // Padding lateral
        rtText.offsetMin = new Vector2(10, 0); rtText.offsetMax = new Vector2(-10, 0);
        
        // 2. Child Placeholder Component (Texto fantasma)
        GameObject placeHolderObj = new GameObject("Placeholder");
        placeHolderObj.transform.SetParent(inputObj.transform, false);
        Text placeComp = placeHolderObj.AddComponent<Text>();
        placeComp.text = "Escriba su nombre...";
        placeComp.font = GetMyFont();
        placeComp.fontSize = 28;
        placeComp.fontStyle = FontStyle.Italic;
        placeComp.color = new Color(1f, 1f, 1f, 0.5f);
        placeComp.alignment = TextAnchor.MiddleCenter;
        RectTransform rtPlace = placeHolderObj.GetComponent<RectTransform>();
        rtPlace.anchorMin = Vector2.zero; rtPlace.anchorMax = Vector2.one;
        rtPlace.sizeDelta = new Vector2(-20, 0);
        rtPlace.offsetMin = new Vector2(10, 0); rtPlace.offsetMax = new Vector2(-10, 0);

        // Conectar todo al InputField
        inputField.textComponent = textComp;
        inputField.placeholder = placeComp;
        inputField.image = img;
        
        // Buttons
        CrearBotonModerno(currentMenuPanel.transform, "CONFIRMAR REGISTRO", 0, -80, new Color(0.2f, 0.6f, 0.3f), () => {
             Debug.Log($"Usuario registrado: {inputField.text}");
             // Aquí podrías guardar el nombre en una variable estática si quisieras
             MostrarMenuPrincipal(); 
        });

        CrearBotonModerno(currentMenuPanel.transform, "ATRAS", 0, -160, new Color(0.6f, 0.2f, 0.2f), MostrarMenuPrincipal);
    }
    
    private void Update()
    {
        // Lógica de aparición de clientes automática
        if (isGameActive && activeCustomers.Count < 5)
        {
            if (Time.time >= nextSpawnTime)
            {
                SpawnAutomaticCustomer();
                nextSpawnTime = Time.time + SPAWN_INTERVAL;
            }
        }
    }

    void SpawnAutomaticCustomer()
    {
        // Posiciones de los taburetes (donde se sientan los clientes)
        float[] xPositions = { -5f, -2.5f, 0f, 2.5f, 5f };
        
        // Buscar un hueco libre
        foreach (float x in xPositions)
        {
            Vector3 pos = new Vector3(x, 0.1f, -8.0f);
            bool occupied = false;
            foreach (GameObject c in activeCustomers)
            {
                if (c != null && Vector3.Distance(c.transform.position, pos) < 1f)
                {
                    occupied = true;
                    break;
                }
            }

            if (!occupied)
            {
                SpawnCustomerNPC(pos);
                Debug.Log($"Nuevo cliente aparecido en X: {x}");
                break; 
            }
        }
    }

    public void OnCustomerLeft(GameObject customer)
    {
        if (activeCustomers.Contains(customer))
        {
            activeCustomers.Remove(customer);
        }
    }

    void CrearBotonModerno(Transform parent, string texto, float x, float y, Color colorBase, UnityEngine.Events.UnityAction accion)
    {
        GameObject b = new GameObject($"Btn_{texto}");
        b.transform.SetParent(parent, false);
        
        // Sombra del botón
        GameObject shadow = new GameObject("Shadow");
        shadow.transform.SetParent(b.transform, false);
        Image imgS = shadow.AddComponent<Image>();
        imgS.color = new Color(0,0,0,0.4f);
        RectTransform rs = shadow.GetComponent<RectTransform>();
        rs.anchorMin = Vector2.zero; rs.anchorMax = Vector2.one;
        rs.offsetMin = new Vector2(4, -4); rs.offsetMax = new Vector2(4, -4);

        // Fondo Botón
        Image img = b.AddComponent<Image>(); 
        img.color = colorBase;
        
        Button btn = b.AddComponent<Button>(); 
        btn.onClick.AddListener(accion);
        // Transición de color
        ColorBlock cb = btn.colors;
        cb.highlightedColor = Color.Lerp(colorBase, Color.white, 0.2f);
        cb.pressedColor = Color.Lerp(colorBase, Color.black, 0.2f);
        btn.colors = cb;
        
        RectTransform r = b.GetComponent<RectTransform>();
        r.sizeDelta = new Vector2(450, 70);
        r.anchoredPosition = new Vector2(x, y);

        // Texto Boton Estilizado
        GameObject t = CrearTexto(b.transform, texto, 0, 0, 24, Color.white, true);
        t.GetComponent<Text>().fontStyle = FontStyle.Bold;
        t.GetComponent<RectTransform>().offsetMin = new Vector2(0,0);
        t.GetComponent<RectTransform>().offsetMax = new Vector2(0,0);
        
        // CrearTexto pone anchor al centro por defecto si stretch=true, ajustamos
        Text txtComp = t.GetComponent<Text>();
        txtComp.alignment = TextAnchor.MiddleCenter;
    }

    void CrearBotonMenu(Transform parent, string texto, float x, float y, Color colorFondo, Vector2 size, UnityEngine.Events.UnityAction accion)
    {
        GameObject b = new GameObject($"Btn_{texto}");
        b.transform.SetParent(parent, false);
        
        Image img = b.AddComponent<Image>(); 
        img.color = colorFondo;
        
        Button btn = b.AddComponent<Button>(); 
        btn.onClick.AddListener(accion);
        
        RectTransform r = b.GetComponent<RectTransform>();
        r.sizeDelta = size;
        r.anchoredPosition = new Vector2(x, y);

        // Texto Boton
        GameObject t = CrearTexto(b.transform, texto, 0, 0, (int)(size.y * 0.45f), Color.white, true);
        t.GetComponent<Text>().fontStyle = FontStyle.Bold;
        t.GetComponent<RectTransform>().offsetMin = new Vector2(10,0);
        t.GetComponent<RectTransform>().offsetMax = new Vector2(-10,0);
    }

    // ==============================================================================================
    // GENERACIÓN DE COCINA "MASTERCHEF" (PROFESIONAL)
    // ==============================================================================================

    // ==============================================================================================
    // GENERACIÓN DE COCINA "ULTRA REALISTA" (DETALLE NIVEL MAESTRO)
    // ==============================================================================================

    public void GenerarCocina2D()
    {
        GenerarRestaurante(); // Redirect to the most modern version autonomously
    }

    private void GenerarCocina2D_Internal(bool soloArquitectura)
    {
        GenerarRestaurante(); // Redirect to the most modern version autonomously
    }

    void GenerarIslaWaterfall(Vector3 pos, float w, float d)
    {
        CreateBlock("IslandBase", pos + Vector3.up * 0.5f, new Vector3(w - 0.4f, 1.0f, d - 0.2f), new Color(0.15f, 0.12f, 0.1f));
        Color marble = new Color(0.98f, 0.98f, 1f);
        CreateBlock("WaterTop", pos + Vector3.up * 1.15f, new Vector3(w + 0.1f, 0.1f, d + 0.1f), marble);
        CreateBlock("WaterLeft", pos + new Vector3(-w/2, 0.6f, 0), new Vector3(0.1f, 1.2f, d + 0.1f), marble);
        CreateBlock("WaterRight", pos + new Vector3(w/2, 0.6f, 0), new Vector3(0.1f, 1.2f, d + 0.1f), marble);
    }

    void GenerarMuroListones(Vector3 pos, float depth, float height)
    {
        // Fondo oscuro
        CreateBlock("WallBase", pos, new Vector3(0.2f, height, depth), new Color(0.1f, 0.1f, 0.1f));
        // Listones de madera
        float spacing = 0.4f;
        for(float z = -depth/2 + 0.2f; z < depth/2; z += spacing) {
            CreateBlock("Liston", pos + new Vector3(0.15f, 0, z), new Vector3(0.1f, height, 0.15f), new Color(0.45f, 0.3f, 0.15f));
        }
    }



    // --- UI INTERACTION PROMPT ---
    private GameObject interactionButton;

    public void ToggleInteractionPrompt(bool show, UnityEngine.Events.UnityAction action = null)
    {
        if (interactionButton == null)
        {
            // Crear el botón si no existe (Arriba a la Izquierda)
            CrearCanvas(); // Asegurar canvas
            interactionButton = new GameObject("Btn_TalkCustomer");
            interactionButton.transform.SetParent(this.menuCanvas.transform, false);
            
            Image img = interactionButton.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.8f); // Fondo negro semitransparente

            Button btn = interactionButton.AddComponent<Button>();
            
            // Texto
            GameObject t = CrearTexto(interactionButton.transform, "HABLAR CON CLIENTE (E)", 0, 0, 24, Color.white, true);
            t.GetComponent<Text>().fontStyle = FontStyle.Bold;

            RectTransform rt = interactionButton.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1); 
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(20, -20); // Margen superior izquierdo
            rt.sizeDelta = new Vector2(300, 60);
        }

        interactionButton.SetActive(show);
        
        if (show && action != null)
        {
            Button btn = interactionButton.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action);
        }
    }

    void SpawnCustomerNPC(Vector3 pos)
    {
        GameObject npc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        npc.name = "CustomerNPC";
        npc.transform.SetParent(restauranteContainer.transform);
        npc.transform.position = pos;
        activeCustomers.Add(npc);
        npc.GetComponent<Renderer>().material.color = Color.yellow; // Distinto al player
        
        // Visual: Sombrero o detalle
        GameObject detail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        detail.transform.SetParent(npc.transform);
        detail.transform.localPosition = new Vector3(0, 0.6f, 0.4f);
        detail.transform.localScale = new Vector3(0.6f, 0.2f, 0.2f); // Gafas?
        detail.GetComponent<Renderer>().material.color = Color.black;

        // Añadir lógica de interacción
        npc.AddComponent<CustomerNPC>();

        // Texto flotante "CLIENTE"
        // CrearTextoInWorld(npc.transform, "CLIENTE", new Vector3(0, 1.2f, 0));
    }

    // --- GENERADORES DE ALTO DETALLE ---

    void GenerarMueblesBajosLujo(Vector3 pos, float totalW)
    {
        // Cuerpo madera
        GameObject baseMob = CreateBlock("MueblesBajos", pos + Vector3.up * 0.55f, new Vector3(totalW, 1.1f, 1.2f), new Color(0.2f, 0.15f, 0.1f));
        // Encimera Mármol
        CreateBlock("EncimeraMarmol", pos + Vector3.up * 1.15f, new Vector3(totalW + 0.2f, 0.1f, 1.4f), new Color(0.98f, 0.98f, 1f));
        
        // Puertas y Tiradores
        for(float x = -totalW/2 + 0.5f; x < totalW/2; x+=1f) {
            // Puerta
            CreateBlock("Puerta", pos + new Vector3(x, 0.6f, 0.61f), new Vector3(0.9f, 1.0f, 0.05f), new Color(0.25f, 0.18f, 0.12f)).transform.SetParent(baseMob.transform);
            // Tirador (Cilindro plata horizontal)
            GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.transform.SetParent(baseMob.transform);
            handle.transform.position = pos + new Vector3(x, 1.0f, 0.65f);
            handle.transform.rotation = Quaternion.Euler(0,0,90);
            handle.transform.localScale = new Vector3(0.015f, 0.15f, 0.015f);
            handle.GetComponent<Renderer>().material.color = Color.white;
        }
    }

    void GenerarNeveraDobleInox(Vector3 pos)
    {
        GameObject body = CreateBlock("Frigorifico", pos + Vector3.up * 1.5f, new Vector3(2.5f, 3f, 1.5f), new Color(0.75f, 0.75f, 0.8f));
        // Puertas (Doble puerta vertical)
        CreateBlock("P_Izquierda", pos + new Vector3(-0.6f, 1.5f, 0.76f), new Vector3(1.2f, 2.8f, 0.05f), new Color(0.8f, 0.8f, 0.85f)).transform.SetParent(body.transform);
        CreateBlock("P_Derecha", pos + new Vector3(0.6f, 1.5f, 0.76f), new Vector3(1.2f, 2.8f, 0.05f), new Color(0.8f, 0.8f, 0.85f)).transform.SetParent(body.transform);
        // Dispensador de agua
        CreateBlock("Dispensador", pos + new Vector3(-0.6f, 1.8f, 0.8f), new Vector3(0.4f, 0.6f, 0.02f), Color.black).transform.SetParent(body.transform);
    }

    void GenerarFregaderoRealista(Vector3 pos)
    {
        // Pila Inox encastrada
        CreateBlock("PilaInox", pos, new Vector3(1.8f, 0.05f, 1.2f), new Color(0.5f, 0.5f, 0.55f));
        // Grifo (Cisne)
        GameObject neck = new GameObject("GrifoCisne");
        neck.transform.SetParent(restauranteContainer.transform);
        neck.transform.position = pos + Vector3.forward * 0.5f;
        // Base grifo
        CreateBlock("Base", pos + Vector3.forward * 0.5f, new Vector3(0.2f, 0.2f, 0.2f), Color.white).transform.SetParent(neck.transform);
        // Cuello curvo (Multi-piezas)
        for(int i=0; i<10; i++) {
            float angle = i * 18 * Mathf.Deg2Rad;
            GameObject p = CreateBlock("Seg", pos + new Vector3(0, 0.5f + Mathf.Sin(angle)*0.5f, 0.5f - (float)i/15f), new Vector3(0.05f, 0.05f, 0.05f), Color.white);
            p.transform.SetParent(neck.transform);
        }
    }

    void GenerarPlacaInduccionRealista(Vector3 pos)
    {
        GameObject plate = CreateBlock("Induccion", pos, new Vector3(2f, 0.02f, 1.8f), Color.black);
        // Fuegos (Aros de cristal)
        for(int i=0; i<4; i++) {
             GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
             ring.transform.SetParent(plate.transform);
             ring.transform.localPosition = new Vector3((i%2 == 0 ? -0.4f : 0.4f), 1f, (i < 2 ? -0.4f : 0.4f));
             ring.transform.localScale = new Vector3(0.35f, 0.1f, 0.35f);
             ring.GetComponent<Renderer>().material.color = new Color(0.2f, 0, 0); // Rojo apagado
        }
    }

    void GenerarCazuelaDetail(Vector3 pos, string name)
    {
        GameObject pot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pot.name = name;
        pot.transform.SetParent(restauranteContainer.transform);
        pot.transform.position = pos;
        pot.transform.localScale = new Vector3(0.8f, 0.3f, 0.8f);
        pot.GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.55f); // Inox
        // Asas
        CreateBlock("AsaL", pos + Vector3.left * 0.45f, new Vector3(0.2f, 0.05f, 0.05f), Color.black).transform.SetParent(pot.transform);
        CreateBlock("AsaR", pos + Vector3.right * 0.45f, new Vector3(0.2f, 0.05f, 0.05f), Color.black).transform.SetParent(pot.transform);
    }

    void GenerarSartenDetail(Vector3 pos, string name)
    {
        GameObject pan = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pan.name = name;
        pan.transform.SetParent(restauranteContainer.transform);
        pan.transform.position = pos + Vector3.up * 1.25f;
        pan.transform.localScale = new Vector3(0.7f, 0.05f, 0.7f);
        pan.GetComponent<Renderer>().material.color = new Color(0.15f, 0.15f, 0.15f); // Teflon
        // Mango largo
        CreateBlock("Mango", pan.transform.position + Vector3.back * 0.7f, new Vector3(0.08f, 0.08f, 1f), Color.black).transform.SetParent(pan.transform);
    }

    void GenerarIluminacionCenital()
    {
        GameObject sun = new GameObject("SunLight");
        sun.transform.SetParent(restauranteContainer.transform);
        Light l = sun.AddComponent<Light>(); 
        l.type = LightType.Directional; 
        l.intensity = 1.2f;
        l.color = new Color(1f, 0.98f, 0.95f);
        l.shadows = LightShadows.Soft;
        sun.transform.rotation = Quaternion.Euler(50, -30, 0);
        
        // Luz de relleno azulada
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(restauranteContainer.transform);
        Light lf = fill.AddComponent<Light>(); 
        lf.type = LightType.Directional; 
        lf.intensity = 0.3f;
        lf.color = new Color(0.8f, 0.9f, 1f);
        fill.transform.rotation = Quaternion.Euler(-50, 150, 0);
    }

    void CrearSueloParqueRealista(int w, int d)
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "ParquetReal";
        floor.transform.SetParent(restauranteContainer.transform);
        floor.transform.localScale = new Vector3(w/10f, 1, d/10f);
        floor.GetComponent<Renderer>().material.color = new Color(0.45f, 0.3f, 0.15f); // Roble oscuro
    }

    void SetupCameraRealist(Vector3 pos, float fov)
    {
        GameObject camObj;
        if (Camera.main != null) camObj = Camera.main.gameObject;
        else { camObj = new GameObject("Main Camera"); camObj.AddComponent<Camera>(); camObj.tag = "MainCamera"; }
        
        camObj.transform.SetParent(restauranteContainer.transform);
        camObj.transform.position = pos;
        camObj.transform.rotation = Quaternion.Euler(55, 0, 0); 
        
        Camera cam = camObj.GetComponent<Camera>();
        cam.orthographic = false;
        cam.fieldOfView = fov;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.12f);
    }

    void GenerarLavavajillasPanelado(Vector3 pos)
    {
        // Encastrado como un mueble pero con botonera
        GenerarMueblesBajosLujo(pos, 1);
        CreateBlock("PanelControl", pos + Vector3.up * 1.05f + Vector3.forward * 0.62f, new Vector3(0.8f, 0.15f, 0.02f), Color.black);
        // Pequeños LEDs
        CreateBlock("LED", pos + Vector3.up * 1.1f + Vector3.forward * 0.63f + Vector3.left * 0.2f, new Vector3(0.05f, 0.05f, 0.02f), Color.green);
    }

    void GenerarIslaLujo(Vector3 pos, float w, float d)
    {
        GameObject island = CreateBlock("IslaLujo", pos + Vector3.up * 0.55f, new Vector3(w, 1.1f, d), new Color(0.2f, 0.15f, 0.1f));
        CreateBlock("EncimeraIsla", pos + Vector3.up * 1.15f, new Vector3(w + 0.2f, 0.1f, d + 0.2f), new Color(0.98f, 0.98f, 1f));
    }

    void GenerarTablaCorteDetalle(Vector3 pos)
    {
        CreateBlock("TablaCorte", pos, new Vector3(1.5f, 0.1f, 1f), new Color(0.8f, 0.65f, 0.45f));
    }

    void GenerarCuchilloRealista(Vector3 pos)
    {
        GameObject knife = CreateBlock("Hoja", pos, new Vector3(0.1f, 0.01f, 0.8f), Color.white);
        CreateBlock("Mango", pos + Vector3.back * 0.5f, new Vector3(0.12f, 0.05f, 0.3f), Color.black).transform.SetParent(knife.transform);
    }

    void CrearMuroRealista(Vector3 pos, Vector3 s, Color c)
    {
        CreateBlock("Muro", pos, s, c);
    }

    // --- HELPERS 2D ---

    void SetupCamera2D(float size)
    {
        GameObject camObj;
        if (Camera.main != null) camObj = Camera.main.gameObject;
        else { camObj = new GameObject("Main Camera"); camObj.AddComponent<Camera>(); camObj.tag = "MainCamera"; }
        
        camObj.transform.SetParent(restauranteContainer.transform);
        camObj.transform.position = new Vector3(0, 20, 0);
        camObj.transform.rotation = Quaternion.Euler(90, 0, 0); // Cenital total
        
        Camera cam = camObj.GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = size;
        cam.backgroundColor = Color.black;
    }

    void CrearSuelo2D(float w, float d, Color c)
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Quad);
        floor.name = "Suelo2D";
        floor.transform.SetParent(restauranteContainer.transform);
        floor.transform.rotation = Quaternion.Euler(90, 0, 0);
        floor.transform.localScale = new Vector3(w, d, 1);
        floor.GetComponent<Renderer>().material.color = c;
    }

    void GenerarEstacion2D(Vector3 pos, Vector2 size, string tag, Color c)
    {
        GameObject station = GameObject.CreatePrimitive(PrimitiveType.Quad);
        station.name = "Station_" + tag;
        station.transform.SetParent(restauranteContainer.transform);
        station.transform.position = pos;
        station.transform.rotation = Quaternion.Euler(90, 0, 0);
        station.transform.localScale = new Vector3(size.x, size.y, 1);
        station.GetComponent<Renderer>().material.color = c;
        
        // Borde sutil
        GameObject border = GameObject.CreatePrimitive(PrimitiveType.Quad);
        border.transform.SetParent(station.transform);
        border.transform.localPosition = new Vector3(0,0,0.01f);
        border.transform.localScale = new Vector3(1.05f, 1.05f, 1);
        border.GetComponent<Renderer>().material.color = Color.black;
    }

    void GenerarIsla2D(Vector3 pos, Vector2 size, Color c)
    {
        GenerarEstacion2D(pos, size, "ISLAND", c);
    }

    void GenerarPila2D(Vector3 pos)
    {
        CrearCirculo2D(pos, 2f, new Color(0.4f, 0.45f, 0.5f)); // Pila circular
    }

    void CrearCirculo2D(Vector3 pos, float scale, Color c)
    {
        GameObject circle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        circle.transform.SetParent(restauranteContainer.transform);
        circle.transform.position = pos;
        circle.transform.localScale = new Vector3(scale, 0.01f, scale);
        circle.GetComponent<Renderer>().material.color = c;
    }

    void SpawnPlayer2D(Vector3 pos)
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        player.name = "Chef2D";
        player.transform.SetParent(restauranteContainer.transform);
        player.transform.position = pos;
        player.transform.localScale = new Vector3(1.2f, 0.1f, 1.2f); // Flat circle
        player.GetComponent<Renderer>().material.color = Color.white;

        // "Nariz" o dirección (puntero)
        GameObject dir = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dir.transform.SetParent(player.transform);
        dir.transform.localPosition = new Vector3(0, 0.5f, 0.6f);
        dir.transform.localScale = new Vector3(0.4f, 2f, 0.4f);
        dir.GetComponent<Renderer>().material.color = Color.black;

        PlayerController pc = player.AddComponent<PlayerController>();
        pc.isFirstPerson = false;
        pc.moveSpeed = 10f;
        
        PlayerInput pi = player.AddComponent<PlayerInput>();
        pi.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // --- GENERADORES INDUSTRIALES ---

    void GenerarFogonIndustrial(Vector3 pos)
    {
        Color inox = new Color(0.8f, 0.8f, 0.85f);
        GameObject body = CreateBlock("FogonPro", pos, new Vector3(4f, 1.2f, 2.5f), inox);
        // Fuegos circulares potentes
        for(int i=-1; i<=1; i++) {
            GameObject burner = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            burner.transform.SetParent(body.transform);
            burner.transform.localPosition = new Vector3(i*1.2f, 0.61f, 0);
            burner.transform.localScale = new Vector3(0.8f, 0.05f, 0.8f);
            burner.GetComponent<Renderer>().material.color = Color.black;
            // Centro rojo incandescente
            CreateBlock("Core", burner.transform.position + Vector3.up*0.02f, new Vector3(0.4f, 0.01f, 0.4f), Color.red).transform.SetParent(burner.transform);
        }
    }

    void GenerarCampanaGigante(Vector3 pos)
    {
        CreateBlock("Hood", pos, new Vector3(4.5f, 1f, 2.5f), new Color(0.7f, 0.7f, 0.75f));
        CreateBlock("Vent", pos + Vector3.up * 1f, new Vector3(1f, 2f, 1f), new Color(0.7f, 0.7f, 0.75f));
    }

    void GenerarNeveraReachIn(Vector3 pos)
    {
        GameObject fridge = CreateBlock("FridgePro", pos + Vector3.up * 1.2f, new Vector3(2.5f, 3.5f, 2.5f), new Color(0.85f, 0.85f, 0.9f));
        // Puerta de cristal con marco inox
        CreateBlock("Glass", pos + Vector3.up * 1.2f + Vector3.right * 1.26f, new Vector3(0.05f, 3.2f, 2.2f), new Color(0.7f, 0.9f, 1f, 0.4f));
        CreateBlock("Frame", pos + Vector3.up * 1.2f + Vector3.right * 1.27f, new Vector3(0.02f, 3.4f, 2.4f), Color.black);
    }

    void GenerarIslaPrepIndustrial(Vector3 pos, float length, bool isWashStation)
    {
        Color inox = new Color(0.85f, 0.85f, 0.85f);
        GameObject island = CreateBlock("IslaPro", pos, new Vector3(3f, 1.2f, length), inox);
        // Encimera super pulida
        CreateBlock("Top", pos + Vector3.up * 0.62f, new Vector3(3.2f, 0.1f, length + 0.2f), new Color(0.95f, 0.95f, 1f));

        if(isWashStation) {
            // Fregaderos integrados en la isla
            GenerarPilaEnIsla(pos + Vector3.forward * (length/3f));
            GenerarPilaEnIsla(pos - Vector3.forward * (length/3f));
        } else {
            // Tablas de corte blancas profesionales
            for(int i=-1; i<=1; i++)
               GenerarTablaCortePro(pos + Vector3.forward * i * (length/4f));
        }
    }

    void GenerarPilaEnIsla(Vector3 pos) {
        CreateBlock("Sink", pos + Vector3.up * 0.65f, new Vector3(2f, 0.1f, 1.5f), Color.gray);
        // Grifo extensible industrial
        GameObject tap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tap.transform.position = pos + Vector3.up * 1.5f + Vector3.right * 1f;
        tap.transform.localScale = new Vector3(0.1f, 0.8f, 0.1f);
        tap.GetComponent<Renderer>().material.color = Color.white;
        tap.transform.SetParent(restauranteContainer.transform);
    }

    void GenerarTablaCortePro(Vector3 pos) {
        CreateBlock("Board", pos + Vector3.up * 0.72f, new Vector3(2.2f, 0.05f, 1.5f), Color.white);
    }

    void GenerarLavavajillasTunel(Vector3 pos) {
        // Máquina grande rectangular con entrada y salida
        CreateBlock("DishWasher", pos + Vector3.up * 0.75f, new Vector3(2.5f, 2.5f, 5f), new Color(0.8f, 0.8f, 0.82f));
        CreateBlock("ExitTray", pos + Vector3.forward * 3.5f, new Vector3(2.5f, 0.1f, 2f), Color.gray);
    }

    void GenerarFregaderoTriplePro(Vector3 pos) {
        GameObject body = CreateBlock("TripleSink", pos, new Vector3(2.5f, 1.2f, 4f), new Color(0.7f, 0.7f, 0.75f));
        for(int i=-1; i<=1; i++)
            CreateBlock("Pila", pos + Vector3.up * 0.61f + Vector3.forward * i * 1.2f, new Vector3(2.2f, 1f, 1f), Color.gray);
    }

    void GenerarRackOllas(Vector3 pos) {
        // Barrotes negros con formas circulares (ollas)
        GameObject rack = CreateBlock("Rack", pos, new Vector3(2f, 0.1f, 8f), Color.black);
        for(int i=0; i<5; i++) {
            GameObject pot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pot.transform.SetParent(rack.transform);
            pot.transform.localPosition = new Vector3(0, -1.5f, -0.4f + i*0.2f);
            pot.transform.rotation = Quaternion.Euler(0,0,90);
            pot.transform.localScale = new Vector3(0.5f, 0.2f, 0.5f);
            pot.GetComponent<Renderer>().material.color = new Color(0.5f, 0.3f, 0.2f); // Cobre
        }
    }

    void GenerarEstanteriaIngredientesPro(Vector3 pos) {
        GameObject shelf = CreateBlock("ChefShelf", pos + Vector3.up * 1f, new Vector3(2f, 3f, 4f), new Color(0.2f, 0.2f, 0.2f));
        // Botes de ingredientes
        for(int i=0; i<10; i++) {
            GameObject jar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            jar.transform.SetParent(shelf.transform);
            jar.transform.localPosition = new Vector3(0, (i%3)*0.3f - 0.4f, (i/3f)*0.3f - 0.4f);
            jar.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            jar.GetComponent<Renderer>().material.color = Random.ColorHSV();
        }
    }

    // --- HELPERS ---
    void SetupCameraPro(Vector3 pos, float fov) {
        GameObject camObj;
        if (Camera.main != null) camObj = Camera.main.gameObject;
        else { camObj = new GameObject("Main Camera"); camObj.AddComponent<Camera>(); camObj.tag = "MainCamera"; }
        
        camObj.transform.SetParent(restauranteContainer.transform);
        camObj.transform.position = pos;
        camObj.transform.rotation = Quaternion.Euler(50, 0, 0); 
        
        Camera cam = camObj.GetComponent<Camera>();
        cam.orthographic = false; // Perspectiva para apreciar la escala
        cam.fieldOfView = fov;
        cam.backgroundColor = new Color(0.05f, 0.05f, 0.07f); 
    }

    void CrearSueloIndustrial(int w, int d) {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "SueloPro";
        floor.transform.SetParent(restauranteContainer.transform);
        floor.transform.localScale = new Vector3(w/10f, 1, d/10f);
        floor.GetComponent<Renderer>().material.color = new Color(0.25f, 0.25f, 0.28f); // Gris oscuro epoxi
    }

    void GenerarMuebleMint(Vector3 pos, float width, Color bodyC, Color topC, bool rotated = false)
    {
        Vector3 size = rotated ? new Vector3(1.2f, 1.2f, width) : new Vector3(width, 1.2f, 1.2f);
        GameObject body = CreateBlock("MuebleMint", pos, size, bodyC);
        Vector3 topSize = rotated ? new Vector3(1.3f, 0.1f, width) : new Vector3(width, 0.1f, 1.3f);
        CreateBlock("Encimera", pos + Vector3.up * 0.65f, topSize, topC);
    }

    // --- GENERADORES ESPECÍFICOS REPLICA ---

    void GenerarVentanalPanoramico(Vector3 pos)
    {
        // Marco minimalista Negro
        CreateBlock("MarcoSup", pos + Vector3.up * 1.5f, new Vector3(10f, 0.2f, 0.2f), Color.black);
        // Cristal único grande con transparencia real
        CreateGlassWindow("Cristal", pos, new Vector3(9.8f, 2.8f, 0.05f), restauranteContainer.transform);
    }

    void GenerarEncimeraBicolor(Vector3 pos, float width)
    {
        // 1. Cuerpo Mueble: NEGRO MATE
        GameObject body = CreateBlock("Mueble", pos, new Vector3(2.5f, 1.2f, width), new Color(0.12f, 0.12f, 0.12f));
        
        // 2. Encimera: BLANCO MÁRMOL (Contraste clave)
        GameObject top = CreateBlock("Top", pos + Vector3.up * 0.62f, new Vector3(2.6f, 0.08f, width + 0.1f), new Color(0.95f, 0.95f, 0.95f));
        
        // 3. Zócalo (Patas ocultas): Negro más profundo
        CreateBlock("Zocalo", pos + Vector3.down * 0.55f + Vector3.right * 0.2f, new Vector3(2.0f, 0.1f, width), Color.black).transform.SetParent(body.transform);
    }

    void GenerarFregaderoBicolor(Vector3 pos)
    {
        GenerarEncimeraBicolor(pos, 3);
        // Pila bajo encimera (Metalica oscura)
        CreateBlock("Pila", pos + Vector3.up * 0.63f, new Vector3(1.8f, 0.05f, 1.5f), new Color(0.3f, 0.3f, 0.3f));
        
        // Grifo Negro Mate moderno (Cuello curvo)
        GenerarFregaderoRealista(pos + new Vector3(0, 0.65f, 0.8f));
    }

    void GenerarPlacaInduccion(Vector3 pos)
    {
        // Placa negra muy fina sobre la encimera blanca
        GameObject plate = CreateBlock("Induccion", pos + Vector3.up * 0.67f, new Vector3(1.8f, 0.02f, 1.8f), new Color(0.05f, 0.05f, 0.05f));
        // Detalles rojos ui
        CreateBlock("UI", pos + Vector3.up * 0.68f + Vector3.right * 0.5f, new Vector3(0.1f, 0f, 0.5f), Color.red).transform.SetParent(plate.transform);
        
        // Station logic
        // (Aqui iría el componente StoveStation si existiera la lógica completa, por ahora visual)
    }

    void GenerarTorreHornos(Vector3 pos)
    {
        // Mueble columna negro
        GameObject col = CreateBlock("Columna", pos + Vector3.up * 1f, new Vector3(2.5f, 3.2f, 2.5f), new Color(0.12f, 0.12f, 0.12f));
        // Horno Inox Empotrado
        GameObject oven = CreateBlock("Horno", pos + Vector3.up * 0.5f, new Vector3(2.51f, 1.2f, 2f), new Color(0.2f, 0.2f, 0.2f));
        // Cristal Horno
        CreateBlock("Cristal", pos + Vector3.up * 0.5f + Vector3.right * 0.1f, new Vector3(2.52f, 0.8f, 1.6f), Color.black);
    }

    void GenerarNeveraAmericana(Vector3 pos)
    {
        // Cuerpo Inox/Negro
        CreateBlock("Nevera", pos + Vector3.up * 1.2f, new Vector3(2.4f, 3.5f, 2.5f), new Color(0.2f, 0.2f, 0.25f));
        // Puertas
        CreateBlock("PuertaL", pos + Vector3.up * 1.2f + Vector3.forward * 0.6f + Vector3.left * 0.1f, new Vector3(2.5f, 3.4f, 1.2f), new Color(0.15f, 0.15f, 0.2f));
        CreateBlock("PuertaR", pos + Vector3.up * 1.2f - Vector3.forward * 0.6f + Vector3.left * 0.1f, new Vector3(2.5f, 3.4f, 1.2f), new Color(0.15f, 0.15f, 0.2f));
    }

    void GenerarTorreGris(Vector3 pos, Color c)
    {
        // Bloque Alto
        GameObject t = CreateBlock("Torre", pos, new Vector3(2f, 3.5f, 2f), c);
        // Lineas separación puertas nevera
        CreateBlock("Sep", pos + Vector3.up * 0.2f, new Vector3(2.05f, 0.05f, 2.05f), Color.black);
        // Horno microondas integrado
        CreateBlock("Micro", pos + Vector3.up * 1f + Vector3.left * 0.5f, new Vector3(0.8f, 0.6f, 2.05f), Color.black);
    }

    void GenerarEstanteMadera(Vector3 pos, float w, Color c)
    {
        CreateBlock("Estante", pos, new Vector3(w, 0.1f, 0.8f), c);
    }
    
    void DecorarEstantes(Vector3 pos)
    {
        // Platos blancos
        for(int i=0; i<3; i++) {
            GameObject p = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            p.transform.position = pos + Vector3.up * 0.2f + Vector3.right * (i*0.5f - 0.5f);
            p.transform.rotation = Quaternion.Euler(90, 0, 0);
            p.transform.localScale = new Vector3(0.4f, 0.05f, 0.4f);
            p.GetComponent<Renderer>().material.color = Color.white;
            p.transform.SetParent(restauranteContainer.transform);
        }
    }

    void GenerarMesaComedorMadera(Vector3 pos, Color c)
    {
        // Tablero grande
        GameObject t = CreateBlock("Mesa", pos, new Vector3(4f, 0.1f, 3f), c);
        t.transform.position = pos + Vector3.up * 0.05f; // Altura mesa (mas baja que encimera 0.75m vs 0.9m)
        
        // Pata panel lateral (estilo moderno solido)
        CreateBlock("PataL", pos + Vector3.left * 1.9f + Vector3.down * 0.4f, new Vector3(0.1f, 0.8f, 3f), c);
        CreateBlock("PataR", pos + Vector3.right * 1.9f + Vector3.down * 0.4f, new Vector3(0.1f, 0.8f, 3f), c);
    }
    
    void GenerarSillaDiseno(Vector3 pos, float angle, Color c)
    {
        // Asiento curvo (simulado con esfera achatarada y cortada o cubo smootheado)
        GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Capsule); // Hack shape
        seat.transform.position = pos + Vector3.up * -0.2f;
        seat.transform.localScale = new Vector3(0.6f, 0.5f, 0.6f);
        seat.GetComponent<Renderer>().material.color = c;
        seat.transform.SetParent(restauranteContainer.transform);
        
        // Patas
        CreateBlock("Pata1", pos + new Vector3(0.2f, -0.6f, 0.2f), new Vector3(0.05f, 0.6f, 0.05f), new Color(0.6f, 0.4f, 0.2f));
        CreateBlock("Pata2", pos + new Vector3(-0.2f, -0.6f, -0.2f), new Vector3(0.05f, 0.6f, 0.05f), new Color(0.6f, 0.4f, 0.2f));
    }

    void GenerarPlatosMesa(Vector3 pos)
    {
        // Individuales y platos
        for(int x=-1; x<=1; x+=2) {
             // Mantelito
             CreateBlock("Mantel", pos + new Vector3(x*0.8f, 0, 0), new Vector3(0.6f, 0.01f, 0.4f), Color.white);
             // Plato
             GameObject p = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
             p.transform.position = pos + new Vector3(x*0.8f, 0.02f, 0);
             p.transform.localScale = new Vector3(0.3f, 0.02f, 0.3f);
             p.GetComponent<Renderer>().material.color = new Color(0.9f, 0.9f, 0.9f);
             p.transform.SetParent(restauranteContainer.transform);
        }
        // Frutero centro
        GenerarBowlFrutas(pos);
    }
    

    void GenerarPlantaGrande(Vector3 pos)
    {
        GameObject pot = CreateBlock("MacetaPlanta", pos, new Vector3(0.8f, 1f, 0.8f), Color.white);
        GameObject leaves = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leaves.transform.SetParent(pot.transform);
        leaves.transform.localPosition = new Vector3(0, 1f, 0);
        leaves.transform.localScale = new Vector3(1.2f, 1.5f, 1.2f);
        leaves.GetComponent<Renderer>().material.color = new Color(0.1f, 0.5f, 0.2f);
    }


    void GenerarCocinaConHorno(Vector3 pos)
    {
        // Placa Negra arriba
        CreateBlock("Placa", pos + Vector3.up * 0.71f, new Vector3(1.8f, 0.05f, 1f), Color.black);
        // Frente Horno Negro
        CreateBlock("HornoFrente", pos + Vector3.up * 0.35f + Vector3.forward * 0.61f, new Vector3(1.8f, 0.7f, 0.1f), Color.black);
        // Maneta plata
        CreateBlock("Maneta", pos + Vector3.up * 0.65f + Vector3.forward * 0.66f, new Vector3(1.4f, 0.05f, 0.05f), Color.white);
    }

    void GenerarFregaderoInox(Vector3 pos)
    {
        // Pila Inox
        CreateBlock("Pila", pos + Vector3.up * 0.65f, new Vector3(1.5f, 0.05f, 1f), new Color(0.6f, 0.6f, 0.6f));
        // Grifo
        GameObject tap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tap.transform.SetParent(restauranteContainer.transform);
        tap.transform.position = pos + Vector3.up * 1f + Vector3.forward * 0.4f;
        tap.transform.localScale = new Vector3(0.05f, 0.6f, 0.05f);
        tap.GetComponent<Renderer>().material.color = Color.white;
    }


    // --- HELPERS ---
    void SetupCameraIsometric()
    {
        GameObject camObj;
        if (Camera.main != null) camObj = Camera.main.gameObject;
        else { camObj = new GameObject("Main Camera"); camObj.AddComponent<Camera>(); camObj.tag = "MainCamera"; }
        
        camObj.transform.SetParent(restauranteContainer.transform);
        // Vista ISO
        camObj.transform.position = new Vector3(0, 20, -15); 
        camObj.transform.rotation = Quaternion.Euler(55, 0, 0); 
        
        Camera cam = camObj.GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 9f; 
        cam.backgroundColor = new Color(0.15f, 0.15f, 0.18f); 
    }

    void CrearSueloMaderaClara(int w, int d)
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "SueloMadera";
        floor.transform.SetParent(restauranteContainer.transform);
        floor.transform.localScale = new Vector3(w/10f, 1, d/10f);
        floor.GetComponent<Renderer>().material.color = new Color(0.85f, 0.75f, 0.65f); // Beige madera
    }
    
    void CrearMuroColor(Vector3 pos, Vector3 s, Color c)
    {
        CreateBlock("Muro", pos, s, c);
    }

    void CrearMuroPro(Vector3 pos, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.SetParent(restauranteContainer.transform);
        wall.transform.position = pos;
        wall.transform.localScale = scale;
        wall.GetComponent<Renderer>().material.color = new Color(0.2f, 0.2f, 0.25f); // Azul oscuro corporativo
    }

    void CreatePointLight(Vector3 pos, Color c, float range, float intensity)
    {
        GameObject l = new GameObject("PointLight");
        l.transform.SetParent(restauranteContainer.transform);
        l.transform.position = pos;
        Light light = l.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = c;
        light.range = range;
        light.intensity = intensity;
    }

    void CrearSueloBaldosas(int width, int depth)
    {
        GameObject floor = new GameObject("Suelo");
        floor.transform.SetParent(restauranteContainer.transform);
        
        GameObject baseFloor = GameObject.CreatePrimitive(PrimitiveType.Quad);
        baseFloor.transform.SetParent(floor.transform);
        baseFloor.transform.rotation = Quaternion.Euler(90, 0, 0);
        baseFloor.transform.localScale = new Vector3(width, depth, 1);
        baseFloor.GetComponent<Renderer>().material.color = new Color(0.4f, 0.35f, 0.3f); // Madera oscura rica

        // Patrón de parqué (Visual fake con líneas más sutiles)
        // No creamos miles de cubos para evitar lag, solo el plano base coloreado funciona bien en top-down distante
    }

    // --- GENERADORES MASTERCHEF ---

    void GenerarNevera(Vector3 pos)
    {
        // Nevera estilo industrial (Acero inoxidable alto)
        GameObject obj = CreateBlock("NeveraPro", pos + Vector3.up * 0.5f, new Vector3(2.5f, 2.5f, 2), new Color(0.75f, 0.75f, 0.8f));
        // Línea de separación puertas
        CreateBlock("Sep", pos + Vector3.up * 0.5f, new Vector3(0.05f, 2.4f, 2.1f), new Color(0.6f, 0.6f, 0.65f)).transform.SetParent(obj.transform);
        // Panel digital temperatura
        CreateBlock("Panel", pos + new Vector3(0.6f, 1.2f, -1.01f), new Vector3(0.5f, 0.2f, 0.05f), Color.blue).transform.SetParent(obj.transform);
        
        // CrearTextoInWorld(obj.transform, "CAMARA", new Vector3(0, 1.5f, 0));
    }

    void GenerarEncimeraInox(Vector3 pos)
    {
        // Bloque principal Inox (Gris azulado brillante)
        GameObject obj = CreateBlock("EncimeraInox", pos, new Vector3(2.5f, 1.2f, 2), new Color(0.75f, 0.75f, 0.8f));
        // Sin etiqueta flotante molesta, solo diseño limpio
    }

    void GenerarFregaderoIndustrial(Vector3 pos)
    {
        GameObject obj = CreateBlock("FregaderoInd", pos, new Vector3(2.5f, 1.2f, 2), new Color(0.6f, 0.6f, 0.65f)); 
        // Gran pila profunda
        GameObject sink = CreateBlock("PilaGrande", pos + new Vector3(0, 0.61f, 0), new Vector3(2.2f, 0.2f, 1.6f), new Color(0.4f, 0.4f, 0.45f));
        sink.transform.SetParent(obj.transform);
        // Grifo alto industrial (arco)
        CreateArco(obj.transform, pos + new Vector3(0, 1.2f, 0.9f), 0.8f, 0.4f, Color.gray);
    }

    void GenerarCocinaPro(Vector3 pos)
    {
        // Cuerpo negro mate industrial
        GameObject obj = CreateBlock("CocinaInd", pos, new Vector3(2.5f, 1.2f, 2), new Color(0.1f, 0.1f, 0.1f));
        
        // 6 Fuegos (Rejilla de hierro fundido)
        GameObject grill = CreateBlock("Rejilla", pos + new Vector3(0, 0.61f, 0), new Vector3(2.3f, 0.05f, 1.8f), new Color(0.2f, 0.2f, 0.2f));
        grill.transform.SetParent(obj.transform);
        
        // Llamas/Quemadores
        for(int x=-1; x<=1; x++) {
            for(int z=-1; z<=1; z+=2) {
                GameObject burner = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                burner.transform.SetParent(obj.transform);
                burner.transform.position = pos + new Vector3(x*0.7f, 0.65f, z*0.5f);
                burner.transform.localScale = new Vector3(0.4f, 0.02f, 0.4f);
                burner.GetComponent<Renderer>().material.color = new Color(0.8f, 0.3f, 0.1f);
            }
        }
    }

    void GenerarHornoPro(Vector3 pos)
    {
        GameObject obj = CreateBlock("HornoConv", pos, new Vector3(2.5f, 1.5f, 2), new Color(0.3f, 0.3f, 0.35f)); 
        // Ventana digital
        CreateBlock("Panel", pos + new Vector3(0, 0.5f, -1.01f), new Vector3(2.0f, 0.4f, 0.05f), Color.black).transform.SetParent(obj.transform);
    }

    void GenerarEstanteriaIngredientes(Vector3 pos, string ingredient, Color c)
    {
        // Estructura metálica (Wire rack)
        GameObject rack = CreateBlock("Estanteria_" + ingredient, pos, new Vector3(1.5f, 0.1f, 1.5f), new Color(0.6f, 0.6f, 0.6f));
        // Patas
        CreateBlock("Leg1", pos+new Vector3(-0.7f, -1, -0.7f), new Vector3(0.1f, 2, 0.1f), Color.gray).transform.SetParent(rack.transform);
        CreateBlock("Leg2", pos+new Vector3(0.7f, -1, -0.7f), new Vector3(0.1f, 2, 0.1f), Color.gray).transform.SetParent(rack.transform);

        // Caja de Ingrediente encima
        GameObject box = CreateBlock("Caja", pos + Vector3.up * 0.3f, new Vector3(1.2f, 0.6f, 1.2f), new Color(0.5f, 0.4f, 0.3f));
        box.transform.SetParent(rack.transform);
        
        // Tapa abierta mostrando color
        CreateBlock("Muestra", pos + Vector3.up * 0.61f, new Vector3(1f, 0.05f, 1f), c).transform.SetParent(box.transform);
        
        // Etiqueta profesional (Texto world space pequeño y limpio)
        // CrearTextoInWorld(rack.transform, ingredient.ToUpper(), new Vector3(0, 1.5f, 0));

        DispenserStation ds = rack.AddComponent<DispenserStation>();
        ds.ingredientName = ingredient;
    }

    void GenerarTablaCortarPro(Vector3 pos)
    {
        GameObject table = CreateBlock("MesaInox", pos, new Vector3(2.5f, 1.2f, 2), new Color(0.75f, 0.75f, 0.8f));
        
        // Tabla de madera gruesa (Butcher block)
        GameObject board = CreateBlock("ButcherBlock", pos + Vector3.up * 0.65f, new Vector3(1.0f, 0.15f, 0.8f), new Color(0.6f, 0.4f, 0.2f));
        board.transform.SetParent(table.transform);

        // Cuchillo decorativo
        GameObject knife = GameObject.CreatePrimitive(PrimitiveType.Cube);
        knife.transform.SetParent(board.transform);
        knife.transform.localPosition = new Vector3(0.3f, 0.1f, 0); 
        knife.transform.localScale = new Vector3(0.1f, 0.02f, 0.6f);
        knife.GetComponent<Renderer>().material.color = Color.white; // Hoja

        table.AddComponent<CuttingStation>();
    }

    void GenerarBasuraPro(Vector3 pos)
    {
        GameObject bin = CreateBlock("BasuraInox", pos, new Vector3(1.4f, 1.4f, 1.4f), new Color(0.4f, 0.4f, 0.4f));
        // Tapa oscilante
        CreateBlock("Tapa", pos + Vector3.up * 0.71f, new Vector3(1.3f, 0.05f, 1.3f), new Color(0.3f, 0.3f, 0.3f)).transform.SetParent(bin.transform);
        
        // CrearTextoInWorld(bin.transform, "BASURA", new Vector3(0, 1.2f, 0));
        bin.AddComponent<TrashStation>();
    }

    void GenerarLavavajillasPro(Vector3 pos)
    {
        GameObject obj = CreateBlock("LavavajillasInd", pos, new Vector3(2.5f, 1.2f, 2), new Color(0.9f, 0.9f, 0.9f));
        // Panel LED azul
        CreateBlock("LED", pos + new Vector3(0.8f, 0.4f, -1.01f), new Vector3(0.4f, 0.1f, 0.05f), Color.cyan).transform.SetParent(obj.transform);
    }

    void GenerarIslaCentral(Vector3 pos)
    {
        CreateBlock("Isla", pos, new Vector3(6f, 1.2f, 3f), new Color(0.85f, 0.85f, 0.9f)); // Superficie de trabajo blanca amplia
    }

    // --- WRAPPERS PARA SOPORTE DE PADRES ESPECÍFICOS ---
    void GenerarEncimeraBicolor_WithParent(Vector3 pos, float width, Transform p) { restauranteContainer = p.gameObject; GenerarEncimeraBicolor(pos, width); }
    void GenerarPlacaInduccion_WithParent(Vector3 pos, Transform p) { restauranteContainer = p.gameObject; GenerarPlacaInduccion(pos); }
    void GenerarFregaderoRealista_WithParent(Vector3 pos, Transform p) { restauranteContainer = p.gameObject; GenerarFregaderoRealista(pos); }
    void GenerarIslaWaterfall_WithParent(Vector3 pos, float w, float d, Transform p) { restauranteContainer = p.gameObject; GenerarIslaWaterfall(pos, w, d); }
    void GenerarTorreHornos_WithParent(Vector3 pos, Transform p) { restauranteContainer = p.gameObject; GenerarTorreHornos(pos); }
    void GenerarNeveraAmericana_WithParent(Vector3 pos, Transform p) { restauranteContainer = p.gameObject; GenerarNeveraAmericana(pos); }
    void GenerarSillaDiseno_WithParent(Vector3 pos, float angle, Color c, Transform p) { restauranteContainer = p.gameObject; GenerarSillaDiseno(pos, angle, c); }
    void GenerarPlantaGrande_WithParent(Vector3 pos, Transform p) { restauranteContainer = p.gameObject; GenerarPlantaGrande(pos); }
    void GenerarVentanalPanoramico(Vector3 pos, Transform p) { restauranteContainer = p.gameObject; GenerarVentanalPanoramico(pos); }
    void GenerarMuroListones(Vector3 pos, float depth, float height, Transform p) { restauranteContainer = p.gameObject; GenerarMuroListones(pos, depth, height); }
    void CrearMuroRealista(Vector3 pos, Vector3 scale, Color c, Transform p) { restauranteContainer = p.gameObject; CrearMuroRealista(pos, scale, c); }
    void CreatePointLight(Vector3 pos, Color c, float range, float intensity, Transform p) { restauranteContainer = p.gameObject; CreatePointLight(pos, c, range, intensity); }

    // Utileria Visual
    void CreateArco(Transform parent, Vector3 center, float height, float width, Color c)
    {
        // Simple representación visual - Barra Horizontal sobre dos postes (representando el grifo ind)
        GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
        top.transform.SetParent(parent);
        top.transform.position = center + Vector3.up * height;
        top.transform.localScale = new Vector3(width, 0.1f, 0.1f);
        top.GetComponent<Renderer>().material.color = c;

        GameObject left = GameObject.CreatePrimitive(PrimitiveType.Cube);
        left.transform.SetParent(parent);
        left.transform.position = center + new Vector3(-width/2, height/2, 0);
        left.transform.localScale = new Vector3(0.1f, height, 0.1f);
        left.GetComponent<Renderer>().material.color = c;
    }

    // --- UTILS ---
    GameObject CreateBlock(string name, Vector3 pos, Vector3 scale, Color c, Transform forcedParent = null)
    {
        GameObject obj;
        // Si tenemos un prefab asignado para el 'Suelo' o 'Pared', lo usamos.
        // Por ahora simplificado para usar los Prefabs de las cabeceras si coinciden nombres
        GameObject prefabToUse = null;
        if (name.Contains("Suelo") && floorPrefab != null) prefabToUse = floorPrefab;
        if (name.Contains("Muro") && wallPrefab != null) prefabToUse = wallPrefab;

        if (prefabToUse != null) {
            obj = Instantiate(prefabToUse, pos, Quaternion.identity);
            obj.name = name;
        } else {
            obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name;
            obj.GetComponent<Renderer>().material.color = c;
        }

        Transform targetParent = (forcedParent != null) ? forcedParent : (restauranteContainer ? restauranteContainer.transform : null);
        obj.transform.SetParent(targetParent);
        obj.transform.position = pos;
        obj.transform.localScale = scale;
        
        return obj;
    }

    void CrearMuro(Vector3 pos, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.SetParent(restauranteContainer.transform);
        wall.transform.position = pos;
        wall.transform.localScale = scale;
        wall.GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.6f);
    }

    void SpawnPlayer(Vector3 pos)
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Chef";
        player.transform.SetParent(restauranteContainer.transform);
        player.transform.position = pos;
        player.GetComponent<Renderer>().material.color = Color.white;

        // Sombrero de Chef (Visual)
        GameObject hat = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        hat.transform.SetParent(player.transform);
        hat.transform.localPosition = new Vector3(0, 1f, 0);
        hat.transform.localScale = new Vector3(0.6f, 0.4f, 0.6f);
        hat.GetComponent<Renderer>().material.color = Color.white;
        Destroy(hat.GetComponent<Collider>()); // Solo visual

        // Lógica
        PlayerController pc = player.AddComponent<PlayerController>();
        pc.interactDistance = 2.5f;

        // Input System: Necesita 'PlayerInput' para enviar mensajes a 'OnMove'
        PlayerInput pi = player.AddComponent<PlayerInput>();
        pi.notificationBehavior = PlayerNotifications.InvokeCSharpEvents; 
    }

    void CrearTextoInWorld(Transform parent, string text, Vector3 localPos)
    {
        GameObject tObj = new GameObject("Label");
        tObj.transform.SetParent(parent);
        tObj.transform.localPosition = localPos;
        // Rotar para ver desde arriba
        tObj.transform.rotation = Quaternion.Euler(90, 0, 0); 
        tObj.transform.localScale = Vector3.one * 0.1f;

        TextMesh tm = tObj.AddComponent<TextMesh>();
        tm.text = text;
        tm.fontSize = 40;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = Color.white;
    }
    // ==============================================================================================
    // COCINA RECTANGULAR PROFESIONAL COMPLETA
    // ==============================================================================================

    [ContextMenu("🔥 COCINA PROFESIONAL COMPLETA")]
    public void GenerarCocinaRectangularProfesional()
    {
        Debug.Log("═══════════════════════════════════════════════════════");
        Debug.Log("🔥 GENERANDO COCINA PROFESIONAL DE ALTA GAMA 🔥");
        Debug.Log("═══════════════════════════════════════════════════════");

        // LIMPIEZA TOTAL
        string[] nombres = { "RESTAURANTE_ESTRUCTURA", "COCINA_2D", "Environment_Main", "COCINA_PROFESIONAL" };
        foreach(string n in nombres) {
            GameObject obj = GameObject.Find(n);
            if(obj != null) DestroySafe(obj);
        }
        
        GameObject root = new GameObject("COCINA_PROFESIONAL");
        restauranteContainer = root;

        // PALETA DE COLORES
        Color maderaRoble = new Color(0.28f, 0.18f, 0.10f);
        Color marmolCarrara = new Color(0.96f, 0.96f, 0.98f);
        Color aceroInox = new Color(0.72f, 0.72f, 0.76f);
        Color negroMate = new Color(0.10f, 0.10f, 0.12f);
        Color azulejoCeramic = new Color(0.98f, 0.98f, 1f);
        Color grisCemento = new Color(0.38f, 0.38f, 0.40f);

        // CÁMARA Y LUZ
        SetupCameraPro(new Vector3(0, 30, -40), 50);
        GameObject sol = new GameObject("Sol_Principal");
        sol.transform.SetParent(root.transform);
        Light luz = sol.AddComponent<Light>();
        luz.type = LightType.Directional;
        luz.intensity = 1.6f;
        luz.color = new Color(1f, 0.98f, 0.94f);
        luz.shadows = LightShadows.Soft;
        sol.transform.rotation = Quaternion.Euler(55, -25, 0);

        // ESTRUCTURA BASE (Suelo, Paredes, Techo)
        CreateBlock("Suelo", Vector3.down * 0.1f, new Vector3(90, 0.2f, 70), grisCemento, root.transform);
        CreateBlock("Pared_Fondo", new Vector3(0, 4.5f, 35), new Vector3(90, 9f, 0.4f), azulejoCeramic, root.transform);
        CreateBlock("Pared_Izq", new Vector3(-45, 4.5f, 0), new Vector3(0.4f, 9f, 70), azulejoCeramic, root.transform);
        CreateBlock("Pared_Der", new Vector3(45, 4.5f, 0), new Vector3(0.4f, 9f, 70), azulejoCeramic, root.transform);
        CreateBlock("Techo", new Vector3(0, 9f, 0), new Vector3(90, 0.3f, 70), Color.white, root.transform);

        // ZONA COCCIÓN (Izquierda)
        float xIzq = -28;
        float z = 22;
        CreateBlock("Mueble_Coccion", new Vector3(xIzq, 0.5f, z), new Vector3(10, 1f, 2.8f), maderaRoble, root.transform);
        CreateBlock("Encimera_Coccion", new Vector3(xIzq, 1.02f, z), new Vector3(10.3f, 0.1f, 3f), marmolCarrara, root.transform);
        
        // Placa inducción
        GameObject placa = CreateBlock("Placa_Induccion", new Vector3(xIzq + 2, 1.05f, z), new Vector3(3f, 0.04f, 2.2f), Color.black, root.transform);
        for(int i = 0; i < 2; i++) {
            for(int j = 0; j < 2; j++) {
                GameObject fuego = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                fuego.transform.SetParent(placa.transform);
                fuego.transform.localPosition = new Vector3(-0.7f + i*1.4f, 0.03f, -0.55f + j*1.1f);
                fuego.transform.localScale = new Vector3(0.6f, 0.01f, 0.6f);
                fuego.GetComponent<Renderer>().material.color = new Color(0.9f, 0.2f, 0.1f);
            }
        }
        
        // Campana extractora
        CreateBlock("Campana", new Vector3(xIzq + 2, 3f, z), new Vector3(3.5f, 2f, 2.5f), aceroInox, root.transform);
        CreateBlock("Campana_Filtro", new Vector3(xIzq + 2, 2f, z), new Vector3(3.3f, 0.12f, 2.3f), new Color(0.25f, 0.25f, 0.25f), root.transform);
        
        // Horno doble
        GameObject torre = CreateBlock("Torre_Hornos", new Vector3(xIzq - 4, 1.5f, z), new Vector3(2.8f, 3f, 2.8f), negroMate, root.transform);
        CreateBlock("Horno_Sup_Puerta", new Vector3(xIzq - 4, 2.2f, z - 1.45f), new Vector3(2.6f, 1.2f, 0.08f), new Color(0.18f, 0.18f, 0.18f), torre.transform);
        CreateBlock("Horno_Sup_Cristal", new Vector3(xIzq - 4, 2.2f, z - 1.4f), new Vector3(2.3f, 0.9f, 0.04f), new Color(0.08f, 0.08f, 0.12f), torre.transform);
        CreateBlock("Horno_Inf_Puerta", new Vector3(xIzq - 4, 0.8f, z - 1.45f), new Vector3(2.6f, 1.2f, 0.08f), new Color(0.18f, 0.18f, 0.18f), torre.transform);
        CreateBlock("Horno_Inf_Cristal", new Vector3(xIzq - 4, 0.8f, z - 1.4f), new Vector3(2.3f, 0.9f, 0.04f), new Color(0.08f, 0.08f, 0.12f), torre.transform);
        
        // Microondas
        CreateBlock("Microondas", new Vector3(xIzq - 4, 3.2f, z), new Vector3(2.6f, 0.9f, 2.6f), aceroInox, root.transform);
        CreateBlock("Micro_Puerta", new Vector3(xIzq - 4, 3.2f, z - 1.35f), new Vector3(2.4f, 0.7f, 0.04f), new Color(0.12f, 0.12f, 0.12f), root.transform);

        // ZONA LAVADO (Derecha)
        float xDer = 28;
        CreateBlock("Mueble_Lavado", new Vector3(xDer, 0.5f, z), new Vector3(10, 1f, 2.8f), maderaRoble, root.transform);
        CreateBlock("Encimera_Lavado", new Vector3(xDer, 1.02f, z), new Vector3(10.3f, 0.1f, 3f), marmolCarrara, root.transform);
        
        // Fregadero doble
        CreateBlock("Pila_Principal", new Vector3(xDer - 1.8f, 1.05f, z), new Vector3(2f, 0.1f, 1.6f), aceroInox, root.transform);
        CreateBlock("Pila_Auxiliar", new Vector3(xDer + 1.8f, 1.05f, z), new Vector3(2f, 0.1f, 1.6f), aceroInox, root.transform);
        
        // Grifo
        GameObject grifo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        grifo.name = "Grifo_Principal";
        grifo.transform.SetParent(root.transform);
        grifo.transform.position = new Vector3(xDer, 1.6f, z + 0.9f);
        grifo.transform.localScale = new Vector3(0.1f, 0.7f, 0.1f);
        grifo.GetComponent<Renderer>().material.color = aceroInox;
        CreateBlock("Grifo_Caño", new Vector3(xDer, 2.1f, z + 0.4f), new Vector3(0.1f, 0.1f, 1f), aceroInox, root.transform);
        
        // Lavavajillas
        CreateBlock("Lavavajillas", new Vector3(xDer + 4, 0.5f, z), new Vector3(2.4f, 1f, 2.8f), aceroInox, root.transform);
        CreateBlock("Lava_Panel", new Vector3(xDer + 4, 0.9f, z - 1.45f), new Vector3(2.2f, 0.35f, 0.04f), new Color(0.15f, 0.15f, 0.15f), root.transform);
        for(int i = 0; i < 6; i++) {
            CreateBlock("LED_" + i, new Vector3(xDer + 4 - 1f + i*0.4f, 0.9f, z - 1.47f), new Vector3(0.12f, 0.12f, 0.02f), Color.cyan, root.transform);
        }
        
        // Nevera americana
        GameObject nevera = CreateBlock("Nevera_Americana", new Vector3(xDer - 6, 2.2f, z), new Vector3(4f, 4.4f, 3f), aceroInox, root.transform);
        CreateBlock("Nevera_Puerta_L", new Vector3(xDer - 7.05f, 2.2f, z - 1.55f), new Vector3(1.8f, 4.3f, 0.08f), new Color(0.68f, 0.68f, 0.72f), nevera.transform);
        CreateBlock("Nevera_Maneta_L", new Vector3(xDer - 6.3f, 2.6f, z - 1.6f), new Vector3(0.12f, 1f, 0.06f), new Color(0.25f, 0.25f, 0.25f), nevera.transform);
        CreateBlock("Nevera_Puerta_R", new Vector3(xDer - 4.95f, 2.2f, z - 1.55f), new Vector3(1.8f, 4.3f, 0.08f), new Color(0.68f, 0.68f, 0.72f), nevera.transform);
        CreateBlock("Nevera_Maneta_R", new Vector3(xDer - 5.7f, 2.6f, z - 1.6f), new Vector3(0.12f, 1f, 0.06f), new Color(0.25f, 0.25f, 0.25f), nevera.transform);
        CreateBlock("Dispensador", new Vector3(xDer - 4.95f, 3f, z - 1.62f), new Vector3(0.7f, 1f, 0.04f), new Color(0.18f, 0.18f, 0.22f), nevera.transform);

        // ISLA CENTRAL
        float zIsla = 8;
        CreateBlock("Isla_Base", new Vector3(0, 0.5f, zIsla), new Vector3(14, 1f, 4.5f), maderaRoble, root.transform);
        CreateBlock("Isla_Encimera", new Vector3(0, 1.02f, zIsla), new Vector3(14.5f, 0.14f, 4.8f), marmolCarrara, root.transform);
        CreateBlock("Tabla_Profesional", new Vector3(0, 1.06f, zIsla), new Vector3(2f, 0.1f, 1.2f), new Color(0.65f, 0.45f, 0.25f), root.transform);
        
        // Taburetes
        for(int i = -2; i <= 2; i++) {
            if(i != 0) GenerarTabureteModerno(new Vector3(i * 3.5f, 0, zIsla - 3.5f), root.transform);
        }

        // ARMARIOS SUPERIORES
        for(int i = 0; i < 4; i++) {
            CreateBlock("Armario_Sup_L_" + i, new Vector3(-25 + i*3f, 3f, 23.8f), new Vector3(2.7f, 1.8f, 1.3f), maderaRoble, root.transform);
            CreateBlock("Armario_Sup_R_" + i, new Vector3(25 - i*3f, 3f, 23.8f), new Vector3(2.7f, 1.8f, 1.3f), maderaRoble, root.transform);
        }

        // DETALLES DECORATIVOS
        GameObject bote = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bote.name = "Bote_Utensilios";
        bote.transform.SetParent(root.transform);
        bote.transform.position = new Vector3(-5, 1.3f, zIsla);
        bote.transform.localScale = new Vector3(0.35f, 0.5f, 0.35f);
        bote.GetComponent<Renderer>().material.color = new Color(0.85f, 0.85f, 0.88f);
        
        for(int i = 0; i < 6; i++) {
            CreateBlock("Cuchillo_" + i, new Vector3(-26 + i*0.5f, 1.8f, 23.5f), new Vector3(0.06f, 0.6f, 0.02f), new Color(0.92f, 0.92f, 0.92f), root.transform);
        }
        
        GameObject frutero = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        frutero.name = "Frutero";
        frutero.transform.SetParent(root.transform);
        frutero.transform.position = new Vector3(3, 1.2f, zIsla);
        frutero.transform.localScale = new Vector3(0.8f, 0.2f, 0.8f);
        frutero.GetComponent<Renderer>().material.color = marmolCarrara;

        // ILUMINACIÓN
        for(int i = 0; i < 8; i++) {
            CreatePointLight(new Vector3(-28 + i*7f, 2.2f, 23f), new Color(1f, 0.96f, 0.92f), 9, 1.4f, root.transform);
        }
        
        for(int i = -1; i <= 1; i++) {
            GameObject lamp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lamp.name = "Lampara_" + i;
            lamp.transform.SetParent(root.transform);
            lamp.transform.position = new Vector3(i * 4f, 5.5f, zIsla);
            lamp.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            lamp.GetComponent<Renderer>().material.color = new Color(1f, 0.92f, 0.75f);
            CreatePointLight(new Vector3(i * 4f, 5f, zIsla), new Color(1f, 0.96f, 0.88f), 14, 2.2f, root.transform);
        }
        
        for(int i = 0; i < 4; i++) {
            CreatePointLight(new Vector3(-20 + i*13f, 8.5f, 0), Color.white, 20, 1.8f, root.transform);
        }

        // ESTACIONES INTERACTIVAS
        GameObject isla = GameObject.Find("Isla_Base");
        if(isla != null) isla.AddComponent<CuttingStation>();

        Debug.Log("═══════════════════════════════════════════════════════");
        Debug.Log("✅ COCINA PROFESIONAL GENERADA EXITOSAMENTE");
        Debug.Log("📦 Nevera Americana | Horno Doble | Microondas");
        Debug.Log("📦 Lavavajillas | Placa Inducción | Campana | Fregadero");
        Debug.Log("═══════════════════════════════════════════════════════");
    }

    void GenerarTabureteModerno(Vector3 pos, Transform parent)
    {
        GameObject taburete = new GameObject("Taburete");
        taburete.transform.SetParent(parent);
        taburete.transform.position = pos;

        // Asiento
        CreateBlock("Asiento", pos + Vector3.up * 0.6f, new Vector3(1f, 0.15f, 1f), new Color(0.2f, 0.2f, 0.22f), taburete.transform);
        
        // Pata central
        GameObject pata = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pata.name = "Pata";
        pata.transform.SetParent(taburete.transform);
        pata.transform.position = pos + Vector3.up * 0.3f;
        pata.transform.localScale = new Vector3(0.15f, 0.6f, 0.15f);
        pata.GetComponent<Renderer>().material.color = new Color(0.7f, 0.7f, 0.75f);
        
        // Base
        GameObject baseObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        baseObj.name = "Base";
        baseObj.transform.SetParent(taburete.transform);
        baseObj.transform.position = pos + Vector3.up * 0.05f;
        baseObj.transform.localScale = new Vector3(0.8f, 0.05f, 0.8f);
        baseObj.GetComponent<Renderer>().material.color = new Color(0.7f, 0.7f, 0.75f);
    }

    // ==============================================================================================
    // AQUI EMPIEZA LA GENERACION DEL RESTAURANTE (EL CODIGO ORIGINAL QUE YA FUNCIONABA)
    // ==============================================================================================

    [ContextMenu("💎 [PRESENTACIÓN PRO] GENERAR ENTORNO ELITE")]
    public void GenerarRestaurante()
    {
        Debug.Log(">>> INICIANDO PROYECTO: RESTAURANTE ELITE EXECUTIVE 2026 <<<");

        // 1. LIMPIEZA TOTAL (Garantizar que no queda NADA antiguo de ningún tipo)
        string[] rootNames = { "RESTAURANTE_ESTRUCTURA", "COCINA_2D", "Environment_Main" };
        foreach(string rName in rootNames) {
            GameObject old = GameObject.Find(rName);
            if(old != null) DestroySafe(old);
        }
        
        GameObject root = new GameObject("RESTAURANTE_ESTRUCTURA");
        restauranteContainer = root;

        // --- PALETA DE LUJO (Fijada para evitar errores) ---
        Color colWallnut = new Color(0.18f, 0.12f, 0.08f); 
        Color colMarble = new Color(0.96f, 0.96f, 0.98f); 
        Color colBrushedGold = new Color(0.82f, 0.65f, 0.35f); 
        Color colMatteBlack = new Color(0.08f, 0.08f, 0.1f);
        Color colSkyBlue = new Color(0.5f, 0.7f, 1f); // Para las ventanas (sin shader para evitar magenta)

        // --- HIERARCHY ---
        Transform fStructure = new GameObject("_ARCH_Architectural_Shell").transform; fStructure.SetParent(root.transform);
        Transform fDining = new GameObject("_ZONE_Dining_Experience").transform; fDining.SetParent(root.transform);
        Transform fKitchen = new GameObject("_CORE_Culinary_Center").transform; fKitchen.SetParent(root.transform);
        Transform fAtmosphere = new GameObject("_FX_Mood_Lighting").transform; fAtmosphere.SetParent(root.transform);

        // 1. CONFIGURACIÓN DE ESCENA Y CÁMARA
        SetupCameraRealist(new Vector3(0, 20, -28), 38);
        
        GameObject sun = new GameObject("Executive_Sun"); sun.transform.SetParent(fAtmosphere);
        Light sl = sun.AddComponent<Light>(); sl.type = LightType.Directional; sl.intensity = 1.3f;
        sun.transform.rotation = Quaternion.Euler(45, -35, 0);

        // 2. ARQUITECTURA (Suelo y Techo)
        // Suelo de Mármol con zonificación técnica
        CreateBlock("Suelo_Elite", new Vector3(0, -0.1f, 10), new Vector3(65, 0.2f, 100), colMarble, fStructure);
        
        // Techo Estructural (Slat Design)
        for(int i=-15; i<15; i++) {
            CreateBlock("Slat_" + i, new Vector3(i*2.2f, 10f, 10), new Vector3(0.4f, 0.4f, 100), colWallnut, fStructure);
        }

        // Muros con "Hornos de Luz" (Sin ventanas transparentes para evitar magenta)
        float hW = 10f;
        CreateWall("BackWall", new Vector3(0, hW/2, 60), new Vector3(65, hW, 1.5f), Color.white, fStructure);
        for(int x=-1; x<=1; x+=2) {
            // Ventanas sólidas pero elegantes
            GameObject win = CreateBlock("LightPanel_" + x, new Vector3(x*18, 5f, 59.3f), new Vector3(12, 6, 0.2f), colSkyBlue, fStructure);
            CreatePointLight(win.transform.position + Vector3.back * 2f, Color.white, 15, 1.5f, fAtmosphere);
        }

        // 3. COCINA INDUSTRIAL "SHOW KITCHEN" (Al fondo)
        Vector3 kPos = new Vector3(-12, 0, 48);
        GenerarBloqueCocinaElite(kPos, fKitchen);
        
        // Isla Central "The Monolith" (Expedición)
        GenerarIslaExecutivePrep(new Vector3(0, 0, 38), 12, fKitchen);
        
        // Neveras de Cristal retroiluminadas
        GenerarTorreFrioElite(new Vector3(18, 0, 48), fKitchen);

        // 4. RECEPCIÓN Y ÁREA DE CLIENTES
        // Barra Mostrador "Executive Reception"
        GameObject bar = new GameObject("Reception_Hostess");
        bar.transform.SetParent(fDining);
        bar.transform.position = new Vector3(0, 0, -8);
        CreateBlock("Body_Walnut", new Vector3(0, 0.62f, -8), new Vector3(18, 1.25f, 3.5f), colWallnut, bar.transform);
        CreateBlock("Top_Marble", new Vector3(0, 1.25f, -8), new Vector3(18.5f, 0.15f, 4f), Color.white, bar.transform);
        CreateBlock("Gold_Zocalo", new Vector3(0, 0.1f, -6), new Vector3(16, 0.2f, 0.1f), colBrushedGold, bar.transform);
        
        bar.AddComponent<DeliveryStation>();
        // CrearTextoInWorld(bar.transform, "E X E C U T I V E  H O S T", new Vector3(0, 1.5f, 0));

        // Mesas de Diseño Minimalista
        for(int r=0; r<2; r++) {
            for(int c=-1; c<=1; c+=2) {
                Vector3 posTable = new Vector3(c*15, 0, 15 - r*18);
                GenerarTableSetElite(posTable, fDining);
            }
        }

        // 5. ILUMINACIÓN "MOOD" CLUSTER
        for(int i=0; i<6; i++) {
             CreatePointLight(new Vector3((i%2==0?-1:1)*15, 7, 15 - (i/2)*18), new Color(1f, 0.9f, 0.7f), 15, 2f, fAtmosphere);
        }

        Debug.Log(">>> SISTEMA ARCHITECTURAL ELITE CARGADO CON ÉXITO.");
        if (Application.isPlaying) ShowRound();
    }

    void GenerarTableSetElite(Vector3 pos, Transform parent)
    {
        GameObject set = new GameObject("TableSet_Elite");
        set.transform.SetParent(parent);
        set.transform.position = pos;

        // Mesa
        CreateBlock("Pillar", pos + Vector3.up * 0.45f, new Vector3(0.4f, 0.9f, 0.4f), new Color(0.82f, 0.65f, 0.35f), set.transform);
        CreateBlock("Top", pos + Vector3.up * 0.95f, new Vector3(4.5f, 0.1f, 4.5f), Color.white, set.transform);

        // Sillas de Diseño
        for(int i=0; i<4; i++) {
            float angle = i * 90 * Mathf.Deg2Rad;
            Vector3 sPos = pos + new Vector3(Mathf.Sin(angle)*2.5f, 0, Mathf.Cos(angle)*2.5f);
            GenerarSillaModerna(sPos, i*90, set.transform);
        }
    }

    void GenerarSillaModerna(Vector3 pos, float rotY, Transform parent)
    {
        GameObject chair = new GameObject("Designer_Chair");
        chair.transform.SetParent(parent);
        chair.transform.position = pos;
        chair.transform.rotation = Quaternion.Euler(0, rotY, 0);

        Color colFabric = new Color(0.15f, 0.15f, 0.18f);
        // Patas Oro
        CreateBlock("Legs", pos + Vector3.up * 0.25f, new Vector3(1f, 0.1f, 1f), new Color(0.82f, 0.65f, 0.35f), chair.transform);
        // Asiento y Respaldo
        CreateBlock("Seat", pos + Vector3.up * 0.45f, new Vector3(1.2f, 0.3f, 1.2f), colFabric, chair.transform);
        CreateBlock("Back", pos + Vector3.up * 1.1f + Vector3.forward * 0.45f, new Vector3(1.2f, 1.0f, 0.2f), colFabric, chair.transform);
    }

    void GenerarBloqueCocinaElite(Vector3 pos, Transform parent)
    {
        GameObject kitchen = new GameObject("Kitchen_HotLine");
        kitchen.transform.SetParent(parent);
        
        // Bloque central de fuegos
        GenerarCocinaPro(pos); 
        // Campana Industrial de Acero Pulido
        GenerarCampanaGigante(pos + Vector3.up * 1.5f);
        // Detalles de pared (Azulejo o Panel Inox)
        CreateBlock("Backsplash", pos + Vector3.forward * 1.3f + Vector3.up * 1f, new Vector3(8, 2.5f, 0.1f), new Color(0.7f, 0.7f, 0.72f), kitchen.transform);
    }

    void GenerarIslaExecutivePrep(Vector3 pos, float len, Transform parent)
    {
        // Una isla imponente de mármol con estantes metálicos abajo
        GameObject isla = CreateBlock("Expo_Island", pos, new Vector3(len, 1.25f, 3.5f), new Color(0.12f, 0.12f, 0.12f), parent);
        CreateBlock("Marble_Top", pos + Vector3.up * 0.65f, new Vector3(len + 0.2f, 0.15f, 3.8f), Color.white, isla.transform);
        
        // Tallas de corte integradas
        for(int i=-1; i<=1; i++) {
            GenerarTablaCortePro(pos + Vector3.right * i * (len/3f));
        }
        isla.AddComponent<CuttingStation>();
    }

    void GenerarSofaElite(Vector3 pos, float rotY, Transform parent)
    {
        GameObject sofa = new GameObject("Executive_Sofa");
        sofa.transform.SetParent(parent);
        sofa.transform.position = pos;
        sofa.transform.rotation = Quaternion.Euler(0, rotY, 0);
        
        Color colLeather = new Color(0.15f, 0.1f, 0.08f); // Cuero oscuro
        // Base Escultural
        CreateBlock("Base", pos + Vector3.up * 0.2f, new Vector3(8, 0.4f, 3.5f), Color.black, sofa.transform);
        // Cojines Proporcionados
        CreateBlock("Cushions", pos + Vector3.up * 0.61f, new Vector3(7.8f, 0.45f, 3.2f), colLeather, sofa.transform);
        // Respaldo Envolvente
        CreateBlock("Backrest", pos + Vector3.up * 1.4f + Vector3.forward * 1.6f, new Vector3(7.8f, 1.5f, 0.5f), colLeather, sofa.transform);
    }

    void GenerarMesaMarmolElite(Vector3 pos, Transform parent)
    {
        GameObject mesa = new GameObject("Table_Mosaico");
        mesa.transform.SetParent(parent);
        mesa.transform.position = pos;
        
        // Pata central de latón (Oro)
        CreateBlock("Pillar", pos + Vector3.up * 0.45f, new Vector3(0.5f, 0.9f, 0.5f), new Color(0.82f, 0.65f, 0.35f), mesa.transform);
        // Sobre circular (Octógono visual) de Mármol Statuario
        CreateBlock("Top", pos + Vector3.up * 0.95f, new Vector3(4.5f, 0.12f, 4.5f), Color.white, mesa.transform);
    }

    void GenerarSillaPremiumElite(Vector3 pos, float rotY, Transform parent)
    {
        GameObject chair = new GameObject("Chair_Elite");
        chair.transform.SetParent(parent);
        chair.transform.position = pos;
        chair.transform.rotation = Quaternion.Euler(0, rotY, 0);
        
        Color colVelvet = new Color(0.12f, 0.2f, 0.15f); // Terciopelo verde oscuro executive
        // Estructura patas oro
        CreateBlock("Frame", pos + Vector3.up * 0.3f, new Vector3(1.2f, 0.1f, 1.2f), new Color(0.82f, 0.65f, 0.35f), chair.transform);
        // Asiento
        CreateBlock("Seat", pos + Vector3.up * 0.5f, new Vector3(1.25f, 0.35f, 1.25f), colVelvet, chair.transform);
        // Respaldo bajo curvo
        CreateBlock("Back", pos + Vector3.up * 1.1f + Vector3.forward * 0.5f, new Vector3(1.25f, 1.0f, 0.25f), colVelvet, chair.transform);
    }

    void GenerarTorreFrioElite(Vector3 pos, Transform parent)
    {
        GameObject fridge = CreateBlock("Elite_Wine_Cooler", pos + Vector3.up * 1.5f, new Vector3(4, 3.5f, 3), new Color(0.1f, 0.1f, 0.12f), parent);
        // Cristal retroiluminado azul
        CreateBlock("Display", pos + Vector3.up * 1.5f + Vector3.right * 2.05f, new Vector3(0.1f, 3.2f, 2.5f), new Color(0.4f, 0.6f, 1f, 0.3f), fridge.transform);
    }

    void GenerarRackIngredientesElite(Vector3 pos, Transform parent)
    {
        GameObject rack = CreateBlock("Ingredient_Rack", pos + Vector3.up * 1.5f, new Vector3(2.5f, 4.5f, 6f), new Color(0.15f, 0.15f, 0.15f), parent);
        // Baldas de cristal
        for(int i=0; i<4; i++) {
            CreateBlock("Shelf", pos + Vector3.up * (i*1.1f + 0.5f), new Vector3(2.6f, 0.05f, 5.8f), Color.white, rack.transform);
        }
    }

    void GenerarSofaModular(Vector3 pos, Transform parent)
    {
        GameObject sofa = new GameObject("Sofa_Premium");
        sofa.transform.SetParent(parent);
        sofa.transform.position = pos;
        
        Color colLeather = new Color(0.25f, 0.15f, 0.1f);
        // Base y Asiento
        CreateBlock("Base", pos + Vector3.up * 0.2f, new Vector3(6f, 0.4f, 3f), Color.black, sofa.transform);
        CreateBlock("Seating", pos + Vector3.up * 0.5f, new Vector3(5.8f, 0.4f, 2.8f), colLeather, sofa.transform);
        // Respaldo
        CreateBlock("Backrest", pos + Vector3.up * 1.2f + Vector3.forward * 1.3f, new Vector3(5.8f, 1.2f, 0.4f), colLeather, sofa.transform);
    }

    void GenerarVentanalPremium(Vector3 pos, Transform parent)
    {
        CreateBlock("Frame_Top", pos + Vector3.up * 4.5f, new Vector3(40f, 0.3f, 0.3f), Color.black, parent);
        GameObject glass = CreateBlock("Panoramic_Glass", pos, new Vector3(38f, 9f, 0.05f), new Color(0.7f, 0.9f, 1f, 0.15f), parent);
        // Material transparente configurado mediante wrapper o lógica
    }

    void GenerarOrbitalBooth(Vector3 pos, Transform parent)
    {
        GameObject booth = new GameObject("OrbitalBooth");
        booth.transform.SetParent(parent);
        booth.transform.position = pos;
        
        // Base Escultural
        CreateBlock("Base", pos + Vector3.up * 0.2f, new Vector3(2f, 0.4f, 10f), new Color(0.15f, 0.15f, 0.15f), booth.transform);
        // Cojines Terciopelo Sage
        CreateBlock("Seating", pos + Vector3.up * 0.5f, new Vector3(1.8f, 0.3f, 9.6f), new Color(0.44f, 0.50f, 0.44f), booth.transform);
        // Respaldo Arquitectónico
        CreateBlock("Backrest", pos + Vector3.up * 1.5f + Vector3.left * 0.8f, new Vector3(0.4f, 2.2f, 9.8f), new Color(0.12f, 0.12f, 0.12f), booth.transform);
    }

    void GenerarMesaEscultural(Vector3 pos, Transform parent)
    {
        GameObject mesa = new GameObject("Mesa_Escultural");
        mesa.transform.SetParent(parent);
        mesa.transform.position = pos;
        
        // Pata en V de latón
        CreateBlock("Pillar_Brass", pos + Vector3.up * 0.45f, new Vector3(0.2f, 0.9f, 1.5f), new Color(0.82f, 0.65f, 0.35f), mesa.transform);
        // Sobre de Ónix Negro
        CreateBlock("Top_Onyx", pos + Vector3.up * 0.95f, new Vector3(3f, 0.1f, 3f), new Color(0.05f, 0.05f, 0.05f), mesa.transform);
    }

    void GenerarTheMonolithMesa(Vector3 pos, Transform parent)
    {
        GameObject table = new GameObject("Mesa_Monolithic");
        table.transform.SetParent(parent);
        table.transform.position = pos;
        
        // Tablero masivo de piedra
        CreateBlock("Stone_Slab", pos + Vector3.up * 0.85f, new Vector3(5f, 0.25f, 15f), new Color(0.1f, 0.1f, 0.12f), table.transform);
        // Patas cilíndricas de Oro Mate
        for(int i=0; i<4; i++) {
            Vector3 offset = new Vector3(i<2?1.8f:-1.8f, 0, i%2==0?6f:-6f);
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leg.transform.SetParent(table.transform);
            leg.transform.position = pos + Vector3.up * 0.4f + offset;
            leg.transform.localScale = new Vector3(0.4f, 0.45f, 0.4f);
            leg.GetComponent<Renderer>().material.color = new Color(0.82f, 0.65f, 0.35f);
        }
        
        // Sillas de Diseñador
        for(int z=0; z<5; z++) {
            GenerarSillaPremium(pos + new Vector3(3.2f, 0, -6f + z * 3f), 90, table.transform);
            GenerarSillaPremium(pos + new Vector3(-3.2f, 0, -6f + z * 3f), 270, table.transform);
        }
    }

    void GenerarNebulaLamp(Vector3 pos, Transform parent)
    {
        GameObject lamp = new GameObject("NebulaCluster");
        lamp.transform.SetParent(parent);
        lamp.transform.position = pos;
        
        // Esfera de cristal ahumado
        GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        orb.transform.SetParent(lamp.transform);
        orb.transform.position = pos;
        orb.transform.localScale = Vector3.one * 1.5f;
        Renderer r = orb.GetComponent<Renderer>();
        r.material = new Material(Shader.Find("Standard"));
        r.material.color = new Color(1, 1, 1, 0.2f);
        r.material.SetFloat("_Mode", 3);
        
        // Luz cálida integrada
        Light l = lamp.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(1f, 0.9f, 0.7f);
        l.range = 15f;
        l.intensity = 2f;
    }

    void GenerarColumnaBiofilica(Vector3 pos, Transform parent)
    {
        GameObject col = CreateBlock("Columna_Eco", pos + Vector3.up * 5f, new Vector3(1.5f, 10f, 1.5f), new Color(0.12f, 0.12f, 0.12f), parent);
        // Jardín vertical en la columna
        for(int i=0; i<5; i++) {
             GameObject foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
             foliage.transform.SetParent(col.transform);
             foliage.transform.localPosition = new Vector3(0, -0.4f + i*0.2f, 0.6f);
             foliage.transform.localScale = new Vector3(1.2f, 0.4f, 0.8f);
             foliage.GetComponent<Renderer>().material.color = new Color(0.1f, 0.4f, 0.15f);
        }
    }

    void GenerarBancoCorridoModerno(Vector3 pos, Transform parent)
    {
        GameObject banco = new GameObject("Banco_Moderno");
        banco.transform.SetParent(parent);
        banco.transform.position = pos;
        
        // Base madera
        CreateBlock("Base_Wood", pos + Vector3.up * 0.2f, new Vector3(1.5f, 0.4f, 8f), new Color(0.8f, 0.7f, 0.5f), banco.transform);
        // Tapizado Verde Salvia
        CreateBlock("Cojin", pos + Vector3.up * 0.5f, new Vector3(1.4f, 0.2f, 7.8f), new Color(0.44f, 0.50f, 0.44f), banco.transform);
        // Respaldo
        CreateBlock("Respaldo", pos + Vector3.up * 1.2f + Vector3.left * 0.6f, new Vector3(0.2f, 1.2f, 7.8f), new Color(0.44f, 0.50f, 0.44f), banco.transform);
    }

    void GenerarMesaSocial(Vector3 pos, Transform parent)
    {
        GameObject mesa = new GameObject("Mesa_Social_Larga");
        mesa.transform.SetParent(parent);
        mesa.transform.position = pos;
        
        // Tablero grueso de Roble
        CreateBlock("Tablero", pos + Vector3.up * 0.9f, new Vector3(4f, 0.15f, 12f), new Color(0.85f, 0.72f, 0.52f), mesa.transform);
        // Patas de acero negro en U
        CreateBlock("Pata_1", pos + Vector3.up * 0.45f + Vector3.forward * 4.5f, new Vector3(3.5f, 0.9f, 0.2f), Color.black, mesa.transform);
        CreateBlock("Pata_2", pos + Vector3.up * 0.45f + Vector3.back * 4.5f, new Vector3(3.5f, 0.9f, 0.2f), Color.black, mesa.transform);
        
        // Sillas alrededor
        for(int i=0; i<4; i++) {
            GenerarSillaDiseno_WithParent(pos + new Vector3(2.5f, 0.8f, -4.5f + i*3.0f), 90, Color.black, mesa.transform);
            GenerarSillaDiseno_WithParent(pos + new Vector3(-2.5f, 0.8f, -4.5f + i*3.0f), 270, Color.black, mesa.transform);
        }
    }

    void GenerarMesaMarmol(Vector3 pos, Transform parent)
    {
        GameObject mesa = new GameObject("Mesa_Marmol");
        mesa.transform.SetParent(parent);
        mesa.transform.position = pos;
        
        // Pata central (Base dorada/negra)
        CreateBlock("Pata", pos + Vector3.up * 0.45f, new Vector3(0.3f, 0.9f, 0.3f), Color.black, mesa.transform);
        // Sobre de Mármol
        CreateBlock("Sobre", pos + Vector3.up * 0.95f, new Vector3(2.5f, 0.1f, 2.5f), new Color(0.95f, 0.95f, 0.95f), mesa.transform);
    }

    void GenerarSillaPremium(Vector3 pos, float rotY, Transform parent)
    {
        GameObject silla = new GameObject("Silla_Premium");
        silla.transform.SetParent(parent);
        silla.transform.position = pos;
        silla.transform.rotation = Quaternion.Euler(0, rotY, 0);

        // Asiento terciopelo
        CreateBlock("Asiento", pos + Vector3.up * 0.4f, new Vector3(1f, 0.2f, 1f), new Color(0.44f, 0.50f, 0.44f), silla.transform);
        // Respaldo curvo
        CreateBlock("Respaldo", pos + Vector3.up * 1.0f + silla.transform.forward * -0.4f, new Vector3(1f, 1.0f, 0.15f), new Color(0.44f, 0.50f, 0.44f), silla.transform);
    }

    void GenerarLamparaColgante(Vector3 pos)
    {
        GameObject lamp = new GameObject("Lampara_Designer");
        lamp.transform.SetParent(restauranteContainer.transform);
        lamp.transform.position = pos;

        // Cable fino
        CreateBlock("Cable", pos + Vector3.up * 1.5f, new Vector3(0.05f, 3f, 0.05f), Color.black, lamp.transform);
        // Tulipa latón
        CreateBlock("Tulipa", pos, new Vector3(0.8f, 0.4f, 0.8f), new Color(0.72f, 0.58f, 0.32f), lamp.transform);
        
        // Punto de luz cálido
        GameObject pl = new GameObject("Light");
        pl.transform.SetParent(lamp.transform);
        pl.transform.position = pos + Vector3.down * 0.2f;
        Light l = pl.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(1f, 0.9f, 0.7f);
        l.range = 8f;
        l.intensity = 1.5f;
    }

    void GenerarBowlFrutas(Vector3 pos)
    {
        GameObject bowl = CreateBlock("Bowl_Decor", pos, new Vector3(0.8f, 0.2f, 0.8f), Color.white);
        // Algunas esferas de colores (frutas)
        for(int i=0; i<3; i++) {
            GameObject fruit = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fruit.transform.SetParent(bowl.transform);
            fruit.transform.localPosition = new Vector3(Random.Range(-0.2f, 0.2f), 0.3f, Random.Range(-0.2f, 0.2f));
            fruit.transform.localScale = Vector3.one * 0.4f;
            fruit.GetComponent<Renderer>().material.color = i==0 ? Color.red : (i==1 ? Color.green : Color.yellow);
        }
    }

    public void ShowRound()
    {
        CrearCanvas(); // Recrear canvas si fue destruido
        if(gamePanel != null) Destroy(gamePanel);

        // Fondo Restaurante
        Sprite fondoGame = Resources.Load<Sprite>("RestauranteJuego");
        Color colorFondo = (fondoGame != null) ? Color.white : new Color(0.9f, 0.9f, 0.95f);
        gamePanel = CrearPanel(colorFondo, fondoGame);

        // Elegir escenario ALEATORIO
        Scenario current = scenarios[Random.Range(0, scenarios.Count)];

        // --- 1. HUD PUNTUACIÓN ---
        GameObject scorePanel = CrearPanelTransparente(gamePanel.transform, new Vector2(0.6f, 0.85f), new Vector2(0.98f, 0.98f));
        Image scoreBg = scorePanel.AddComponent<Image>();
        scoreBg.color = new Color(0, 0, 0, 0.6f); 
        
        string scoreText = $"<color=yellow>RONDA: {roundsSurvived + 1}</color>   |   <color=white>PUNTOS: {totalScore}</color>";
        CrearTexto(scorePanel.transform, scoreText, 0, 0, 30, Color.white, true);


        // --- 2. COCINERA ---
        GameObject cookObj = new GameObject("CocineraImg");
        cookObj.transform.SetParent(gamePanel.transform, false);
        Image cookImg = cookObj.AddComponent<Image>();
        Sprite spriteCocinera = GetOrGenerateSprite("Characters/Cocinera", Color.magenta);
        if(spriteCocinera != null) {
            cookImg.sprite = spriteCocinera;
            cookImg.color = Color.white;
            cookImg.preserveAspect = true;
        } else {
            cookImg.color = Color.gray; 
        }
        
        RectTransform cookRect = cookObj.GetComponent<RectTransform>();
        cookRect.anchorMin = new Vector2(0.02f, 0.1f); 
        cookRect.anchorMax = new Vector2(0.25f, 0.9f); 
        cookRect.offsetMin = Vector2.zero; cookRect.offsetMax = Vector2.zero;
        
        // --- 3. NUBE DE DIÁLOGO ---
        GameObject bubble = new GameObject("Bubble");
        bubble.transform.SetParent(gamePanel.transform, false);
        Image bubbleImg = bubble.AddComponent<Image>();
        Sprite nubeSprite = Resources.Load<Sprite>("NubeDialogo");
        if(nubeSprite != null) {
             bubbleImg.sprite = nubeSprite;
             bubbleImg.color = Color.white;
        } else { 
             bubbleImg.color = new Color(1f, 1f, 1f, 0.95f);
        }
           
        RectTransform bubbleRect = bubble.GetComponent<RectTransform>();
        bubbleRect.anchorMin = new Vector2(0.3f, 0.5f);
        bubbleRect.anchorMax = new Vector2(0.9f, 0.9f);
        bubbleRect.offsetMin = Vector2.zero; bubbleRect.offsetMax = Vector2.zero;

        // Texto Pregunta
        string pregunta = $"<b>CLIENTE: {current.clientName}</b>\n\n" +
                          $"{current.conditionDescription}\n\n" +
                          "<color=blue>¿Qué le preparas?</color>";
        
        GameObject txtObj = CrearTexto(bubble.transform, pregunta, 0, 0, 28, Color.black, true);
        RectTransform txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.offsetMin = new Vector2(40, 30);
        txtRect.offsetMax = new Vector2(-40, -30);


        // --- 4. OPCIONES (CARDS) ---
        // Area gris oscura de fondo para resaltar las cartas
        GameObject optionsArea = new GameObject("OptionsBackground");
        optionsArea.transform.SetParent(gamePanel.transform, false);
        Image optBg = optionsArea.AddComponent<Image>();
        optBg.color = new Color(0, 0, 0, 0.3f); // Fondo semitransparente detrás de las opciones

        RectTransform optKRect = optionsArea.GetComponent<RectTransform>();
        optKRect.anchorMin = new Vector2(0.3f, 0.05f);
        optKRect.anchorMax = new Vector2(0.95f, 0.45f); 
        optKRect.offsetMin = Vector2.zero; optKRect.offsetMax = Vector2.zero;

        // --- SISTEMA DE ALEATORIEDAD ---
        // Creamos una lista con las 3 opciones
        List<OptionDataHelper> choices = new List<OptionDataHelper>() {
            new OptionDataHelper { text = current.optionA_Text, image = current.optionA_Image, score = current.optionA_Score },
            new OptionDataHelper { text = current.optionB_Text, image = current.optionB_Image, score = current.optionB_Score },
            new OptionDataHelper { text = current.optionC_Text, image = current.optionC_Image, score = current.optionC_Score }
        };

        // Barajamos la lista (Shuffle)
        for (int i = 0; i < choices.Count; i++) {
            OptionDataHelper temp = choices[i];
            int randomIndex = Random.Range(i, choices.Count);
            choices[i] = choices[randomIndex];
            choices[randomIndex] = temp;
        }

        // Generamos las cartas en orden aleatorio
        string[] labels = { "A", "B", "C" };
        for (int i = 0; i < choices.Count; i++) {
            OptionDataHelper choice = choices[i];
            CrearCartaOpcion(optionsArea.transform, i, labels[i], choice.text, choice.image, () => CheckAnswer(choice.score));
        }

        // --- BOTÓN SALIR (X) ---
        // Botón pequeño en la esquina superior derecha para cerrar la pregunta
        GameObject closeBtnObj = new GameObject("Btn_Close");
        closeBtnObj.transform.SetParent(gamePanel.transform, false);
        Image closeImg = closeBtnObj.AddComponent<Image>();
        closeImg.color = new Color(0.8f, 0.2f, 0.2f); // Rojo
        
        Button closeBtn = closeBtnObj.AddComponent<Button>();
        closeBtn.onClick.AddListener(() => {
            if(gamePanel != null) Destroy(gamePanel);
            // Volver a mostrar el prompt de interacción si se acerca de nuevo
            StartCoroutine(ReenablePromptDelayed());
        });

        RectTransform closeRect = closeBtnObj.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.95f, 0.92f);
        closeRect.anchorMax = new Vector2(0.99f, 0.98f);
        closeRect.offsetMin = Vector2.zero; closeRect.offsetMax = Vector2.zero;

        CrearTexto(closeBtnObj.transform, "X", 0, 0, 20, Color.white, true);
    }

    IEnumerator ReenablePromptDelayed()
    {
        yield return new WaitForSeconds(1f);
        // Si el jugador sigue cerca (trigger exit no saltó), mostrar prompt de nuevo?
        // En realidad, OnTriggerEnter/Exit lo manejan. Al cerrar panel, simplemente volvemos al juego.
    }

    void CrearCartaOpcion(Transform parent, int index, string letter, string text, string imageName, UnityEngine.Events.UnityAction action)
    {
        GameObject card = new GameObject($"Option_{letter}");
        card.transform.SetParent(parent, false);
        
        Image bg = card.AddComponent<Image>();
        bg.color = Color.white; // Blanco puro para resaltar
        
        Button btn = card.AddComponent<Button>();
        btn.onClick.AddListener(action);
        
        // ColorBlock para feedback visual al hacer click
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.9f, 1f, 0.9f);
        cb.pressedColor = new Color(0.7f, 1f, 0.7f);
        btn.colors = cb;

        // Layout: 3 columnas con hueco
        float widthPerCard = 1f / 3f;
        float padding = 0.02f;
        
        RectTransform rect = card.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(index * widthPerCard + padding, 0.05f); 
        rect.anchorMax = new Vector2((index + 1) * widthPerCard - padding, 0.95f);
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        
        // -- Header: "OPCION X" --
        GameObject headerObj = CrearTexto(card.transform, $"OPCIÓN {letter}", 0, 0, 20, Color.gray, false);
        RectTransform headerRect = headerObj.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 0.85f); headerRect.anchorMax = new Vector2(1, 1);
        headerRect.offsetMin = Vector2.zero; headerRect.offsetMax = Vector2.zero;

        // -- Imagen --
        GameObject imgObj = new GameObject("Icon");
        imgObj.transform.SetParent(card.transform, false);
        Image icon = imgObj.AddComponent<Image>();
        Sprite foodSprite = GetOrGenerateSprite(imageName, new Color(Random.value, Random.value, Random.value));
        
        if(foodSprite != null) {
            icon.sprite = foodSprite;
            icon.color = Color.white;
            icon.preserveAspect = true;
        } else {
            // Placeholder visual si falta imagen
            icon.color = new Color(0.9f, 0.9f, 0.9f); 
            GameObject noImgTxt = CrearTexto(imgObj.transform, "SIN IMAGEN", 0,0, 14, Color.gray, true);
        }
        
        RectTransform imgRect = imgObj.GetComponent<RectTransform>();
        imgRect.anchorMin = new Vector2(0.1f, 0.4f);
        imgRect.anchorMax = new Vector2(0.9f, 0.85f); 
        imgRect.offsetMin = Vector2.zero; imgRect.offsetMax = Vector2.zero;

        // -- Texto Descripción --
        GameObject txtObj = CrearTexto(card.transform, text, 0, 0, 24, Color.black, true);
        txtObj.GetComponent<Text>().fontStyle = FontStyle.Bold;
        RectTransform txtTR = txtObj.GetComponent<RectTransform>();
        txtTR.anchorMin = new Vector2(0.05f, 0.15f);
        txtTR.anchorMax = new Vector2(0.95f, 0.4f); 
        txtTR.offsetMin = Vector2.zero; txtTR.offsetMax = Vector2.zero;
        
        // -- Hint "Click" --
        GameObject clickObj = CrearTexto(card.transform, "(Click para elegir)", 0,0, 14, new Color(0.3f, 0.3f, 0.8f), false);
        RectTransform clickRect = clickObj.GetComponent<RectTransform>();
        clickRect.anchorMin = new Vector2(0, 0); clickRect.anchorMax = new Vector2(1, 0.15f);
        clickRect.offsetMin = Vector2.zero; clickRect.offsetMax = Vector2.zero;
    }

    // --- Helpers UI Updates ---

    GameObject CrearPanelTransparente(Transform parent, Vector2 min, Vector2 max)
    {
        GameObject p = new GameObject("PanelT");
        p.transform.SetParent(parent, false);
        RectTransform r = p.AddComponent<RectTransform>();
        r.anchorMin = min; r.anchorMax = max; 
        r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
        return p;
    }

    GameObject CrearTexto(Transform parent, string contenido, float x, float y, int tamano, Color color, bool stretch = false)
    {
        GameObject t = new GameObject("Txt"); 
        t.transform.SetParent(parent, false);
        Text txt = t.AddComponent<Text>(); 
        txt.text = contenido; 
        txt.alignment = TextAnchor.MiddleCenter;
        txt.fontSize = tamano; 
        txt.color = color;
        txt.font = GetMyFont();
        txt.resizeTextForBestFit = true;
        txt.resizeTextMinSize = 10;
        txt.resizeTextMaxSize = tamano;
        
        // Soporte rico y overflow para que no se corte
        txt.supportRichText = true; 
        txt.verticalOverflow = VerticalWrapMode.Truncate; 
        txt.horizontalOverflow = HorizontalWrapMode.Wrap;

        RectTransform r = t.GetComponent<RectTransform>(); 
        if(stretch) {
            r.anchorMin = Vector2.zero; 
            r.anchorMax = Vector2.one;
            r.sizeDelta = Vector2.zero; 
        } else {
            r.sizeDelta = new Vector2(400, 100); 
            r.anchoredPosition = new Vector2(x,y);
        }
        return t;
    }

    void CheckAnswer(int score)
    {
        bool gameOver = false;
        string reason = "";

        if (score == 0)
        {
             // 0% -> Pierdes inmediatamente
            gameOver = true;
            reason = "¡Cuidado! Esa elección fue terrible (0%).";
        }
        else if (score == 50)
        {
            // 50% -> Pasas de ronda pero... si haces 3 seguidos pierdes
            consecutiveMediums++;
            if (consecutiveMediums >= 3)
            {
                gameOver = true;
                reason = "Has cometido demasiados errores 'regulares' seguidos (3x).";
            }
        }
        else if (score == 100)
        {
            // 100% -> Pasas y limpias rachas de errores
            consecutiveMediums = 0; 
        }

        if (gameOver)
        {
            ShowGameOver(reason);
        }
        else
        {
            // Sumar puntos y avanzar
            totalScore += score;
            roundsSurvived++;
            // Feedback breve antes de la siguiente ronda? O directa?
            // El usuario dijo "si aciertas pasas de ronda". Vamos a mostrar feedback rapido.
            ShowFeedback(score);
        }
    }

    void ShowFeedback(int score)
    {
        // Liberar al cliente si la respuesta fue aceptable (para que deje hueco al irse)
        if (score >= 50 && currentCustomerServed != null)
        {
            currentCustomerServed.SatisfyAndLeave();
        }
        currentCustomerServed = null;

        if(gamePanel != null) Destroy(gamePanel);
        // Fondo resultado
        gamePanel = CrearPanel(score == 100 ? new Color(0.2f, 0.6f, 0.2f) : (score == 50 ? new Color(0.8f, 0.8f, 0.2f) : new Color(0.8f, 0.2f, 0.2f)));

        string msg = "";
        if (score == 100) msg = "¡HAS ESCOGIDO BIEN! (+100)";
        else if (score == 50) msg = "REGULAR (+50)";
        else msg = "ELECCIÓN INCORRECTA (+0)";

        CrearTexto(gamePanel.transform, msg, 0, 50, 60, Color.white);
        
        CrearBoton(gamePanel.transform, "SIGUIENTE RETO >>", 0, -100, Color.white, () => {
             // Al dar a siguiente, quitamos el feedback y mostramos otra pregunta directamente
             if(gamePanel != null) Destroy(gamePanel);
             ShowRound();
        });
    }

    void ShowGameOver(string reason)
    {
        if(gamePanel != null) Destroy(gamePanel);
        gamePanel = CrearPanel(new Color(0.6f, 0.1f, 0.1f)); // Rojo oscuro

        CrearTexto(gamePanel.transform, "¡FIN DEL JUEGO!", 0, 100, 60, Color.yellow);
        CrearTexto(gamePanel.transform, reason, 0, 0, 30, Color.white);
        CrearTexto(gamePanel.transform, $"Rondas Sobrevividas: {roundsSurvived}\nPuntuación Final: {totalScore}", 0, -80, 40, Color.white);

        CrearBoton(gamePanel.transform, "VOLVER AL MENÚ", 0, -200, Color.white, MostrarMenuPrincipal);
    }


    // --- HELPERS UI ---

    void CrearCanvas()
    {
        if(this.menuCanvas == null) {
            this.menuCanvas = new GameObject("CanvasMenu");
            Canvas c = this.menuCanvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = this.menuCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            this.menuCanvas.AddComponent<GraphicRaycaster>();
            
            // Asegurar que se ve encima de todo
            c.sortingOrder = 999;
        }
    }

    // Helper para fuente robusta
    Font GetMyFont()
    {
         // INTENTO 1: LegacyRuntime.ttf (Standard Unity)
         Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
         // INTENTO 2: Arial del SO si falla
         if (f == null) f = Font.CreateDynamicFontFromOSFont("Arial", 16);
         return f;
    }

    GameObject CrearPanel(Color c, Sprite sprite = null)
    {
        CrearCanvas(); // Asegurar que el canvas existe antes de intentar ser hijo de él
        GameObject p = new GameObject("Panel");
        p.transform.SetParent(this.menuCanvas.transform, false);
        RectTransform r = p.AddComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.sizeDelta = Vector2.zero;
        
        Image i = p.AddComponent<Image>(); 
        i.color = c;
        if (sprite != null)
        {
            i.sprite = sprite;
            i.color = Color.white; // Para que se vea la imagen sin tinte
        }
        return p;
    }

    void CrearBoton(Transform parent, string texto, float x, float y, Color colorFondo, UnityEngine.Events.UnityAction accion)
    {
        GameObject b = new GameObject("Btn_" + texto);
        b.transform.SetParent(parent, false);
        
        Image img = b.AddComponent<Image>(); 
        img.color = colorFondo;
        
        Button btn = b.AddComponent<Button>(); 
        btn.onClick.AddListener(accion);
        
        RectTransform r = b.GetComponent<RectTransform>();
        r.sizeDelta = new Vector2(400, 60); 
        r.anchoredPosition = new Vector2(x, y);

        GameObject t = new GameObject("Text"); 
        t.transform.SetParent(b.transform, false);
        Text txt = t.AddComponent<Text>(); 
        txt.text = texto; 
        txt.alignment = TextAnchor.MiddleCenter;
        txt.fontSize = 20; 
        txt.color = Color.black; 
        txt.font = GetMyFont();
        
        RectTransform tr = t.GetComponent<RectTransform>(); 
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.sizeDelta = Vector2.zero;
    }

    GameObject CrearTexto(Transform parent, string contenido, float x, float y, int tamano, Color color)
    {
        GameObject t = new GameObject("Txt"); 
        t.transform.SetParent(parent, false);
        Text txt = t.AddComponent<Text>(); 
        txt.text = contenido; 
        txt.alignment = TextAnchor.MiddleCenter;
        txt.fontSize = tamano; 
        txt.color = color;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        
        RectTransform r = t.GetComponent<RectTransform>(); 
        r.sizeDelta = new Vector2(600, 300); 
        r.anchoredPosition = new Vector2(x,y);
        return t;
    }

    private Sprite GetOrGenerateSprite(string path, Color fallbackColor)
    {
        Sprite s = Resources.Load<Sprite>(path);
        if (s != null) return s;

        // Fallback: Create a run-time sprite so the UI never looks broken
        Texture2D tex = new Texture2D(64, 64);
        Color[] cols = new Color[64*64];
        for(int i=0; i<cols.Length; i++) cols[i] = fallbackColor;
        tex.SetPixels(cols);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,64,64), new Vector2(0.5f, 0.5f));
    }

    // --- BUILD METHODS (Missing Helpers) ---
    void CreateWall(string name, Vector3 pos, Vector3 scale, Transform parent)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent);
        wall.transform.position = pos;
        wall.transform.localScale = scale;
    }

    void CreateWall(string name, Vector3 pos, Vector3 scale, Color color, Transform parent)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent);
        wall.transform.position = pos;
        wall.transform.localScale = scale;
        wall.GetComponent<Renderer>().material.color = color;
    }

    void CreateGlassWindow(string name, Vector3 pos, Vector3 scale, Transform parent)
    {
        // REEMPLAZO ROBUSTO: En lugar de un shader transparente que da Magenta, usamos un bloque sólido "Celeste Cielo"
        // que simula una ventana con luz perfecta, ideal para presentaciones.
        GameObject win = GameObject.CreatePrimitive(PrimitiveType.Cube);
        win.name = name;
        win.transform.SetParent(parent);
        win.transform.position = pos;
        win.transform.localScale = scale;
        
        Renderer r = win.GetComponent<Renderer>();
        r.material = new Material(Shader.Find("Standard")); 
        r.material.color = new Color(0.5f, 0.75f, 1f); // Azul cielo profesional
        r.material.EnableKeyword("_EMISSION");
        r.material.SetColor("_EmissionColor", new Color(0.2f, 0.3f, 0.4f)); // Efecto glow sutil
    }

    
    void SpawnPlayerTopDown(Vector3 pos)
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "ChefTopDown";
        player.transform.SetParent(restauranteContainer.transform);
        player.transform.position = pos;
        player.GetComponent<Renderer>().material.color = Color.white;

        // Sombrero Chef
        GameObject hat = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        hat.transform.SetParent(player.transform);
        hat.transform.localPosition = new Vector3(0, 0.8f, 0);
        hat.transform.localScale = new Vector3(0.8f, 0.5f, 0.8f);
        hat.GetComponent<Renderer>().material.color = Color.white;

        // Player Controller (Top Down Mode)
        PlayerController pc = player.AddComponent<PlayerController>();
        pc.isFirstPerson = false; 
        pc.moveSpeed = 7f;
        pc.interactDistance = 2.5f;

        // Input
        PlayerInput pi = player.AddComponent<PlayerInput>();
        pi.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;

        // Cursor Visible
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
} // Fin class KitchenBootstrap

[System.Serializable]
public class Scenario
{
    public string clientName;
    public string conditionDescription;
    public string optionA_Text;
    public string optionA_Image;
    public int optionA_Score;
    public string optionB_Text;
    public string optionB_Image;
    public int optionB_Score;
    public string optionC_Text;
    public string optionC_Image;
    public int optionC_Score;
}

public struct OptionDataHelper
{
    public string text;
    public string image;
    public int score;
}
