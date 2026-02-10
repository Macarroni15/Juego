using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI; // Necesario para el nuevo sistema de inputs
using System.Collections;

public class GameLauncher : MonoBehaviour
{
    [Header("Configuración")]
    public string nombreEscenaJuego = "SampleScene"; // CAMBIA ESTO si tu escena del restaurante tiene otro nombre

    private GameObject canvasObj;
    private GameObject panelCarga;
    private GameObject panelMenu;
    private GameObject panelTutorial;
    private Slider barraCarga;

    private void Start()
    {
        // 1. Asegurar que existe el sistema de eventos (Inputs)
        SetupEventSystem();

        // 2. Crear toda la Interfaz Visualmente (Código genera UI)
        GenerarUI();

        // 3. Iniciar la secuencia de carga falsa
        StartCoroutine(SecuenciaCarga());
    }

    private void SetupEventSystem()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }
    }

    private IEnumerator SecuenciaCarga()
    {
        // Estado inicial: Solo carga visible
        panelCarga.SetActive(true);
        panelMenu.SetActive(false);
        panelTutorial.SetActive(false);

        float tiempoCarga = 0f;
        while (tiempoCarga < 3f) // Carga ficticia de 3 segundos
        {
            tiempoCarga += Time.deltaTime;
            float progreso = tiempoCarga / 3f;
            if (barraCarga != null) barraCarga.value = progreso;
            yield return null;
        }

        // Fin de carga
        panelCarga.SetActive(false);
        panelMenu.SetActive(true);
    }

    // --- FUNCIONES DE LOS BOTONES ---

    public void OnClick_Iniciar()
    {
        SceneManager.LoadScene(nombreEscenaJuego);
    }

    public void OnClick_Tutorial()
    {
        panelMenu.SetActive(false);
        panelTutorial.SetActive(true);
    }

    public void OnClick_Salir()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void OnClick_VolverDeTutorial()
    {
        panelTutorial.SetActive(false);
        panelMenu.SetActive(true);
    }

    // --- GENERACIÓN DE UI (FEA PERO FUNCIONAL) ---
    // Este código crea los objetos visuales (Canvas, Paneles, Botones) en tiempo real.
    
    private void GenerarUI()
    {
        // Canvas
        canvasObj = new GameObject("CanvasMenu");
        Canvas c = canvasObj.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>(); // Para que se adapte
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // FUENTE (Intento robusto de conseguir Arial)
        Font fuente = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (fuente == null) fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // --- 1. PANEL DE CARGA ---
        panelCarga = CrearPanel("PanelCarga", Color.black);
        CrearTexto(panelCarga.transform, "TXT_Cargando", "CARGANDO...", 0, 50, 60, Color.white, fuente);
        
        // Barra de carga
        GameObject sliderObj = new GameObject("BarraProgreso");
        sliderObj.transform.SetParent(panelCarga.transform);
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.sizeDelta = new Vector2(500, 30);
        sliderRect.anchoredPosition = new Vector2(0, -50);
        barraCarga = sliderObj.AddComponent<Slider>();
        
        // Fondo barra
        GameObject fondoBarra = new GameObject("Fondo");
        fondoBarra.transform.SetParent(sliderObj.transform);
        Image imgFondo = fondoBarra.AddComponent<Image>();
        imgFondo.color = Color.gray;
        RectTransform fondoRect = fondoBarra.GetComponent<RectTransform>();
        fondoRect.anchorMin = Vector2.zero; fondoRect.anchorMax = Vector2.one; fondoRect.offsetMin = Vector2.zero; fondoRect.offsetMax = Vector2.zero;

        // Relleno barra
        GameObject areaRelleno = new GameObject("AreaRelleno");
        areaRelleno.transform.SetParent(sliderObj.transform);
        RectTransform areaRect = areaRelleno.AddComponent<RectTransform>();
        areaRect.anchorMin = Vector2.zero; areaRect.anchorMax = Vector2.one; areaRect.offsetMin = new Vector2(5,5); areaRect.offsetMax = new Vector2(-5,-5);
        
        GameObject kRelleno = new GameObject("Relleno");
        kRelleno.transform.SetParent(areaRelleno.transform);
        Image imgRelleno = kRelleno.AddComponent<Image>();
        imgRelleno.color = Color.green;
        RectTransform rellenoRect = kRelleno.GetComponent<RectTransform>();
        rellenoRect.anchorMin = Vector2.zero; rellenoRect.anchorMax = Vector2.one; rellenoRect.offsetMin = Vector2.zero; rellenoRect.offsetMax = Vector2.zero;
        
        barraCarga.targetGraphic = imgFondo;
        barraCarga.fillRect = rellenoRect;


        // --- 2. PANEL MENU PRINCIPAL ---
        Sprite bgMenu = Resources.Load<Sprite>("Images/fondo_cocina");
        Color colorMenu = (bgMenu != null) ? Color.white : new Color(0.1f, 0.1f, 0.2f);
        panelMenu = CrearPanel("PanelMenu", colorMenu);
        if (bgMenu != null) panelMenu.GetComponent<Image>().sprite = bgMenu;
        panelMenu.name = "Background"; // Match requested hierarchy name
        CrearTexto(panelMenu.transform, "TituloJuego", "JUEGO DE COCINA", 0, 200, 80, Color.yellow, fuente);

        CrearBoton(panelMenu.transform, "BtnJugar", "INICIAR JUEGO", 0, 20, Color.green, fuente, OnClick_Iniciar);
        CrearBoton(panelMenu.transform, "BtnTutorial", "TUTORIALES", 0, -80, Color.cyan, fuente, OnClick_Tutorial);
        CrearBoton(panelMenu.transform, "BtnSalir", "SALIR", 0, -180, new Color(1f, 0.5f, 0.5f), fuente, OnClick_Salir);


        // --- 3. PANEL TUTORIAL ---
        panelTutorial = CrearPanel("PanelTutorial", new Color(0.2f, 0.3f, 0.2f));
        CrearTexto(panelTutorial.transform, "TituloTut", "TUTORIAL", 0, 250, 70, Color.white, fuente);
        
        string textoInstrucciones = 
            "BIENVENIDO A TU COCINA\n\n" +
            "1. Observa a los clientes que entran.\n" +
            "2. Detecta su estado (Migraña, Cansancio, etc).\n" +
            "3. Cocina el plato que mejor les venga.\n" +
            "   (Evita ingredientes que les sienten mal)\n\n" +
            "¡Buena suerte Chef!";
            
        CrearTexto(panelTutorial.transform, "TextoInstrucciones", textoInstrucciones, 0, 0, 30, Color.white, fuente);

        CrearBoton(panelTutorial.transform, "BtnVolver", "VOLVER AL MENÚ", 0, -250, Color.white, fuente, OnClick_VolverDeTutorial);
    }

    // --- HELPERS PARA CREAR OBJETOS ---

    private GameObject CrearPanel(string nombre, Color color)
    {
        GameObject panel = new GameObject(nombre);
        panel.transform.SetParent(canvasObj.transform);
        panel.AddComponent<CanvasRenderer>();
        Image img = panel.AddComponent<Image>();
        img.color = color;
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
        return panel;
    }

    private void CrearTexto(Transform padre, string nombre, string contenido, float x, float y, int tamano, Color color, Font fuente)
    {
        GameObject textObj = new GameObject(nombre);
        textObj.transform.SetParent(padre);
        Text txt = textObj.AddComponent<Text>();
        txt.text = contenido;
        txt.font = fuente;
        txt.fontSize = tamano;
        txt.color = color;
        txt.alignment = TextAnchor.MiddleCenter;
        
        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(800, 400);
        rect.localScale = Vector3.one;
    }

    private void CrearBoton(Transform padre, string nombre, string texto, float x, float y, Color colorFondo, Font fuente, UnityEngine.Events.UnityAction accion)
    {
        GameObject btnObj = new GameObject(nombre);
        btnObj.transform.SetParent(padre);
        
        Image img = btnObj.AddComponent<Image>();
        img.color = colorFondo;
        
        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(accion);

        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(400, 80);
        rect.anchoredPosition = new Vector2(x, y);
        rect.localScale = Vector3.one;

        CrearTexto(btnObj.transform, "TextoBtn", texto, 0, 0, 30, Color.black, fuente);
    }
}
