using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using System.Collections;
using System.Collections.Generic;

// ESTE SCRIPT HACE TODO: CARGA, MENU Y JUEGO.
[ExecuteAlways]
public class KitchenBootstrap : MonoBehaviour
{
    public static KitchenBootstrap Instance;

    private GameObject menuCanvas;
    private GameObject currentMenuPanel;

    [Header("CONFIGURACIÓN DE DISEÑO")]
    public bool autoGenerateInEditor = true;
    
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

        #if UNITY_EDITOR
        if (!Application.isPlaying && autoGenerateInEditor) {
            BuscarOReemplazarCocina();
        }
        #endif
    }

    private void OnEnable()
    {
        #if UNITY_EDITOR
        if (!Application.isPlaying) {
             BuscarOReemplazarCocina();
        }
        #endif
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

    // --- INICIALIZACIÓN DE ESCENARIOS (Estrictamente Acidez, Migraña y Deporte) ---
    void InitializeScenarios()
    {
        scenarios = new List<Scenario>();

        // ======================================================================================
        // CATEGORIA 1: ACIDEZ
        // Sintomas: Ardor, pesadez, reflujo.
        // Solución: Cremas suaves, verduras no ácidas.
        // ======================================================================================

        scenarios.Add(new Scenario {
            clientName = "Ejecutivo con Estrés",
            conditionDescription = "Llevo una semana fatal con el estómago. Siento un ardor constante que sube por el pecho. ¿Qué me recomiendas comer que sea suave?",
            optionA_Text = "Crema de Lentejas Rojas con Espinacas y Verduras",
            optionA_Image = "Images/crema-de-lentejas-rojas",
            optionA_Score = 100,
            optionB_Text = "Pechuga a la Plancha con Langostinos en Tempura",
            optionB_Image = "Images/pechuga_pollo",
            optionB_Score = 50,
            optionC_Text = "Patatas Fritas con Salsa Picante",
            optionC_Image = "Images/cesta-patatas-fritas-salsa-picante_359687-951",
            optionC_Score = 0
        });

        scenarios.Add(new Scenario {
            clientName = "Cantante de Ópera",
            conditionDescription = "Esta noche tengo función y noto el estómago revuelto, como con fuego. Necesito algo que no me repita ni me de acidez. ¿Qué opción es la mejor?",
            optionA_Text = "Crema Verde de Puerros con Guisantes, Espinacas y Patata",
            optionA_Image = "Images/pure-de-espinacas-receta",
            optionA_Score = 100,
            optionB_Text = "Brocheta de Pollo y Tempura de Gambas",
            optionB_Image = "Images/pechuga_pollo",
            optionB_Score = 50,
            optionC_Text = "Cesta de Patatas Bravas muy Picantes",
            optionC_Image = "Images/cesta-patatas-fritas-salsa-picante_359687-951",
            optionC_Score = 0
        });

        scenarios.Add(new Scenario {
            clientName = "Joven con Indigestión",
            conditionDescription = "Me siento súper hinchado después de la comida de ayer. Quiero cenar algo ligero, quizás una ensalada, pero me da miedo que me siente mal. ¿Cuál elijo?",
            optionA_Text = "Ensalada Templada de Garbanzos con Verduras (Pepino, Aguacate)",
            optionA_Image = "Food/ChickpeaSalad",
            optionA_Score = 100,
            optionB_Text = "Pechuga Grillé con Tempura Crujiente",
            optionB_Image = "Images/pechuga_pollo",
            optionB_Score = 50,
            optionC_Text = "Patatas Fritas con Mucha Salsa",
            optionC_Image = "Images/cesta-patatas-fritas-salsa-picante_359687-951",
            optionC_Score = 0
        });

        scenarios.Add(new Scenario {
            clientName = "Anciano Delicado",
            conditionDescription = "Tengo el estómago un poco delicado hoy, hijo. ¿Podrías prepararme una crema que sea fácil de digerir y me asiente el cuerpo?",
            optionA_Text = "Crema de Calabacín, Zanahoria y Jengibre",
            optionA_Image = "Food/ZucchiniGinger",
            optionA_Score = 100,
            optionB_Text = "Pollo a la Plancha y Fritos",
            optionB_Image = "Images/pechuga_pollo",
            optionB_Score = 50,
            optionC_Text = "Chips de Patata al Fuego (Picante)",
            optionC_Image = "Images/cesta-patatas-fritas-salsa-picante_359687-951",
            optionC_Score = 0
        });

        scenarios.Add(new Scenario {
            clientName = "Mujer Embarazada",
            conditionDescription = "Tengo muchísima acidez estos últimos meses. Cualquier cosa fuerte me mata. ¿Tienes algún plato de cuchara que sea muy suave?",
            optionA_Text = "Crema de Lentejas Rojas con Espinacas",
            optionA_Image = "Images/crema-de-lentejas-rojas",
            optionA_Score = 100,
            optionB_Text = "Pechuga y Langostinos Rebozados",
            optionB_Image = "Images/pechuga_pollo",
            optionB_Score = 50,
            optionC_Text = "Patatas Fritas con Tabasco",
            optionC_Image = "Images/cesta-patatas-fritas-salsa-picante_359687-951",
            optionC_Score = 0
        });

        scenarios.Add(new Scenario {
            clientName = "Crítico Gastronómico",
            conditionDescription = "Hoy quiero descansar el paladar y el estómago. Busco algo alcalino, verde y reconfortante para la gastritis. Sorpréndeme.",
            optionA_Text = "Crema Verde de Puerros y Guisantes",
            optionA_Image = "Images/pure-de-espinacas-receta",
            optionA_Score = 100,
            optionB_Text = "Pechuga con Tempura de Aros de Cebolla",
            optionB_Image = "Images/pechuga_pollo",
            optionB_Score = 50,
            optionC_Text = "Patatas con Salsa del Infierno",
            optionC_Image = "Images/cesta-patatas-fritas-salsa-picante_359687-951",
            optionC_Score = 0
        });


        // ======================================================================================
        // CATEGORIA 2: MIGRAÑA / NEURALGIA
        // Sintomas: Dolor de cabeza, fotofobia.
        // Solución: Vitaminas B, Magnesio, Omega 3. Evitar tiramina/histamina.
        // ======================================================================================

        scenarios.Add(new Scenario {
            clientName = "Programadora Senior",
            conditionDescription = "Llevo 12 horas programando y veo luces parpadeantes. Me va a dar una migraña fuerte. ¿Qué debería comer para prevenirlo?",
            optionA_Text = "Huevos Rellenos con Guisantes y Huevo",
            optionA_Image = "Food/StuffedEggs",
            optionA_Score = 100,
            optionB_Text = "Pato a la Naranja con Arroz Integral",
            optionB_Image = "Food/DuckOrange",
            optionB_Score = 50,
            optionC_Text = "Salchichas con Queso y Ketchup en Pan Blanco",
            optionC_Image = "Food/HotDog",
            optionC_Score = 0
        });

        scenarios.Add(new Scenario {
            clientName = "Profesora con Cefalea",
            conditionDescription = "Me palpita la sien derecha y me molesta mucho la luz de la sala. Necesito nutrientes para el cerebro. ¿Qué me sugieres?",
            optionA_Text = "Salmón al Horno con Verduras de Primavera y Quinoa",
            optionA_Image = "Food/SalmonQuinoa",
            optionA_Score = 100,
            optionB_Text = "Magret de Pato con Salsa de Naranja",
            optionB_Image = "Food/DuckOrange",
            optionB_Score = 50,
            optionC_Text = "Hot Dog con Doble de Ketchup y Queso",
            optionC_Image = "Food/HotDog",
            optionC_Score = 0
        });

        scenarios.Add(new Scenario {
            clientName = "Conductor de Autobús",
            conditionDescription = "Tengo la cabeza embotada después del turno. Quiero algo sencillo, tipo una tosta, pero que me ayude con el dolor de cabeza.",
            optionA_Text = "Tostada Integral con Crema de Calabacín y Queso",
            optionA_Image = "Food/ToastZucchini",
            optionA_Score = 100,
            optionB_Text = "Pato Asado con Cítricos y Arroz",
            optionB_Image = "Food/DuckOrange",
            optionB_Score = 50,
            optionC_Text = "Salchichas Frankfurt con Mucho Ketchup",
            optionC_Image = "Food/HotDog",
            optionC_Score = 0
        });

        scenarios.Add(new Scenario {
            clientName = "Artista Visual",
            conditionDescription = "Sufro de neuralgia a menudo. Mi médico dice que coma alimentos ricos en vitaminas y evite los procesados. ¿Qué plato tienes hoy?",
            optionA_Text = "Huevos Rellenos Caseros con Guisantes",
            optionA_Image = "Food/StuffedEggs",
            optionA_Score = 100,
            optionB_Text = "Pato con Naranja Glaseada",
            optionB_Image = "Food/DuckOrange",
            optionB_Score = 50,
            optionC_Text = "Perrito Caliente Industrial con Queso",
            optionC_Image = "Food/HotDog",
            optionC_Score = 0
        });

        scenarios.Add(new Scenario {
            clientName = "Estudiante de Oposiciones",
            conditionDescription = "No me puedo concentrar del dolor de cabeza. Necesito Omega-3 y comida real para seguir estudiando. ¿Cuál es el mejor plato?",
            optionA_Text = "Salmón al Horno con Quinoa y Verduras",
            optionA_Image = "Food/SalmonQuinoa",
            optionA_Score = 100,
            optionB_Text = "Pato a la Naranja (Arroz Integral)",
            optionB_Image = "Food/DuckOrange",
            optionB_Score = 50,
            optionC_Text = "Salchichas Baratas con Ketchup",
            optionC_Image = "Food/HotDog",
            optionC_Score = 0
        });


        // ======================================================================================
        // CATEGORIA 3: DESPUÉS DE HACER DEPORTE
        // Sintomas: Agotamiento, necesidad de recuperación.
        // Solución: Proteína + Carbohidratos complejos (Bowls).
        // ======================================================================================

        scenarios.Add(new Scenario {
            clientName = "Ciclista de Montaña",
            conditionDescription = "Acabo de terminar una ruta de 4 horas por la montaña. Estoy vacía de energía. ¿Qué debería comer para recuperar glucógeno y músculo?",
            optionA_Text = "Tazón de Pollo, Espárragos y Quinoa",
            optionA_Image = "Food/BowlChicken",
            optionA_Score = 100,
            optionB_Text = "Hígado con Cebolla Caramelizada",
            optionB_Image = "Food/LiverOnion",
            optionB_Score = 50,
            optionC_Text = "Pizza con Piña",
            optionC_Image = "Food/PizzaPineapple",
            optionC_Score = 0
        });

        scenarios.Add(new Scenario {
            clientName = "Nadadora de Competición",
            conditionDescription = "Salgo de la piscina muerta de hambre. Necesito grasas saludables. ¿Qué me pones?",
            optionA_Text = "Tazón de Salmón, Calabacín y Nueces",
            optionA_Image = "Food/BowlSalmon",
            optionA_Score = 100,
            optionB_Text = "Plato de Hígado Encebollado",
            optionB_Image = "Food/LiverOnion",
            optionB_Score = 50,
            optionC_Text = "Pizza Hawaiana (Piña y Jamón)",
            optionC_Image = "Food/PizzaPineapple",
            optionC_Score = 0
        });

        scenarios.Add(new Scenario {
            clientName = "Corredor de Maratón",
            conditionDescription = "Mañana tengo carrera y necesito hacer carga de hidratos de calidad, nada de grasas malas. ¿Qué plato me recomiendas?",
            optionA_Text = "Tazón de Pavo, Calabaza y Batata",
            optionA_Image = "Food/BowlTurkey",
            optionA_Score = 100,
            optionB_Text = "Hígado de Ternera con Cebolla",
            optionB_Image = "Food/LiverOnion",
            optionB_Score = 50,
            optionC_Text = "Pizza Familiar con Piña",
            optionC_Image = "Food/PizzaPineapple",
            optionC_Score = 0
        });

        scenarios.Add(new Scenario {
            clientName = "Levantador de Pesas",
            conditionDescription = "Hoy ha sido día de pierna y he roto muchas fibras. Necesito proteínas y hierro a tope. ¿Cuál es la mejor opción?",
            optionA_Text = "Tazón de Pollo, Brócoli y Lentejas",
            optionA_Image = "Food/BowlLentils",
            optionA_Score = 100,
            optionB_Text = "Hígado Frito con Cebolla",
            optionB_Image = "Food/LiverOnion",
            optionB_Score = 50,
            optionC_Text = "Pizza con Trozos de Piña",
            optionC_Image = "Food/PizzaPineapple",
            optionC_Score = 0
        });

        scenarios.Add(new Scenario {
            clientName = "Jugadora de Baloncesto",
            conditionDescription = "Acabo de jugar una prórroga y estoy exhausta. Necesito recuperar sales y fuerzas, pero algo sano. ¿Cuál es la mejor opción?",
            optionA_Text = "Tazón de Pollo, Espárragos y Quinoa",
            optionA_Image = "Food/BowlChicken",
            optionA_Score = 100,
            optionB_Text = "Hígado con Salsa de Cebolla",
            optionB_Image = "Food/LiverOnion",
            optionB_Score = 50,
            optionC_Text = "Pizza Hawaiana",
            optionC_Image = "Food/PizzaPineapple",
            optionC_Score = 0
        });

        scenarios.Add(new Scenario {
            clientName = "Entrenador Personal",
            conditionDescription = "Siempre digo a mis clientes que la comida post-entreno es sagrada. Hoy me toca a mí pedir consejo. ¿Cuál es la mejor opción?",
            optionA_Text = "Tazón de Pavo, Calabaza y Batata",
            optionA_Image = "Food/BowlTurkey",
            optionA_Score = 100,
            optionB_Text = "Hígado Encebollado",
            optionB_Image = "Food/LiverOnion",
            optionB_Score = 50,
            optionC_Text = "Pizza con Piña en Almíbar",
            optionC_Image = "Food/PizzaPineapple",
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
        if (container == null)
        {
            GenerarCocina2D();
            container = GameObject.Find("COCINA_2D");
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
        GenerarCocina2D_Internal(false);
    }

    private void GenerarCocina2D_Internal(bool soloArquitectura)
    {
        Debug.Log(soloArquitectura ? ">>> GENERANDO ARQUITECTURA... <<<<" : ">>> GENERANDO COCINA COMPLETA... <<<<");

        GameObject oldRoot = GameObject.Find("COCINA_2D");
        if (oldRoot != null) DestroySafe(oldRoot);
        
        GameObject root = new GameObject("COCINA_2D");
        restauranteContainer = root;

        // --- SUB-CARPETAS (Locales para no ensuciar el global) ---
        Transform folderStructure = new GameObject("_Estructura").transform; folderStructure.SetParent(root.transform);
        Transform folderZones = new GameObject("_Zonas_Trabajo").transform; folderZones.SetParent(root.transform);
        Transform folderAppliances = new GameObject("_Electrodomesticos").transform; folderAppliances.transform.SetParent(root.transform);
        Transform folderDecor = new GameObject("_Decoracion").transform; folderDecor.transform.SetParent(root.transform);
        Transform folderLights = new GameObject("_Iluminacion").transform; folderLights.transform.SetParent(root.transform);

        // 1. CÁMARA & ILUMINACIÓN
        SetupCameraRealist(new Vector3(0, 18, -16), 42);
        
        GameObject sun = new GameObject("LuzGlobal"); sun.transform.SetParent(folderLights);
        Light l = sun.AddComponent<Light>(); l.type = LightType.Directional; l.intensity = 1.2f;
        sun.transform.rotation = Quaternion.Euler(50, -30, 0);

        CreatePointLight(new Vector3(-10, 5, 5), new Color(0.5f, 0.8f, 1f), 15, 0.5f, folderLights);
        CreatePointLight(new Vector3(10, 5, 5), new Color(1f, 0.9f, 0.7f), 15, 0.5f, folderLights);

        // 2. ARQUITECTURA
        int width = 32; int depth = 22;
        CreateBlock("SueloMaster", Vector3.zero, new Vector3(width, 0.1f, depth), new Color(0.05f, 0.05f, 0.07f), folderStructure);

        float wallH = 7f;
        CreateWall("MuroFondo", new Vector3(0, wallH/2, 11), new Vector3(width, wallH, 0.5f), folderStructure);
        GenerarVentanalPanoramico(new Vector3(-9, wallH/2, 10.7f), folderStructure);
        GenerarVentanalPanoramico(new Vector3(9, wallH/2, 10.7f), folderStructure);
        GenerarMuroListones(new Vector3(-16, wallH/2, 0), depth, wallH, folderStructure); 
        CrearMuroRealista(new Vector3(16, wallH/2, 0), new Vector3(0.5f, wallH, depth), new Color(0.1f, 0.1f, 0.12f), folderStructure);

        if (soloArquitectura) {
            Debug.Log(">>> ARQUITECTURA GENERADA.");
            return;
        }

        // 3. ZONAS DE TRABAJO (Usando carpetas específicas)
        GenerarEncimeraBicolor_WithParent(new Vector3(-10, 0, 8), 8, folderZones);
        GenerarPlacaInduccion_WithParent(new Vector3(-10, 0, 8), folderZones);
        
        GenerarEncimeraBicolor_WithParent(new Vector3(10, 0, 8), 8, folderZones);
        GenerarFregaderoRealista_WithParent(new Vector3(10, 0, 8), folderZones); 
        
        // El fregadero ahora tiene un nombre predecible para buscarlo
        GameObject faucet = GameObject.Find("GrifoCisne");
        if(faucet != null) {
            if (faucet.GetComponent<DispenserStation>() == null) {
                DispenserStation ds = faucet.AddComponent<DispenserStation>();
                ds.ingredientName = "Agua";
            }
            CrearTextoInWorld(faucet.transform, "AGUA", new Vector3(0, 1.2f, 0));
        }

        GenerarIslaWaterfall_WithParent(new Vector3(0, 0, 1), 9, 4.5f, folderZones); 
        GameObject cBoard = CreateBlock("TablaCorte_Interact", new Vector3(-2, 1.25f, 1), new Vector3(1.2f, 0.05f, 1.0f), Color.white, folderZones);
        if (cBoard.GetComponent<CuttingStation>() == null) cBoard.AddComponent<CuttingStation>();
        CrearTextoInWorld(cBoard.transform, "CORTE", new Vector3(0, 0.8f, 0));

        GameObject bin = CreateBlock("Basura_Pro", new Vector3(-14, 0.6f, -9), new Vector3(0.8f, 1.2f, 0.8f), new Color(0.2f, 0.2f, 0.2f), folderZones);
        if (bin.GetComponent<TrashStation>() == null) bin.AddComponent<TrashStation>();
        CrearTextoInWorld(bin.transform, "BASURA", new Vector3(0, 1.2f, 0));

        GameObject delivBar = CreateBlock("Entrega_Pro", new Vector3(-3, 0.6f, -7), new Vector3(8, 1.2f, 0.8f), new Color(0.1f, 0.1f, 0.1f), folderZones);
        if (delivBar.GetComponent<DeliveryStation>() == null) delivBar.AddComponent<DeliveryStation>();
        CrearTextoInWorld(delivBar.transform, "ENTREGA", new Vector3(0, 1.2f, -0.5f));

        // 4. ELECTRODOMESTICOS
        GenerarTorreHornos_WithParent(new Vector3(-14, 0, 8), folderAppliances);
        GenerarNeveraAmericana_WithParent(new Vector3(14, 0, 8), folderAppliances);

        // 5. DECORACIÓN
        for(int i=-2; i<=2; i++) GenerarSillaDiseno_WithParent(new Vector3(i*2.5f, 0.8f, -8.5f), 180, new Color(0.7f, 0.6f, 0.2f), folderDecor);
        GenerarPlantaGrande_WithParent(new Vector3(-14, 0, -10f), folderDecor);
        GenerarPlantaGrande_WithParent(new Vector3(14, 0, -10f), folderDecor);

        Debug.Log(">>> COCINA GENERADA COMPLETAMENTE SIN ERRORES.");
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
        CrearTextoInWorld(npc.transform, "CLIENTE", new Vector3(0, 1.2f, 0));
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
    
    void GenerarLamparaColgante(Vector3 pos)
    {
        // Cable
        CreateBlock("Cable", pos + Vector3.up*1f, new Vector3(0.02f, 2f, 0.02f), Color.black);
        // Pantalla media esfera
        GameObject hem = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hem.transform.position = pos;
        hem.transform.localScale = Vector3.one * 1.2f;
        hem.GetComponent<Renderer>().material.color = new Color(0.8f, 0.8f, 0.8f); // Plata mate
        hem.transform.SetParent(restauranteContainer.transform);
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

    void GenerarBowlFrutas(Vector3 pos)
    {
        GameObject bowl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bowl.transform.SetParent(restauranteContainer.transform);
        bowl.transform.position = pos;
        bowl.transform.localScale = new Vector3(0.6f, 0.3f, 0.6f);
        bowl.GetComponent<Renderer>().material.color = new Color(0.6f, 0.4f, 0.2f); // Madera

        // Frutas (naranjas de la foto)
        for(int i=0; i<3; i++) {
            GameObject fruit = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fruit.transform.SetParent(bowl.transform);
            fruit.transform.localPosition = new Vector3(Mathf.Sin(i*2)*0.3f, 0.4f, Mathf.Cos(i*2)*0.3f);
            fruit.transform.localScale = Vector3.one * 0.4f;
            fruit.GetComponent<Renderer>().material.color = new Color(1f, 0.5f, 0f); // Naranja
        }
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
        
        CrearTextoInWorld(obj.transform, "CAMARA", new Vector3(0, 1.5f, 0));
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
        CrearTextoInWorld(rack.transform, ingredient.ToUpper(), new Vector3(0, 1.5f, 0));

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
        
        CrearTextoInWorld(bin.transform, "BASURA", new Vector3(0, 1.2f, 0));
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
    // AQUI EMPIEZA LA GENERACION DEL RESTAURANTE (EL CODIGO ORIGINAL QUE YA FUNCIONABA)
    // ==============================================================================================

    [ContextMenu("Generar Restaurante Original")]
    [ContextMenu("✨ GENERAR RESTAURANTE PREMIUM")]
    public void GenerarRestaurante()
    {
        Debug.Log(">>> TRANSFORMANDO RESTAURANTE A NIVEL PREMIUM... <<<<");

        GameObject oldRoot = GameObject.Find("RESTAURANTE_ESTRUCTURA");
        if (oldRoot != null) DestroySafe(oldRoot);
        
        GameObject root = new GameObject("RESTAURANTE_ESTRUCTURA");
        restauranteContainer = root;

        // --- SUB-CARPETAS ---
        Transform folderStructure = new GameObject("_Arquitectura").transform; folderStructure.SetParent(root.transform);
        Transform folderDining = new GameObject("_Sala_Comedor").transform; folderDining.SetParent(root.transform);
        Transform folderStaff = new GameObject("_Zonas_Privadas").transform; folderStaff.transform.SetParent(root.transform);
        Transform folderAmbience = new GameObject("_Ambientacion").transform; folderAmbience.SetParent(root.transform);

        // 1. ILUMINACIÓN & CÁMARA
        SetupCameraRealist(new Vector3(0, 15, -20), 45);
        GameObject sun = new GameObject("Luz_Ambiente"); sun.transform.SetParent(folderAmbience);
        Light l = sun.AddComponent<Light>(); l.type = LightType.Directional; l.intensity = 1.1f;
        sun.transform.rotation = Quaternion.Euler(45, -45, 0);

        // Luces puntuales decorativas
        CreatePointLight(new Vector3(-5, 6, -5), new Color(1f, 0.9f, 0.7f), 15, 0.8f, folderAmbience);
        CreatePointLight(new Vector3(15, 6, -5), new Color(1f, 0.9f, 0.7f), 15, 0.8f, folderAmbience);

        // 2. ARQUITECTURA (Suelo y Paredes)
        // Suelo Principal: Moqueta Azul Noche Profundo
        CreateBlock("Suelo_Master", new Vector3(8, -0.05f, 15), new Vector3(50, 0.1f, 80), new Color(0.1f, 0.12f, 0.15f), folderStructure);
        
        // Muros Maestros
        float h = 9f;
        CreateWall("Muro_Fondo", new Vector3(8, h/2, 55), new Vector3(50, h, 1), folderStructure);
        CreateWall("Muro_Frontal_L", new Vector3(-8, h/2, -25), new Vector3(18, h, 1), folderStructure);
        CreateWall("Muro_Frontal_R", new Vector3(25, h/2, -25), new Vector3(16, h, 1), folderStructure);

        // 3. ZONA DE COMEDOR (Luxury Dining)
        // Mesas de Mármol con Sillas Premium
        for(int x=0; x<3; x++) {
            for(int z=0; z<4; z++) {
                Vector3 posMesa = new Vector3(-5 + x * 12, 0, -10 + z * 14);
                GenerarMesaMarmol(posMesa, folderDining);
                GenerarSillaPremium(posMesa + Vector3.back * 1.8f, 0, folderDining);
                GenerarSillaPremium(posMesa + Vector3.forward * 1.8f, 180, folderDining);
            }
        }

        // 4. BARRA DE SERVICIO & RECEPCIÓN
        // Barra Principal
        GameObject bar = CreateBlock("Barra_Recepcion", new Vector3(5, 0.6f, -24), new Vector3(12, 1.2f, 1.5f), new Color(0.15f, 0.15f, 0.18f), folderStaff);
        bar.AddComponent<DeliveryStation>();
        CrearTextoInWorld(bar.transform, "RECEPCIÓN / ENTREGA", new Vector3(0, 1.2f, 0));

        // 5. ESTACIONES DE TRABAJO (Integradas en la arquitectura)
        GameObject bin = CreateBlock("Basura_Inox", new Vector3(-14, 0.6f, -22), new Vector3(1, 1.2f, 1), Color.gray, folderStaff);
        bin.AddComponent<TrashStation>();
        CrearTextoInWorld(bin.transform, "DEPÓSITO", new Vector3(0, 1.2f, 0));

        GameObject prep = CreateBlock("Mesa_Corte_Pub", new Vector3(25, 0.6f, -22), new Vector3(3, 1.2f, 2), Color.white, folderStaff);
        prep.AddComponent<CuttingStation>();
        CrearTextoInWorld(prep.transform, "PREPARACIÓN", new Vector3(0, 1.2f, 0));

        // 6. AMBIENTACIÓN (Plantas y Columnas)
        for(int i=0; i<4; i++) {
            GenerarPlantaGrande_WithParent(new Vector3(-16, 0, -15 + i*20), folderAmbience);
            GenerarPlantaGrande_WithParent(new Vector3(32, 0, -15 + i*20), folderAmbience);
        }

        Debug.Log(">>> RESTAURANTE PREMIUM GENERADO CON ÉXITO.");
        if (Application.isPlaying) ShowRound();
    }

    void GenerarMesaMarmol(Vector3 pos, Transform parent)
    {
        GameObject mesa = new GameObject("Mesa_Marmol");
        mesa.transform.SetParent(parent);
        mesa.transform.position = pos;
        
        // Pata central
        CreateBlock("Pata", pos + Vector3.up * 0.45f, new Vector3(0.3f, 0.9f, 0.3f), Color.black, mesa.transform);
        // Sobre de Mármol
        CreateBlock("Sobre", pos + Vector3.up * 0.95f, new Vector3(2.5f, 0.1f, 2.5f), new Color(0.95f, 0.95f, 0.95f), mesa.transform);
    }

    void GenerarSillaPremium(Vector3 pos, float rotY, Transform parent)
    {
        GameObject silla = new GameObject("Silla_Velvet");
        silla.transform.SetParent(parent);
        silla.transform.position = pos;
        silla.transform.rotation = Quaternion.Euler(0, rotY, 0);

        // Asiento
        CreateBlock("Asiento", pos + Vector3.up * 0.4f, new Vector3(1f, 0.2f, 1f), new Color(0.4f, 0.1f, 0.1f), silla.transform);
        // Respaldo
        CreateBlock("Respaldo", pos + Vector3.up * 0.9f + silla.transform.forward * -0.4f, new Vector3(1f, 0.8f, 0.15f), new Color(0.4f, 0.1f, 0.1f), silla.transform);
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
        // 1. Intentar cargar como Sprite (Ideal)
        Sprite s = Resources.Load<Sprite>(path);
        if (s != null) return s;

        // 2. Intentar cargar como Texture2D (Si la importación no está en modo Sprite)
        Texture2D t = Resources.Load<Texture2D>(path);
        if (t != null)
        {
             return Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f));
        }

        // 3. Fallback: Generar un cuadrado de color para que no se rompa la UI
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

    void CreateGlassWindow(string name, Vector3 pos, Vector3 scale, Transform parent)
    {
        GameObject win = GameObject.CreatePrimitive(PrimitiveType.Cube);
        win.name = name;
        win.transform.SetParent(parent);
        win.transform.position = pos;
        win.transform.localScale = scale;
        
        Renderer r = win.GetComponent<Renderer>();
        // Configurar material transparente
        r.material = new Material(Shader.Find("Standard")); 
        r.material.color = new Color(0.7f, 0.9f, 1f, 0.3f); 
        r.material.SetFloat("_Mode", 3); // Transparent
        r.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        r.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        r.material.SetInt("_ZWrite", 0);
        r.material.DisableKeyword("_ALPHATEST_ON");
        r.material.EnableKeyword("_ALPHABLEND_ON");
        r.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        r.material.renderQueue = 3000;
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
