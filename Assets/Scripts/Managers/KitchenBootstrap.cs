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
    private GameObject gamePanel;

    // --- LOGIC VARIABLES ---
    private int totalScore = 0;
    private int consecutiveMediums = 0; // Para perder si sacas 3 veces 50%
    private int roundsSurvived = 0;

    [System.Serializable]
    public class Scenario
    {
        public string clientName;
        public string conditionDescription;
        
        public string optionA_Text;
        public int optionA_Score;
        public string optionA_Image; // Nombre del sprite en Resources
        
        public string optionB_Text;
        public int optionB_Score;
        public string optionB_Image;
        
        public string optionC_Text;
        public int optionC_Score;
        public string optionC_Image;
    }
    
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
        gamePanel = null; 

        // 1. CARGAR FONDO
        // IMPORTANTE: Resources.Load NO debe llevar extension.
        Sprite bgSprite = Resources.Load<Sprite>("Images/fondo_cocina");
        
        if(bgSprite == null) Debug.LogWarning("No se encontro ninguna imagen de fondo llamada fondo_cocina.");
        
        Color baseColor = (bgSprite != null) ? Color.white : new Color(0.1f, 0.1f, 0.2f);
        currentMenuPanel = CrearPanel(baseColor, bgSprite);
        currentMenuPanel.name = "Background"; // Rename to match user request

        // 2. OVERLAY (Oscurecer un poco, no demasiado)
        GameObject overlay = new GameObject("Overlay");
        overlay.transform.SetParent(currentMenuPanel.transform, false);
        Image overlayImg = overlay.AddComponent<Image>();
        overlayImg.color = new Color(0, 0, 0, 0.4f); // 40% opacidad
        RectTransform ovRect = overlay.GetComponent<RectTransform>();
        ovRect.anchorMin = Vector2.zero; ovRect.anchorMax = Vector2.one;
        ovRect.offsetMin = Vector2.zero; ovRect.offsetMax = Vector2.zero;

        // 3. AREA TITULO (Mitad Superior)
        GameObject headerArea = new GameObject("HeaderArea");
        headerArea.transform.SetParent(overlay.transform, false);
        RectTransform headerRect = headerArea.AddComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 0.55f); 
        headerRect.anchorMax = new Vector2(1, 1); // Ocupa de la mitad para arriba
        headerRect.offsetMin = Vector2.zero; headerRect.offsetMax = Vector2.zero;

        // Titulo
        GameObject titleObj = CrearTexto(headerArea.transform, "LA COCINA DESASTRE", 0, 30, 80, Color.white, false);
        titleObj.GetComponent<Text>().fontStyle = FontStyle.Bold;
        Shadow shadow = titleObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0,0,0,0.8f);
        shadow.effectDistance = new Vector2(3, -3);
        
        RectTransform titleTR = titleObj.GetComponent<RectTransform>();
        titleTR.anchorMin = new Vector2(0, 0.3f); titleTR.anchorMax = new Vector2(1, 0.9f);
        titleTR.offsetMin = Vector2.zero; titleTR.offsetMax = Vector2.zero;

        // Subtitulo
        GameObject subObj = CrearTexto(headerArea.transform, "<i>Elige el plato correcto o pierde clientes</i>", 0, 0, 36, new Color(0.9f, 0.9f, 0.9f), false);
        RectTransform subTR = subObj.GetComponent<RectTransform>();
        subTR.anchorMin = new Vector2(0, 0.1f); subTR.anchorMax = new Vector2(1, 0.3f);
        subTR.offsetMin = Vector2.zero; subTR.offsetMax = Vector2.zero;


        // 4. AREA BOTONES (Mitad Inferior)
        GameObject buttonsArea = new GameObject("ButtonsArea");
        buttonsArea.transform.SetParent(overlay.transform, false);
        RectTransform btnAreaRect = buttonsArea.AddComponent<RectTransform>();
        btnAreaRect.anchorMin = new Vector2(0, 0); 
        btnAreaRect.anchorMax = new Vector2(1, 0.5f); // Ocupa la mitad inferior
        btnAreaRect.offsetMin = Vector2.zero; btnAreaRect.offsetMax = Vector2.zero;

        // JUGAR (Centrado en la parte superior del area botones)
        CrearBotonMenu(buttonsArea.transform, "¡ A JUGAR !", 0, 50, new Color(0.2f, 0.7f, 0.3f), new Vector2(350, 80), StartEduGame);
        
        // SALIR (Mas abajo)
        CrearBotonMenu(buttonsArea.transform, "Salirdel Juego", 0, -60, new Color(0.8f, 0.3f, 0.3f), new Vector2(200, 50), () => {
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

    private void InitializeScenarios()
    {
        scenarios = new List<Scenario>();

        scenarios.Add(new Scenario {
            clientName = "María",
            conditionDescription = "Tengo una migraña terrible y necesito comer algo ligero.",
            optionA_Text = "Hamburguesa", optionA_Score = 0, optionA_Image = "Food/Burger",
            optionB_Text = "Aguacate", optionB_Score = 50, optionB_Image = "Food/Toast",
            optionC_Text = "Pescado", optionC_Score = 100, optionC_Image = "Food/Fish"
        });

        scenarios.Add(new Scenario {
            clientName = "Carlos",
            conditionDescription = "Tengo una maratón en 2 horas, necesito energía.",
            optionA_Text = "Pasta", optionA_Score = 100, optionA_Image = "Food/Pasta",
            optionB_Text = "Ensalada", optionB_Score = 50, optionB_Image = "Food/Salad",
            optionC_Text = "Cocido", optionC_Score = 0, optionC_Image = "Food/Stew"
        });

        scenarios.Add(new Scenario {
            clientName = "Lucía",
            conditionDescription = "Siento mucha acidez estomacal.",
            optionA_Text = "Arroz y Pollo", optionA_Score = 100, optionA_Image = "Food/Rice",
            optionB_Text = "Zumo Naranja", optionB_Score = 0, optionB_Image = "Food/Juice",
            optionC_Text = "Sándwich", optionC_Score = 50, optionC_Image = "Food/Sandwich"
        });
        
        // Add more if needed...
    }

    // ... (SecuenciaInicio logic is fine) ...

    // --- JUEGO ---

    public void StartEduGame()
    {
        if (currentMenuPanel != null) Destroy(currentMenuPanel);
        CrearCanvas(); 

        totalScore = 0;
        consecutiveMediums = 0;
        roundsSurvived = 0;
        
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
