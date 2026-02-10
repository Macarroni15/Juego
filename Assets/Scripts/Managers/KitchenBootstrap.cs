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

    // --- SECUENCIA DE INICIO ---
    IEnumerator SecuenciaInicio()
    {
        CrearCanvas();

        // PANTALLA DE CARGA
        GameObject panelCarga = CrearPanel(Color.black);
        CrearTexto(panelCarga.transform, "CARGANDO...", 0, 50, 50, Color.white);
        
        GameObject barra = CrearBarraCarga(panelCarga.transform);
        Slider slider = barra.GetComponent<Slider>();
        
        float tiempo = 0f;
        while(tiempo < 2f) // Simular carga
        {
            tiempo += Time.deltaTime;
            slider.value = tiempo / 2f;
            yield return null;
        }

        Destroy(panelCarga);
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

    GameObject CrearBarraCarga(Transform parent)
    {
        GameObject b = new GameObject("Barra"); 
        b.transform.SetParent(parent, false);
        Slider s = b.AddComponent<Slider>();
        
        RectTransform r = b.GetComponent<RectTransform>();
        r.sizeDelta = new Vector2(500, 30); 
        r.anchoredPosition = new Vector2(0, -50);
        
        // Fondo
        GameObject fondo = new GameObject("Fondo"); 
        fondo.transform.SetParent(b.transform, false);
        Image imgF = fondo.AddComponent<Image>(); 
        imgF.color = Color.gray;
        RectTransform rf = fondo.GetComponent<RectTransform>();
        rf.anchorMin = Vector2.zero; rf.anchorMax = Vector2.one; rf.sizeDelta = Vector2.zero;
        
        // Fill Area
        GameObject fillArea = new GameObject("FillArea"); 
        fillArea.transform.SetParent(b.transform, false);
        RectTransform rfa = fillArea.AddComponent<RectTransform>();
        rfa.anchorMin = Vector2.zero; rfa.anchorMax = Vector2.one; 
        rfa.offsetMin = new Vector2(5,5); rfa.offsetMax = new Vector2(-5,-5);

        // Fill
        GameObject fill = new GameObject("Fill"); 
        fill.transform.SetParent(fillArea.transform, false);
        Image imgFill = fill.AddComponent<Image>(); 
        imgFill.color = Color.green;
        RectTransform rfi = fill.GetComponent<RectTransform>();
        rfi.anchorMin = Vector2.zero; rfi.anchorMax = Vector2.one; rfi.sizeDelta = Vector2.zero;

        s.targetGraphic = imgF; 
        s.fillRect = rfi;
        s.direction = Slider.Direction.LeftToRight;
        s.value = 0;

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
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
        gamePanel = CrearPanel(score == 100 ? new Color(0.2f, 0.6f, 0.2f) : new Color(0.8f, 0.8f, 0.2f));

        string msg = score == 100 ? "¡EXCELENTE! (+100)" : "REGULAR (+50)";
        CrearTexto(gamePanel.transform, msg, 0, 50, 60, Color.white);
        
        CrearBoton(gamePanel.transform, "SIGUIENTE >>", 0, -100, Color.white, ShowRound);
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
        }
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
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        
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
}
