using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using System.Collections;
using System.Collections.Generic;

// ESTE SCRIPT HACE TODO: CARGA, MENU Y JUEGO.
public class KitchenBootstrap : MonoBehaviour
{
    private GameObject menuCanvas;
    private GameObject currentMenuPanel;
    public GameObject restauranteContainer; 

    // --- AUTO-EJECT (Asegura que el script corra sí o sí) ---
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoStartGlobal()
    {
        if (FindObjectOfType<KitchenBootstrap>() == null)
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
            if (FindObjectOfType<EventSystem>() == null)
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

        // Caso 1: ACIDEZ
        scenarios.Add(new Scenario {
            clientName = "Cliente con Acidez",
            conditionDescription = "Tengo un ardor de estómago terrible... necesito algo que no me haga daño.",
            // Correcto 100: Avena/Pollo suave
            optionA_Text = "Pechuga de Pollo Hervida y Manzana", 
            optionA_Image = "Food/ChickenApple", 
            optionA_Score = 100,
            // Normal 50: Pan
            optionB_Text = "Pan Tostado", 
            optionB_Image = "Food/Toast", 
            optionB_Score = 50,
            // Mal 0: Picante/Grasa
            optionC_Text = "Tacos Picantes con Salsa", 
            optionC_Image = "Food/SpicyTacos", 
            optionC_Score = 0
        });

        // Caso 2: MIGRAÑAS
        scenarios.Add(new Scenario {
            clientName = "Cliente con Migraña",
            conditionDescription = "Me estalla la cabeza y me molesta la luz... ¿qué puedo comer?",
            // Correcto 100: Espinacas/Magnesio
            optionA_Text = "Ensalada de Espinacas y Salmón", 
            optionA_Image = "Food/SpinachSalmon", 
            optionA_Score = 100,
            // Normal 50: Neutro
            optionB_Text = "Arroz Blanco", 
            optionB_Image = "Food/Rice", 
            optionB_Score = 50,
            // Mal 0: Queso/Vino (Tiramina)
            optionC_Text = "Tabla de Quesos Curados y Vino", 
            optionC_Image = "Food/CheeseWine", 
            optionC_Score = 0
        });

        // Caso 3: DEPORTE
        scenarios.Add(new Scenario {
            clientName = "Atleta Agotado",
            conditionDescription = "Acabo de terminar una maratón y mis músculos no responden.",
            // Correcto 100: Potasio/Carbohidratos/Proteina
            optionA_Text = "Pasta Integral y Plátano", 
            optionA_Image = "Food/PastaBanana", 
            optionA_Score = 100,
            // Normal 50: Agua (Hidrata pero falta comida)
            optionB_Text = "Solo Agua Mineral", 
            optionB_Image = "Food/Water", 
            optionB_Score = 50,
            // Mal 0: Grasa mala
            optionC_Text = "Hamburguesa Doble con Bacon", 
            optionC_Image = "Food/BurgerBacon", 
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
        if (currentMenuPanel != null) Destroy(currentMenuPanel);

        // 1. Fondo Corporativo (Azul noche profundo o Gris oscuro)
        currentMenuPanel = CrearPanel(new Color(0.1f, 0.12f, 0.15f)); 
        
        // 2. HEADER - Título y Subtítulo
        GameObject headerContainer = new GameObject("Header");
        headerContainer.transform.SetParent(currentMenuPanel.transform, false);
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
        buttonContainer.transform.SetParent(currentMenuPanel.transform, false);
        RectTransform rtContainer = buttonContainer.AddComponent<RectTransform>();
        // Anclar al centro de la pantalla
        rtContainer.anchorMin = new Vector2(0.5f, 0.5f); 
        rtContainer.anchorMax = new Vector2(0.5f, 0.5f);
        rtContainer.anchoredPosition = new Vector2(0, -50); 
        rtContainer.sizeDelta = new Vector2(500, 400);

        // --- OPCIÓN 1: INICIO DE JUEGO ---
        CrearBotonModerno(buttonContainer.transform, "INICIO DE JUEGO", 0, 100, new Color(0.2f, 0.6f, 0.3f), () => {
            Destroy(menuCanvas); 
            // ShowRound(); // Card game disabled
            GenerarCocina2D(); // Start 2D Top-Down Game
        });

        // --- OPCIÓN 2: MANUAL DE INSTRUCCIONES ---
        CrearBotonModerno(buttonContainer.transform, "MANUAL DE INSTRUCCIONES", 0, 0, new Color(0.2f, 0.4f, 0.6f), MostrarInstrucciones);

        // --- OPCIÓN 3: CREAR USUARIO ---
        CrearBotonModerno(buttonContainer.transform, "CREAR USUARIO", 0, -100, new Color(0.5f, 0.3f, 0.6f), MostrarCrearUsuario);
        
        // Footer
        GameObject footer = CrearTexto(currentMenuPanel.transform, "© 2026 Enterprise Solutions", 0, -300, 12, Color.gray);
        RectTransform rtFooter = footer.GetComponent<RectTransform>();
        rtFooter.anchorMin = new Vector2(0.5f, 0.05f); rtFooter.anchorMax = new Vector2(0.5f, 0.05f);
        rtFooter.anchoredPosition = Vector2.zero;
    }

    void MostrarInstrucciones()
    {
        if (currentMenuPanel != null) Destroy(currentMenuPanel);
        currentMenuPanel = CrearPanel(new Color(0.15f, 0.2f, 0.25f)); // Azul acero

        CrearTexto(currentMenuPanel.transform, "MANUAL OPERATIVO", 0, 200, 50, Color.white);

        string manual = 
            "- PROTOCOLO DE ACIDEZ: Servir alimentos alcalinos (Pollo, Avena).\n\n" +
            "- PROTOCOLO DE MIGRAÑA: Evitar tiramina. Priorizar Espinacas/Magnesio.\n\n" +
            "- PROTOCOLO DEPORTIVO: Requerimiento de Potasio y Carbohidratos (Plátano, Pasta).\n\n" +
            "OBJETIVO: Maximizar satisfacción del cliente (100 pts) para evitar sanciones.";

        GameObject txt = CrearTexto(currentMenuPanel.transform, manual, 0, 0, 24, new Color(0.9f, 0.9f, 0.9f));
        txt.GetComponent<RectTransform>().sizeDelta = new Vector2(900, 400); // Dar espacio para leer

        CrearBotonModerno(currentMenuPanel.transform, "VOLVER AL MENÚ", 0, -200, new Color(0.6f, 0.2f, 0.2f), MostrarMenuPrincipal);
    }

    void MostrarCrearUsuario()
    {
        if (currentMenuPanel != null) Destroy(currentMenuPanel);
        currentMenuPanel = CrearPanel(new Color(0.2f, 0.15f, 0.25f)); // Púrpura oscuro corporativo

        // Header
        GameObject header = CrearTexto(currentMenuPanel.transform, "REGISTRO DE PERSONAL", 0, 150, 50, Color.white);
        header.GetComponent<Text>().fontStyle = FontStyle.Bold;
        
        CrearTexto(currentMenuPanel.transform, "Identificación del Chef Ejecutivo", 0, 100, 20, new Color(0.8f, 0.8f, 0.8f));

        // -- INPUT FIELD CONTAINER --
        GameObject inputObj = new GameObject("InputField_Name");
        inputObj.transform.SetParent(currentMenuPanel.transform, false);
        
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
    // GENERACIÓN DE COCINA TOP-DOWN "REPLICA EXACTA" (MODERN DARK + ISLA)
    // ==============================================================================================

    public void GenerarCocina2D()
    {
        Debug.Log(">>> GENERANDO COCINA TOP-DOWN MODERN DARK... <<<<");

        string containerName = "COCINA_2D";
        GameObject old = GameObject.Find(containerName);
        if (old != null) Destroy(old);

        restauranteContainer = new GameObject(containerName);
        
        // 1. CÁMARA & LUZ (VISTA CENITAL)
        SetupCameraTopDown();
        
        GameObject lightObj = new GameObject("LuzSol");
        lightObj.transform.SetParent(restauranteContainer.transform);
        Light l = lightObj.AddComponent<Light>(); 
        l.type = LightType.Directional;
        // Eliminamos la isla central para dejar la "U" abierta y limpia como en muchas fotos de diseño moderno.

        // 5. JUGADOR
        SpawnPlayerTopDown(new Vector3(0, 0.1f, -3)); 
    }

    // --- GENERADORES ESPECÍFICOS REPLICA ---

    void GenerarVentanalPanoramico(Vector3 pos)
    {
        // Marco minimalista Negro
        CreateBlock("MarcoSup", pos + Vector3.up * 1.5f, new Vector3(10f, 0.2f, 0.2f), Color.black);
        CreateBlock("MarcoInf", pos - Vector3.up * 1.5f, new Vector3(10f, 0.2f, 0.2f), Color.black);
        // Cristal único grande
        GameObject glass = CreateBlock("Cristal", pos, new Vector3(9.8f, 2.8f, 0.1f), new Color(0.8f, 0.9f, 1f, 0.3f));
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
        // Grifo Negro Mate moderno
        GameObject tap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tap.transform.position = pos + new Vector3(0, 1.0f, 0.8f);
        tap.transform.localScale = new Vector3(0.1f, 0.8f, 0.1f);
        tap.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.1f);
        tap.transform.SetParent(restauranteContainer.transform);
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

    void GenerarMacetaHierbas(Vector3 pos)
    {
        GameObject p = CreateBlock("Pot", pos, new Vector3(0.5f, 0.4f, 0.5f), Color.white);
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        g.transform.position = pos + Vector3.up * 0.4f;
        g.transform.localScale = Vector3.one * 0.6f;
        g.GetComponent<Renderer>().material.color = new Color(0.2f, 0.7f, 0.2f); // Verde vivo
        g.transform.SetParent(p.transform);
    }

    void GenerarBowlMadera(Vector3 pos, string ing, Color c)
    {
        GameObject b = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        b.transform.position = pos;
        b.transform.localScale = new Vector3(0.5f, 0.25f, 0.5f);
        b.GetComponent<Renderer>().material.color = new Color(0.6f, 0.4f, 0.2f); // Madera
        b.transform.SetParent(restauranteContainer.transform);

        GameObject content = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        content.transform.position = pos + Vector3.up * 0.1f;
        content.transform.localScale = new Vector3(0.4f, 0.2f, 0.4f);
        content.GetComponent<Renderer>().material.color = c;
        content.transform.SetParent(b.transform);

        DispenserStation ds = b.AddComponent<DispenserStation>();
        ds.ingredientName = ing;
        CrearTextoInWorld(b.transform, ing, Vector3.up * 1f);
    }

    void GenerarTablaCortar(Vector3 pos)
    {
        GameObject t = CreateBlock("Tabla", pos, new Vector3(1.2f, 0.05f, 0.8f), new Color(0.8f, 0.6f, 0.4f)); // Bamboo
        t.AddComponent<CuttingStation>();
        CrearTextoInWorld(t.transform, "CORTAR", Vector3.up * 0.5f);
    }


    // --- HELPERS ---
    void CrearSueloBaldosasBlancas(int w, int d)
    {
        // Base blanca
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "SueloBaldosas";
        floor.transform.SetParent(restauranteContainer.transform);
        floor.transform.localScale = new Vector3(w/10f, 1, d/10f);
        floor.GetComponent<Renderer>().material.color = new Color(0.95f, 0.95f, 0.95f);
        
        // Grid Negro muy fino
        for(int x=-w/2; x<=w/2; x+=2) {
             GameObject l = GameObject.CreatePrimitive(PrimitiveType.Cube);
             l.transform.SetParent(restauranteContainer.transform);
             l.transform.position = new Vector3(x, 0.01f, 0);
             l.transform.localScale = new Vector3(0.02f, 0.01f, d);
             l.GetComponent<Renderer>().material.color = new Color(0.8f, 0.8f, 0.8f);
        }
        for(int z=-d/2; z<=d/2; z+=2) {
             GameObject l = GameObject.CreatePrimitive(PrimitiveType.Cube);
             l.transform.SetParent(restauranteContainer.transform);
             l.transform.position = new Vector3(0, 0.01f, z);
             l.transform.localScale = new Vector3(w, 0.01f, 0.02f);
             l.GetComponent<Renderer>().material.color = new Color(0.8f, 0.8f, 0.8f);
        }
    }
    
    void CrearMuroLiso(Vector3 pos, Vector3 s)
    {
        CreateBlock("Muro", pos, s, new Color(0.9f, 0.9f, 0.92f));
    }

    // --- SETUP VISUAL ---

    void SetupCameraTopDown()
    {
        GameObject camObj;
        if (Camera.main != null) camObj = Camera.main.gameObject;
        else { camObj = new GameObject("Main Camera"); camObj.AddComponent<Camera>(); camObj.tag = "MainCamera"; }
        
        camObj.transform.SetParent(restauranteContainer.transform);
        camObj.transform.position = new Vector3(0, 25, -2); 
        camObj.transform.rotation = Quaternion.Euler(85, 0, 0); 
        
        Camera cam = camObj.GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 11f; 
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.12f); 
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

    void CrearTecho(int width, int depth)
    {
        GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ceiling.name = "Techo";
        ceiling.transform.SetParent(restauranteContainer.transform);
        ceiling.transform.position = new Vector3(0, 5, 0); // Alto
        ceiling.transform.rotation = Quaternion.Euler(270, 0, 0); // Mirando abajo
        ceiling.transform.localScale = new Vector3(width, depth, 1);
        ceiling.GetComponent<Renderer>().material.color = new Color(0.9f, 0.9f, 0.9f); // Blanco sucio
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

    // Utileria Visual
    void CreateArco(Transform parent, Vector3 center, float height, float width, Color c)
    {
        // Simple representación visual
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
    GameObject CreateBlock(string name, Vector3 pos, Vector3 scale, Color c)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.SetParent(restauranteContainer.transform);
        obj.transform.position = pos;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().material.color = c;
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

    public void GenerarRestaurante()
    {
        Debug.Log(">>> GENERANDO RESTAURANTE... <<<<");

        string containerName = "RESTAURANTE_GENERADO_AUTOMATICAMENTE";
        
        GameObject old = GameObject.Find(containerName);
        if (old != null) Destroy(old);

        // Assign to the public field so the Editor script can see it
        restauranteContainer = new GameObject(containerName);
        GameObject container = restauranteContainer; // Local reference for existing code compatibility
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

        // 2. SUELO GENERAL Y ZONIFICACIÓN
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Suelo_Principal";
        floor.transform.SetParent(container.transform);
        floor.transform.localScale = new Vector3(10, 1, 10.5f);
        floor.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.12f);

        // Alfombra de Lujo en Comedor (Desplazada hacia atrás)
        GameObject rug = GameObject.CreatePrimitive(PrimitiveType.Plane);
        rug.name = "Alfombra_Comedor"; rug.transform.SetParent(container.transform);
        rug.transform.position = new Vector3(0, 0.05f, -27); rug.transform.localScale = new Vector3(5, 1, 3.5f);
        rug.GetComponent<Renderer>().material.color = new Color(0.3f, 0.1f, 0.1f);

        // 3. MUROS EXTERIORES PANORÁMICOS
        // Muro de Fondo (Sólido)
        CreateWall("Muro_Fondo", new Vector3(0, 4, 55), new Vector3(65, 8, 1), container.transform);
        
        // Muros Frontales
        CreateWall("Muro_Frontal_L", new Vector3(-18, 4, -50), new Vector3(28, 8, 1), container.transform);
        CreateWall("Muro_Frontal_R", new Vector3(18, 4, -50), new Vector3(28, 8, 1), container.transform);
        CreateWall("Muro_Frontal_Top", new Vector3(0, 6.5f, -50), new Vector3(8, 3, 1), container.transform);
        // CreateDoor("Puerta_Principal", new Vector3(0, 2.5f, -50.1f), new Vector3(8.5f, 5, 0.25f), new Color(0.4f, 0.2f, 0.1f), container.transform);

        // MUROS LATERALES CON VENTANALES PANORÁMICOS
        float[] sideX = { -32f, 32f };
        foreach(float x in sideX) {
            string sideName = x < 0 ? "Izq" : "Der";
            // Zócalo inferior
            CreateWall("Zocalo_" + sideName, new Vector3(x, 1f, 2.25f), new Vector3(1, 2, 104.5f), container.transform);
            // Parte superior
            CreateWall("Dintel_" + sideName, new Vector3(x, 7.5f, 2.25f), new Vector3(1, 1, 104.5f), container.transform);
            
            // Pilares y Cristal
            for(int i=0; i<6; i++) {
                float zPillar = -50 + (i * 20.9f);
                CreateWall("Pilar_" + sideName + "_" + i, new Vector3(x, 4.5f, zPillar), new Vector3(1.1f, 5f, 1.5f), container.transform);
                if(i < 5) {
                    float zWin = zPillar + 10.45f;
                    CreateGlassWindow("Ventana_" + sideName + "_" + i, new Vector3(x, 4.5f, zWin), new Vector3(0.2f, 5f, 19.4f), container.transform);
                }
            }
        }

        // Suelo Exterior (Para que se vea algo por las ventanas)
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Exterior_Ground"; ground.transform.position = new Vector3(0, -0.1f, 0);
        ground.transform.localScale = new Vector3(20, 1, 20); ground.GetComponent<Renderer>().material.color = new Color(0.2f, 0.3f, 0.2f);


        // 4. BAÑO REAL Y BONITO (A la izquierda - TOTALMENTE CERRADO)
        GameObject bathroom = new GameObject("Area_Baño");
        bathroom.transform.SetParent(container.transform);
        
        // Game Loop
        ShowRound();
    }

    void ShowRound()
    {
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

        // Opcion A
        CrearCartaOpcion(optionsArea.transform, 0, "A", current.optionA_Text, current.optionA_Image, () => CheckAnswer(current.optionA_Score));
        // Opcion B
        CrearCartaOpcion(optionsArea.transform, 1, "B", current.optionB_Text, current.optionB_Image, () => CheckAnswer(current.optionB_Score));
        // Opcion C
        CrearCartaOpcion(optionsArea.transform, 2, "C", current.optionC_Text, current.optionC_Image, () => CheckAnswer(current.optionC_Score));
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
        if(gamePanel != null) Destroy(gamePanel);
        // Fondo resultado
        gamePanel = CrearPanel(score == 100 ? new Color(0.2f, 0.6f, 0.2f) : (score == 50 ? new Color(0.8f, 0.8f, 0.2f) : new Color(0.8f, 0.2f, 0.2f)));

        string msg = "";
        if (score == 100) msg = "¡HAS ESCOGIDO BIEN! (+100)";
        else if (score == 50) msg = "REGULAR (+50)";
        else msg = "ELECCIÓN INCORRECTA (+0)";

        CrearTexto(gamePanel.transform, msg, 0, 50, 60, Color.white);
        
        CrearBoton(gamePanel.transform, "SIGUIENTE PLATO >>", 0, -100, Color.white, ShowRound);
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
        if(menuCanvas == null) {
            menuCanvas = new GameObject("CanvasMenu");
            Canvas c = menuCanvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = menuCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            menuCanvas.AddComponent<GraphicRaycaster>();
            
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
        GameObject p = new GameObject("Panel");
        p.transform.SetParent(menuCanvas.transform, false);
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
}
