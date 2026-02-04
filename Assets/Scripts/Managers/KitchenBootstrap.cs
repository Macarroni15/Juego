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

        string containerName = "RESTAURANTE_GENERADO_AUTOMATICAMENTE";
        
        GameObject old = GameObject.Find(containerName);
        if (old != null)
        {
            if (Application.isPlaying) Destroy(old);
            else DestroyImmediate(old);
        }

        GameObject container = new GameObject(containerName);
        container.transform.position = Vector3.zero;

        // 1. CÁMARA Y LUZ
        if (Camera.main == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.transform.SetParent(container.transform);
            camObj.AddComponent<Camera>();
            camObj.transform.position = new Vector3(0, 45, -45);
            camObj.transform.rotation = Quaternion.Euler(50, 0, 0);
            camObj.tag = "MainCamera";
        }

        GameObject lightObj = new GameObject("Luz");
        lightObj.transform.SetParent(container.transform);
        Light l = lightObj.AddComponent<Light>();
        l.type = LightType.Directional;
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
        CreateDoor("Puerta_Principal", new Vector3(0, 2.5f, -50.1f), new Vector3(8.5f, 5, 0.25f), new Color(0.4f, 0.2f, 0.1f), container.transform);

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

        // SUELO EXTERIOR (PARQUE GIGANTE)
        GameObject parkContainer = new GameObject("PARQUE_EXTERIOR");
        parkContainer.transform.SetParent(container.transform);

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Suelo_Parque"; ground.transform.SetParent(parkContainer.transform);
        ground.transform.position = new Vector3(0, -0.2f, 0);
        ground.transform.localScale = new Vector3(40, 1, 40); // 400x400 unidades
        ground.GetComponent<Renderer>().material.color = new Color(0.2f, 0.35f, 0.2f);

        // 1. EL RESORT DE LA PISCINA (Privada, Profunda y con Socorrista)
        CreatePoolArea("Resort_Piscina", new Vector3(-80, -0.1f, 0), new Vector3(45, 1.5f, 30), parkContainer.transform);

        // 2. PARKING Y COCHES (Realismo Urbano)
        CreateParkingLot("Parking_Principal", new Vector3(0, 0, -85), 12, parkContainer.transform);
        Color[] carColors = { Color.red, new Color(0.1f, 0.1f, 0.1f), Color.blue, Color.white, Color.gray, new Color(0.1f, 0.4f, 0.1f) };
        for(int i=0; i<8; i++) {
            if (i == 3 || i == 6) continue; // Huecos libres
            float xPos = -27.5f + (i * 5);
            CreateCar("Coche_Cliente_" + i, new Vector3(xPos, 0, -85), carColors[i % carColors.Length], parkContainer.transform);
        }

        // 3. ZONA DE PICNIC (Mesas y bancos de madera)
        Vector3[] picnicPos = { 
            new Vector3(60, 0, 30), new Vector3(80, 0, 30), new Vector3(100, 0, 30),
            new Vector3(60, 0, -30), new Vector3(80, 0, -30), new Vector3(100, 0, -30)
        };
        foreach(var p in picnicPos) CreatePicnicTable("Mesa_Picnic", p, parkContainer.transform);

        // 4. BOSQUE DE ÁRBOLES (Decoración masiva)
        for(int i=0; i<40; i++) {
            float rx = Random.Range(-180f, 180f);
            float rz = Random.Range(-180f, 180f);
            // Evitar spawn dentro del edificio o parking
            if (Mathf.Abs(rx) < 45 && rz > -60) continue; 
            if (Mathf.Abs(rx) < 40 && rz < -70 && rz > -100) continue; // No árboles en el parking
            CreateTree("Arbol_Parque_" + i, new Vector3(rx, 0, rz), parkContainer.transform);
        }


        // 4. BAÑO REAL Y BONITO (A la izquierda - TOTALMENTE CERRADO)
        GameObject bathroom = new GameObject("Area_Baño");
        bathroom.transform.SetParent(container.transform);
        
        // Suelo Baño (Sistema de baldosas celeste claro)
        CreateTiledFloor("Suelo_Baño", new Vector3(-21, 0.05f, 40), new Vector3(22, 1, 30), new Color(0.85f, 0.95f, 1f), bathroom.transform);

        // Muros Baño para CERRARLO (Ajuste al fondo z=54.5)
        CreateWall("Baño_Wall_DER", new Vector3(-10, 4, 39.75f), new Vector3(0.5f, 8, 29.5f), bathroom.transform, new Color(0.9f, 0.9f, 0.9f));
        // Muro frontal partido para la PUERTA (Hueco de 5 unidades)
        CreateWall("Baño_Wall_FRON_L", new Vector3(-29, 4, 25), new Vector3(6, 8, 0.5f), bathroom.transform, new Color(0.9f, 0.9f, 0.9f));
        CreateWall("Baño_Wall_FRON_R", new Vector3(-15.5f, 4, 25), new Vector3(11, 8, 0.5f), bathroom.transform, new Color(0.9f, 0.9f, 0.9f));
        // Top y Puerta con overlap de seguridad aumentado
        CreateWall("Baño_Wall_FRON_Top", new Vector3(-23.5f, 6.5f, 25), new Vector3(6.5f, 3, 0.5f), bathroom.transform, new Color(0.9f, 0.9f, 0.9f));
        
        // Puerta del Baño (Sellado total con Z=25.02 y ancho extra)
        CreateDoor("Puerta_Entrada_Baño", new Vector3(-23.5f, 2.5f, 25.02f), new Vector3(5.8f, 5, 0.2f), new Color(0.4f, 0.2f, 0f), bathroom.transform);

        // Mobiliario Baño
        // Mueble Lavabo (Vanity)
        GameObject vanity = GameObject.CreatePrimitive(PrimitiveType.Cube);
        vanity.name = "Mueble_Espejo"; vanity.transform.SetParent(bathroom.transform);
        vanity.transform.position = new Vector3(-15, 1, 53.5f); vanity.transform.localScale = new Vector3(6, 2, 2);
        vanity.GetComponent<Renderer>().material.color = new Color(0.3f, 0.2f, 0.1f);

        CreateDetailedSink("Lavabo_Real", new Vector3(-15, 2.1f, 53.5f), bathroom.transform);
        CreateMirror("Espejo_Pared", new Vector3(-15, 6, 54.4f), bathroom.transform);
        CreateDetailedToilet("Bater_Real", new Vector3(-25, 0.1f, 53.5f), bathroom.transform);

        // 5. COCINA PROFESIONAL (A la derecha al fondo)
        GameObject kitchen = new GameObject("Area_Cocina");
        kitchen.transform.SetParent(container.transform);
        
        // SUELO COCINA GASTRONÓMICO (Baldosas Blancas Pulidas)
        CreateTiledFloor("Suelo_Cocina", new Vector3(16, 0.05f, 40), new Vector3(32, 1, 30), new Color(0.95f, 0.95f, 0.95f), kitchen.transform);

        // Muros Cocina (AJUSTE MATEMÁTICO - TERMINA EN 54.5)
        CreateWall("Cocina_Wall_IZQ", new Vector3(0, 4, 39.75f), new Vector3(1f, 8, 29.5f), kitchen.transform, new Color(0.9f, 0.9f, 0.9f));
        // Frente partido: La suma de anchos debe cubrir exactamente x=0 a x=32
        // L: Centro 7.5, Ancho 15 -> Cubre 0 a 15
        CreateWall("Cocina_Wall_FRON_L", new Vector3(7.5f, 4, 25), new Vector3(15, 8, 0.6f), kitchen.transform, new Color(0.9f, 0.9f, 0.9f));
        // R: Centro 25.5, Ancho 13 -> Cubre 19 a 32
        CreateWall("Cocina_Wall_FRON_R", new Vector3(25.5f, 4, 25), new Vector3(13, 8, 0.6f), kitchen.transform, new Color(0.9f, 0.9f, 0.9f));
        // Hueco queda entre x=15 y x=19 (4 unidades). Usamos ancho 5.5 para sellar totalmente los lados.
        CreateWall("Cocina_Wall_FRON_Top", new Vector3(17, 6.5f, 25), new Vector3(5.5f, 3, 0.6f), kitchen.transform, new Color(0.9f, 0.9f, 0.9f));
        
        // Puerta de la Cocina (Z muy pegada al muro para evitar fugas de luz)
        CreateDoor("Puerta_Modern_Cocina", new Vector3(17, 2.5f, 25.02f), new Vector3(5.2f, 5, 0.15f), new Color(0.4f, 0.25f, 0.1f), kitchen.transform);

        // Decoración Azulejos (Centrados y contenidos)
        for(int tx=3; tx<28; tx+=4) {
            for(int ty=1; ty<8; ty+=3) {
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.name = "Tile"; tile.transform.SetParent(kitchen.transform);
                tile.transform.position = new Vector3(tx, ty, 54.7f); tile.transform.localScale = new Vector3(3.5f, 2.5f, 0.1f);
                tile.GetComponent<Renderer>().material.color = new Color(0.94f, 0.96f, 1f);
            }
        }

        // Mobiliario Cocina (Contenedor faltante)
        GameObject kFurniture = new GameObject("Muebles_Cocina");
        kFurniture.transform.SetParent(kitchen.transform);

        // Iluminación LED Moderna y Estantes de Acero
        for(int i=0; i<3; i++) {
            float xPos = 6 + (i * 10);
            // Estante metálico colgante
            GameObject rack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rack.name = "Rack_Acero"; rack.transform.SetParent(kitchen.transform);
            rack.transform.position = new Vector3(xPos, 5.5f, 49.2f); rack.transform.localScale = new Vector3(8, 0.15f, 1.5f);
            rack.GetComponent<Renderer>().material.color = new Color(0.8f, 0.8f, 0.82f);
            
            // Tira LED bajo estante
            GameObject led = GameObject.CreatePrimitive(PrimitiveType.Cube);
            led.transform.SetParent(rack.transform); led.transform.localPosition = new Vector3(0, -0.4f, 0); led.transform.localScale = new Vector3(0.95f, 0.3f, 0.2f);
            led.GetComponent<Renderer>().material.color = new Color(0.9f, 1f, 1f); // Luz fría moderna
        }

        // Decoración Fresca: Plantas/Hierbas aromáticas en estantes
        for(int i=0; i<2; i++) {
            GameObject plant = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            plant.name = "Hierba_Cocina"; plant.transform.SetParent(kitchen.transform);
            plant.transform.position = new Vector3(4 + (i * 20), 5.85f, 49.2f); plant.transform.localScale = new Vector3(0.6f, 0.8f, 0.6f);
            plant.GetComponent<Renderer>().material.color = new Color(0.1f, 0.5f, 0.1f);
        }

        // Nevera Industrial (Compacta y dentro de márgenes)
        GameObject fridge = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fridge.name = "Nevera_Pro"; fridge.transform.SetParent(kFurniture.transform);
        fridge.transform.position = new Vector3(27.5f, 3.5f, 48); fridge.transform.localScale = new Vector3(4.5f, 7, 3);
        fridge.GetComponent<Renderer>().material.color = new Color(0.95f, 0.95f, 0.95f);

        // Puestos de cocina MODERNOS (Espaciado quirúrgico)
        float[] xPositions = { 4f, 10f, 16f, 22f };
        for (int i = 0; i < xPositions.Length; i++) {
            float x = xPositions[i];
            if (i == 0) CreateAppliance("Fregadero_Elite", new Vector3(x, 1f, 48), Color.white, "Sink", kFurniture.transform);
            else if (i == 1) CreateAppliance("Lavavajillas_Elite", new Vector3(x, 1f, 48), new Color(0.8f, 0.8f, 0.82f), "Dishwasher", kFurniture.transform);
            else if (i == 2) {
                CreateAppliance("Fogon_Elite_A", new Vector3(x, 1f, 48), new Color(0.1f, 0.1f, 0.12f), "Stove", kFurniture.transform);
                CreatePot("Olla_Elite", new Vector3(x - 0.8f, 2.3f, 47.5f), kFurniture.transform);
            }
            else {
                CreateAppliance("Fogon_Elite_B", new Vector3(x, 1f, 48), new Color(0.1f, 0.1f, 0.12f), "Stove", kFurniture.transform);
                CreatePan("Sarten_Elite", new Vector3(x + 0.8f, 2.15f, 48.5f), kFurniture.transform);
            }
        }

        // Isla Central con Tablas de Corte
        GameObject island = new GameObject("Isla_Trabajo");
        island.transform.SetParent(kFurniture.transform); island.transform.position = new Vector3(14, 0, 36);
        GameObject iBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        iBase.transform.SetParent(island.transform); iBase.transform.localPosition = new Vector3(0, 1, 0); iBase.transform.localScale = new Vector3(14, 2, 5);
        iBase.GetComponent<Renderer>().material.color = new Color(0.6f, 0.6f, 0.65f);
        
        // Estantes altos (Decoración industrial)
        for (int i = 0; i < 4; i++) {
            GameObject shelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shelf.name = "Estante_Gastro_" + i; shelf.transform.SetParent(kFurniture.transform);
            shelf.transform.position = new Vector3(4 + (i * 7), 6, 49.5f); shelf.transform.localScale = new Vector3(6, 0.2f, 2);
            shelf.GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.5f);
        }

        // 6. MOSTRADOR Y BARRA DE PINTXOS (Elegante y Detallada)
        GameObject bar = new GameObject("Barra_Pedidos");
        bar.transform.SetParent(container.transform);
        // Barra principal con tope de mármol
        CreateBar("Mueble_Barra", new Vector3(0, 1.1f, -5), new Vector3(40, 2.2f, 2), bar.transform);
        GameObject marbleTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marbleTop.name = "Tope_Marmol"; marbleTop.transform.SetParent(bar.transform);
        marbleTop.transform.position = new Vector3(0, 2.25f, -5); marbleTop.transform.localScale = new Vector3(41, 0.15f, 2.5f);
        marbleTop.GetComponent<Renderer>().material.color = new Color(0.95f, 0.95f, 0.98f);
        
        // Llenar la barra de PINTXOS EN PLATOS Y COPAS (Sin vitrinas)
        float surY = 2.33f;
        
        // 1. Zona de Pintxos (Presentación abierta en platos)
        string[] pTypes = { "Tortilla", "Chaca", "Pimiento", "Jamon", "Chorizo", "Tortilla", "Jamon" };
        for (int i = 0; i < pTypes.Length; i++) {
            float xPos = -18 + (i * 4.2f);
            Vector3 pPos = new Vector3(xPos, surY, -5);
            
            // Plato de cerámica blanca
            GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plate.name = "Plato_Pintxo"; plate.transform.SetParent(bar.transform);
            plate.transform.position = pPos + new Vector3(0, 0.025f, 0); // Ligeramente sobre el mármol
            plate.transform.localScale = new Vector3(1.2f, 0.02f, 1.2f);
            plate.GetComponent<Renderer>().material.color = Color.white;

            // El Pintxo encima
            CreatePintxo("Px_Barra_" + i, pPos + new Vector3(0, 0.05f, 0), pTypes[i], bar.transform);
        }

        // 2. Zona de Copas y Vasos (Agrupados y sobre la barra)
        GameObject glassZone = new GameObject("Zona_Copas");
        glassZone.transform.SetParent(bar.transform);
        for (int r = 0; r < 3; r++) {
            for (int c = 0; c < 4; c++) {
                // Posición ajustada para que el vaso (0.5u altura) apoye en la base
                CreateGlass("Copa_Stock", new Vector3(12 + (r * 0.9f), surY + 0.25f, -5.5f + (c * 0.8f)), glassZone.transform);
            }
        }

        // 3. Botellas de reserva (Ajustadas a superficie)
        for(int i=0; i<6; i++) {
            CreateBottle("Botella_Exposicion", new Vector3(18.5f, surY, -4.5f - (i*0.6f)), bar.transform);
        }

        CreateCashRegister("Caja", new Vector3(-12, surY, -5), bar.transform);
        CreateOrderScreen("Monitor", new Vector3(0, surY, -5), bar.transform);
        CreateTPV("Datafono", new Vector3(7, surY, -5), bar.transform);

        // 7. COCINEROS Y STAFF
        Vector3[] cPos = { new Vector3(10, 0.1f, 32), new Vector3(20, 0.1f, 32), new Vector3(15, 0.1f, 42) };
        for (int i = 0; i < 3; i++) {
            GameObject c = CreatePerson("Cocinero_" + i, cPos[i], Color.white, null);
            c.AddComponent<PlayerController>(); c.AddComponent<PlayerInput>();
        }
        GameObject staff = CreatePerson("Staff_Senior", new Vector3(0, 0.1f, -2), new Color(0.2f, 0.4f, 0.7f), null);
        staff.transform.rotation = Quaternion.Euler(0, 180, 0);
        staff.transform.localScale = new Vector3(1.1f, 1.25f, 1.1f); // Personaje más alto para la barra

        // 8. DESPENSA DE INGREDIENTES (TODOS JUNTOS EN UNA MESA)
        GameObject pantry = new GameObject("Despensa_Ingredientes");
        pantry.transform.SetParent(kitchen.transform);
        pantry.transform.position = new Vector3(5, 1, 30);
        
        GameObject pantryTable = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pantryTable.transform.SetParent(pantry.transform); pantryTable.transform.localPosition = Vector3.zero;
        pantryTable.transform.localScale = new Vector3(8, 2, 3);
        pantryTable.GetComponent<Renderer>().material.color = new Color(0.4f, 0.25f, 0.1f);

        float sY = 2.1f;
        // Ingredientes organizados en la mesa de la despensa
        CreateStation("Tomate", new Vector3(2.5f, sY, 30), Color.red, typeof(DispenserStation), pantry.transform, "Tomato");
        CreateStation("Pan", new Vector3(5, sY, 30), new Color(0.8f, 0.5f, 0.2f), typeof(DispenserStation), pantry.transform, "Bread");
        CreateStation("Lechuga", new Vector3(7.5f, sY, 30), Color.green, typeof(DispenserStation), pantry.transform, "Lettuce");
        
        // Estaciones de Trabajo
        CreateStation("Tabla_1", new Vector3(10, sY, 36), Color.gray, typeof(CuttingStation), kitchen.transform);
        CreateStation("Tabla_2", new Vector3(18, sY, 36), Color.gray, typeof(CuttingStation), kitchen.transform);

        // 9. COMEDOR EXPANDIDO Y DECORADO
        int count = 0;
        // Más mesas en un grid de 3x4
        for (int x = -1; x <= 1; x++) {
            for (int z = 0; z < 4; z++) {
                Vector3 tablePos = new Vector3(x * 12, 0.8f, -13 - (z * 10));
                CreateTableWithChairs("Mesa_Lujo_" + (count++), tablePos, container.transform);
            }
        }

        // Decoración extra: Plantas y Lámparas de pie
        for (int i = 0; i < 4; i++) {
            // Lámparas modernas
            GameObject lamp = new GameObject("Lampara_Pie");
            lamp.transform.SetParent(container.transform);
            lamp.transform.position = new Vector3((i%2==0?1:-1) * 25, 0, -10 - (i*12));
            GameObject baseL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            baseL.transform.SetParent(lamp.transform); baseL.transform.localPosition = new Vector3(0, 0.1f, 0); baseL.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
            GameObject mast = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mast.transform.SetParent(lamp.transform); mast.transform.localPosition = new Vector3(0, 2, 0); mast.transform.localScale = new Vector3(0.1f, 2, 0.1f);
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.transform.SetParent(lamp.transform); head.transform.localPosition = new Vector3(0, 4, 0); head.transform.localScale = new Vector3(1.2f, 0.8f, 1.2f);
            head.GetComponent<Renderer>().material.color = new Color(1, 0.9f, 0.6f);
            head.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");

            // Plantas en maceta
            GameObject pot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pot.name = "Maceta"; pot.transform.SetParent(container.transform);
            pot.transform.position = new Vector3((i%2==0?-1:1) * 25, 0.5f, -15 - (i*12));
            pot.transform.localScale = new Vector3(1.2f, 0.5f, 1.2f); pot.GetComponent<Renderer>().material.color = new Color(0.5f, 0.3f, 0.2f);
            GameObject plant = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            plant.transform.SetParent(pot.transform); plant.transform.localPosition = new Vector3(0, 2f, 0); plant.transform.localScale = new Vector3(0.8f, 2.5f, 0.8f);
            plant.GetComponent<Renderer>().material.color = new Color(0.15f, 0.45f, 0.15f);
        }

        Debug.Log("Restaurante de LUJO Generado: Mesas ampliadas, decoración y vegetación lista.");
    }

    private void CreateDetailedToilet(string n, Vector3 p, Transform par)
    {
        GameObject toilet = new GameObject(n);
        toilet.transform.SetParent(par); toilet.transform.position = p;
        
        // Base
        GameObject baseT = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        baseT.transform.SetParent(toilet.transform); baseT.transform.localPosition = new Vector3(0, 0.2f, 0);
        baseT.transform.localScale = new Vector3(0.6f, 0.2f, 0.6f); baseT.GetComponent<Renderer>().material.color = Color.white;
        
        // Taza
        GameObject bowl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bowl.transform.SetParent(toilet.transform); bowl.transform.localPosition = new Vector3(0, 0.7f, 0.2f);
        bowl.transform.localScale = new Vector3(0.7f, 0.6f, 0.9f); bowl.GetComponent<Renderer>().material.color = Color.white;

        // Tanque
        GameObject tank = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tank.transform.SetParent(toilet.transform); tank.transform.localPosition = new Vector3(0, 1.2f, -0.4f);
        tank.transform.localScale = new Vector3(0.8f, 0.8f, 0.4f); tank.GetComponent<Renderer>().material.color = Color.white;
        
        // Botón
        GameObject btn = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        btn.transform.SetParent(tank.transform); btn.transform.localPosition = new Vector3(0.3f, 0.52f, 0);
        btn.transform.localScale = new Vector3(0.2f, 0.05f, 0.2f); btn.GetComponent<Renderer>().material.color = Color.silver;
    }

    private void CreateDetailedSink(string n, Vector3 p, Transform par)
    {
        GameObject sink = new GameObject(n);
        sink.transform.SetParent(par); sink.transform.position = p;

        // Lavabo
        GameObject bowl = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bowl.transform.SetParent(sink.transform); bowl.transform.localPosition = new Vector3(0, 0.05f, 0);
        bowl.transform.localScale = new Vector3(1.2f, 0.3f, 1f); bowl.GetComponent<Renderer>().material.color = Color.white;

        // Grifo
        GameObject tap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tap.transform.SetParent(bowl.transform); tap.transform.localPosition = new Vector3(0, 0.6f, -0.4f);
        tap.transform.localScale = new Vector3(0.1f, 0.2f, 0.1f); tap.GetComponent<Renderer>().material.color = Color.silver;
    }

    private void CreateMirror(string n, Vector3 p, Transform par)
    {
        GameObject mirror = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mirror.name = n; mirror.transform.SetParent(par); mirror.transform.position = p;
        mirror.transform.localScale = new Vector3(4, 3, 0.1f);
        mirror.GetComponent<Renderer>().material.color = new Color(0.7f, 0.8f, 1f, 0.5f);
    }

    private void CreatePot(string n, Vector3 p, Transform par)
    {
        GameObject pot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pot.name = n; pot.transform.SetParent(par); pot.transform.position = p;
        pot.transform.localScale = new Vector3(0.7f, 0.35f, 0.7f);
        pot.GetComponent<Renderer>().material.color = new Color(0.6f, 0.62f, 0.65f);
        // Asas
        for (int i = 0; i < 2; i++) {
            GameObject h = GameObject.CreatePrimitive(PrimitiveType.Cube);
            h.transform.SetParent(pot.transform);
            h.transform.localPosition = new Vector3(i == 0 ? 0.6f : -0.6f, 0.3f, 0);
            h.transform.localScale = new Vector3(0.4f, 0.1f, 0.2f);
            h.GetComponent<Renderer>().material.color = Color.black;
        }
    }

    private void CreatePan(string n, Vector3 p, Transform par)
    {
        GameObject pan = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pan.name = n; pan.transform.SetParent(par); pan.transform.position = p;
        pan.transform.localScale = new Vector3(0.8f, 0.1f, 0.8f);
        pan.GetComponent<Renderer>().material.color = new Color(0.15f, 0.15f, 0.15f);
        // Mango
        GameObject h = GameObject.CreatePrimitive(PrimitiveType.Cube);
        h.transform.SetParent(pan.transform);
        h.transform.localPosition = new Vector3(0, 0, -1.2f);
        h.transform.localScale = new Vector3(0.15f, 0.3f, 1.5f);
        h.GetComponent<Renderer>().material.color = Color.black;
    }

    private void CreateTableWithChairs(string nameBase, Vector3 pos, Transform par)
    {
        GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
        table.name = nameBase; table.transform.SetParent(par); table.transform.position = pos;
        table.transform.localScale = new Vector3(5, 0.2f, 5); table.GetComponent<Renderer>().material.color = new Color(0.3f, 0.15f, 0.1f);
        GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        leg.transform.SetParent(table.transform); leg.transform.localPosition = new Vector3(0, -4, 0);
        leg.transform.localScale = new Vector3(0.2f, 4, 0.2f); leg.GetComponent<Renderer>().material.color = Color.black;

        Vector3[] chairOffsets = { new Vector3(0, 0, 4), new Vector3(0, 0, -4), new Vector3(4, 0, 0), new Vector3(-4, 0, 0) };
        for (int i = 0; i < chairOffsets.Length; i++)
        {
            Vector3 chairPos = pos + chairOffsets[i]; chairPos.y = 0.5f;
            CreateChair(nameBase + "_Silla_" + i, chairPos, chairOffsets[i] * -1, par);
            
            // Añadir cubiertos, vaso y plato en la mesa frente a la silla
            Vector3 tableSettingPos = pos + chairOffsets[i].normalized * 1.5f;
            tableSettingPos.y = pos.y + 0.15f;
            CreateTableSetting(nameBase + "_Servicio_" + i, tableSettingPos, Quaternion.LookRotation(chairOffsets[i] * -1), par);

            if (Random.value > 0.3f) {
                Vector3 customerPos = chairPos + Vector3.up * 0.1f;
                GameObject customer = CreatePerson("Cliente_" + nameBase + "_" + i, customerPos, Random.ColorHSV(), par, true);
                customer.transform.LookAt(pos);
            }
        }
    }

    private void CreateTableSetting(string n, Vector3 p, Quaternion rot, Transform par)
    {
        GameObject setting = new GameObject(n);
        setting.transform.SetParent(par); setting.transform.position = p; setting.transform.rotation = rot;

        // Plato
        GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        plate.name = "Plato"; plate.transform.SetParent(setting.transform);
        plate.transform.localPosition = Vector3.zero;
        plate.transform.localScale = new Vector3(0.8f, 0.02f, 0.8f);
        plate.GetComponent<Renderer>().material.color = Color.white;

        // Vaso
        GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        glass.name = "Vaso"; glass.transform.SetParent(setting.transform);
        glass.transform.localPosition = new Vector3(0.5f, 0.2f, 0.3f);
        glass.transform.localScale = new Vector3(0.15f, 0.25f, 0.15f);
        glass.GetComponent<Renderer>().material.color = new Color(0.8f, 0.9f, 1f, 0.5f);

        // Tenedor (Cubo fino)
        GameObject fork = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fork.name = "Tenedor"; fork.transform.SetParent(setting.transform);
        fork.transform.localPosition = new Vector3(-0.55f, 0.05f, 0);
        fork.transform.localScale = new Vector3(0.05f, 0.02f, 0.4f);
        fork.GetComponent<Renderer>().material.color = Color.gray;

        // Cuchillo (Cubo fino)
        GameObject knife = GameObject.CreatePrimitive(PrimitiveType.Cube);
        knife.name = "Cuchillo"; knife.transform.SetParent(setting.transform);
        knife.transform.localPosition = new Vector3(0.55f, 0.05f, 0);
        knife.transform.localScale = new Vector3(0.05f, 0.02f, 0.4f);
        knife.GetComponent<Renderer>().material.color = Color.gray;
    }

    private void CreateChair(string n, Vector3 p, Vector3 lookDir, Transform par)
    {
        GameObject chair = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chair.name = n; chair.transform.SetParent(par); chair.transform.position = p;
        chair.transform.localScale = new Vector3(1.2f, 1f, 1.2f); chair.GetComponent<Renderer>().material.color = new Color(0.4f, 0.4f, 0.4f);
        GameObject back = GameObject.CreatePrimitive(PrimitiveType.Cube);
        back.transform.SetParent(chair.transform); back.transform.localPosition = new Vector3(0, 1f, -0.4f);
        back.transform.localScale = new Vector3(1f, 1.5f, 0.2f); back.GetComponent<Renderer>().material.color = new Color(0.4f, 0.4f, 0.4f);
        chair.transform.rotation = Quaternion.LookRotation(lookDir);
    }

    private GameObject CreatePerson(string n, Vector3 p, Color c, Transform par, bool isSitting = false)
    {
        GameObject person = new GameObject(n);
        if (par != null) person.transform.SetParent(par);
        person.transform.position = p;

        // Root logic
        if (!isSitting) {
            Rigidbody rb = person.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            CapsuleCollider col = person.AddComponent<CapsuleCollider>();
            col.height = 2f; col.radius = 0.4f; col.center = new Vector3(0, 0.9f, 0);
        }

        GameObject visuals = new GameObject("Visuals");
        visuals.transform.SetParent(person.transform);
        visuals.transform.localPosition = Vector3.zero;
        if (!isSitting) visuals.AddComponent<CharacterAnimator>();

        // Posiciones relativas fijas para que toquen el suelo si person.y = 0
        
        // Piernas
        if (isSitting) {
            CreateLimb("LegL", visuals.transform, new Vector3(-0.15f, 0.5f, 0.4f), new Vector3(0.2f, 0.2f, 0.8f), Color.blue);
            CreateLimb("LegR", visuals.transform, new Vector3(0.15f, 0.5f, 0.4f), new Vector3(0.2f, 0.2f, 0.8f), Color.blue);
        } else {
            CreateLimb("LegL", visuals.transform, new Vector3(-0.15f, 0.4f, 0), new Vector3(0.2f, 0.8f, 0.2f), Color.blue);
            CreateLimb("LegR", visuals.transform, new Vector3(0.15f, 0.4f, 0), new Vector3(0.2f, 0.8f, 0.2f), Color.blue);
        }

        // Torso
        GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Cube);
        torso.name = "Torso"; torso.transform.SetParent(visuals.transform);
        float torsoY = isSitting ? 0.7f : 1.2f;
        torso.transform.localPosition = new Vector3(0, torsoY, 0);
        torso.transform.localScale = new Vector3(0.6f, 0.8f, 0.3f);
        torso.GetComponent<Renderer>().material.color = c;

        // Cabeza
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head"; head.transform.SetParent(visuals.transform);
        float headY = isSitting ? 1.3f : 1.8f;
        head.transform.localPosition = new Vector3(0, headY, 0);
        head.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        head.GetComponent<Renderer>().material.color = new Color(0.9f, 0.7f, 0.6f); // Skin tone

        // Ojos
        GameObject eyeL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        eyeL.name = "EyeL"; eyeL.transform.SetParent(head.transform);
        eyeL.transform.localPosition = new Vector3(-0.2f, 0.15f, 0.4f);
        eyeL.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        eyeL.GetComponent<Renderer>().material.color = Color.black;

        GameObject eyeR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        eyeR.name = "EyeR"; eyeR.transform.SetParent(head.transform);
        eyeR.transform.localPosition = new Vector3(0.2f, 0.15f, 0.4f);
        eyeR.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        eyeR.GetComponent<Renderer>().material.color = Color.black;

        // Brazos
        float armsY = isSitting ? 0.7f : 1.2f;
        float armsZ = isSitting ? 0.3f : 0f;
        Vector3 armsScale = isSitting ? new Vector3(0.15f, 0.15f, 0.7f) : new Vector3(0.15f, 0.7f, 0.15f);
        CreateLimb("ArmL", visuals.transform, new Vector3(-0.4f, armsY, armsZ), armsScale, c);
        CreateLimb("ArmR", visuals.transform, new Vector3(0.4f, armsY, armsZ), armsScale, c);

        return person;
    }

    private void CreateLimb(string name, Transform parent, Vector3 localPos, Vector3 scale, Color color)
    {
        GameObject limb = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        limb.name = name; limb.transform.SetParent(parent);
        limb.transform.localPosition = localPos;
        limb.transform.localScale = scale;
        limb.GetComponent<Renderer>().material.color = color;
    }

    private void CreateCashRegister(string n, Vector3 p, Transform par)
    {
        GameObject register = new GameObject(n);
        register.transform.SetParent(par); register.transform.position = p;
        
        GameObject baseObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseObj.name = "Base"; baseObj.transform.SetParent(register.transform);
        baseObj.transform.localPosition = new Vector3(0, 0, 0);
        baseObj.transform.localScale = new Vector3(0.8f, 0.3f, 0.6f);
        baseObj.GetComponent<Renderer>().material.color = Color.gray;

        GameObject drawer = GameObject.CreatePrimitive(PrimitiveType.Cube);
        drawer.name = "Cajon"; drawer.transform.SetParent(register.transform);
        drawer.transform.localPosition = new Vector3(0, 0, 0.35f);
        drawer.transform.localScale = new Vector3(0.7f, 0.1f, 0.1f);
        drawer.GetComponent<Renderer>().material.color = new Color(0.2f, 0.2f, 0.2f);

        GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
        screen.name = "Pantalla"; screen.transform.SetParent(register.transform);
        screen.transform.localPosition = new Vector3(0, 0.25f, -0.1f);
        screen.transform.rotation = Quaternion.Euler(-30, 0, 0);
        screen.transform.localScale = new Vector3(0.6f, 0.4f, 0.1f);
        screen.GetComponent<Renderer>().material.color = Color.black;
    }

    private void CreateOrderScreen(string n, Vector3 p, Transform par)
    {
        GameObject monitor = new GameObject(n);
        monitor.transform.SetParent(par); monitor.transform.position = p;

        GameObject stand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stand.transform.SetParent(monitor.transform); stand.transform.localPosition = new Vector3(0, -0.2f, 0);
        stand.transform.localScale = new Vector3(0.1f, 0.2f, 0.1f);
        stand.GetComponent<Renderer>().material.color = Color.black;

        GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
        screen.transform.SetParent(monitor.transform); screen.transform.localPosition = Vector3.zero;
        screen.transform.localScale = new Vector3(0.8f, 0.5f, 0.05f);
        screen.GetComponent<Renderer>().material.color = new Color(0.1f, 0.3f, 0.1f);
    }

    private void CreateTPV(string n, Vector3 p, Transform par)
    {
        GameObject tpv = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tpv.name = n; tpv.transform.SetParent(par); tpv.transform.position = p;
        tpv.transform.localScale = new Vector3(0.2f, 0.1f, 0.4f);
        tpv.transform.rotation = Quaternion.Euler(15, 0, 0);
        tpv.GetComponent<Renderer>().material.color = new Color(0.15f, 0.15f, 0.2f);
    }

    private void CreateWall(string n, Vector3 p, Vector3 s, Transform par, Color? c = null)
    {
        GameObject w = GameObject.CreatePrimitive(PrimitiveType.Cube);
        w.name = n; w.transform.SetParent(par); w.transform.position = p; w.transform.localScale = s;
        w.GetComponent<Renderer>().material.color = c ?? new Color(0.8f, 0.8f, 0.7f);
    }

    private void CreateBar(string n, Vector3 p, Vector3 s, Transform par)
    {
        GameObject b = GameObject.CreatePrimitive(PrimitiveType.Cube);
        b.name = n; b.transform.SetParent(par); b.transform.position = p; b.transform.localScale = s;
        b.GetComponent<Renderer>().material.color = new Color(0.4f, 0.2f, 0.1f);
    }

    private void CreateDoor(string n, Vector3 p, Vector3 s, Color c, Transform par)
    {
        GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.name = n; door.transform.SetParent(par); door.transform.position = p; door.transform.localScale = s;
        door.GetComponent<Renderer>().material.color = c;
        if (c.a < 1f) {
            Material mat = door.GetComponent<Renderer>().material;
            mat.SetFloat("_Mode", 3); // Transparent
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
        }
    }

    private void CreateAppliance(string n, Vector3 p, Color c, string type, Transform par)
    {
        GameObject app = GameObject.CreatePrimitive(PrimitiveType.Cube);
        app.name = n; app.transform.SetParent(par); app.transform.position = p;
        app.transform.localScale = new Vector3(4.5f, 2f, 3.5f);
        app.GetComponent<Renderer>().material.color = c;

        if (type == "Stove") {
            // Fuego / Placa
            for (int i = 0; i < 4; i++) {
                GameObject burner = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                burner.transform.SetParent(app.transform);
                burner.transform.localPosition = new Vector3((i < 2 ? -0.25f : 0.25f), 0.51f, (i % 2 == 0 ? -0.25f : 0.25f));
                burner.transform.localScale = new Vector3(0.35f, 0.02f, 0.35f);
                burner.GetComponent<Renderer>().material.color = new Color(0.05f, 0.05f, 0.05f);
            }
            // Campana extractora (Industrial)
            GameObject vent = GameObject.CreatePrimitive(PrimitiveType.Cube);
            vent.name = "Campana_Industrial"; vent.transform.SetParent(app.transform);
            vent.transform.localPosition = new Vector3(0, 2.5f, 0.2f);
            vent.transform.localScale = new Vector3(1.1f, 0.6f, 0.7f);
            vent.GetComponent<Renderer>().material.color = new Color(0.75f, 0.75f, 0.75f);
        } else if (type == "Sink") {
            GameObject basin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            basin.transform.SetParent(app.transform);
            basin.transform.localPosition = new Vector3(0, 0.45f, 0);
            basin.transform.localScale = new Vector3(0.85f, 0.15f, 0.75f);
            basin.GetComponent<Renderer>().material.color = new Color(0.4f, 0.5f, 0.65f);
            // Grifo Mejorado (Cuello de cisne)
            GameObject tapBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tapBase.transform.SetParent(app.transform);
            tapBase.transform.localPosition = new Vector3(0, 0.8f, 0.45f);
            tapBase.transform.localScale = new Vector3(0.08f, 0.4f, 0.08f);
            tapBase.GetComponent<Renderer>().material.color = Color.gray;
            
            GameObject neck = GameObject.CreatePrimitive(PrimitiveType.Cube);
            neck.transform.SetParent(tapBase.transform);
            neck.transform.localPosition = new Vector3(0, 0.8f, -1.0f);
            neck.transform.localScale = new Vector3(1.2f, 0.2f, 3.0f);
            neck.GetComponent<Renderer>().material.color = Color.gray;
        } else if (type == "Dishwasher") {
            // Panel frontal
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.transform.SetParent(app.transform);
            panel.transform.localPosition = new Vector3(0, 0.35f, 0.51f);
            panel.transform.localScale = new Vector3(0.9f, 0.1f, 0.05f);
            panel.GetComponent<Renderer>().material.color = new Color(0.2f, 0.2f, 0.2f);
            // Asa
            GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            handle.transform.SetParent(app.transform);
            handle.transform.localPosition = new Vector3(0, 0.2f, 0.52f);
            handle.transform.localScale = new Vector3(0.6f, 0.05f, 0.05f);
            handle.GetComponent<Renderer>().material.color = Color.black;
        }
    }

    private void CreateCrateWithIngredients(string n, Vector3 p, string ing, Transform par)
    {
        GameObject crateRoot = new GameObject(n);
        crateRoot.transform.SetParent(par); crateRoot.transform.position = p;

        Color woodColor = new Color(0.45f, 0.3f, 0.15f);
        // Base
        GameObject bottom = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bottom.transform.SetParent(crateRoot.transform); bottom.transform.localScale = new Vector3(2.5f, 0.1f, 2f);
        bottom.GetComponent<Renderer>().material.color = woodColor;

        // Walls
        float wallThick = 0.1f;
        GameObject wallL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallL.transform.SetParent(crateRoot.transform); 
        wallL.transform.localPosition = new Vector3(-1.2f, 0.5f, 0); wallL.transform.localScale = new Vector3(wallThick, 1f, 2f);
        wallL.GetComponent<Renderer>().material.color = woodColor;

        GameObject wallR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallR.transform.SetParent(crateRoot.transform);
        wallR.transform.localPosition = new Vector3(1.2f, 0.5f, 0); wallR.transform.localScale = new Vector3(wallThick, 1f, 2f);
        wallR.GetComponent<Renderer>().material.color = woodColor;

        GameObject wallF = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallF.transform.SetParent(crateRoot.transform);
        wallF.transform.localPosition = new Vector3(0, 0.5f, 0.95f); wallF.transform.localScale = new Vector3(2.4f, 1f, wallThick);
        wallF.GetComponent<Renderer>().material.color = woodColor;

        GameObject wallB = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallB.transform.SetParent(crateRoot.transform);
        wallB.transform.localPosition = new Vector3(0, 0.5f, -0.95f); wallB.transform.localScale = new Vector3(2.4f, 1f, wallThick);
        wallB.GetComponent<Renderer>().material.color = woodColor;

        GameObject contents = new GameObject("Contents");
        contents.transform.SetParent(crateRoot.transform);
        contents.transform.localPosition = new Vector3(0, 0.4f, 0);
        IngredientVisualizer.BuildVisual(contents, ing, true);
    }

    private void CreatePintxo(string n, Vector3 p, string variant, Transform par)
    {
        GameObject pintxo = new GameObject(n);
        pintxo.transform.SetParent(par); pintxo.transform.position = p;
        // Pan
        GameObject bread = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bread.transform.SetParent(pintxo.transform); bread.transform.localPosition = new Vector3(0, 0.05f, 0);
        bread.transform.localScale = new Vector3(0.5f, 0.15f, 0.5f); bread.GetComponent<Renderer>().material.color = new Color(0.85f, 0.7f, 0.45f);
        
        // Topping variado
        GameObject topping = GameObject.CreatePrimitive(PrimitiveType.Cube);
        topping.transform.SetParent(pintxo.transform); topping.transform.localPosition = new Vector3(0, 0.25f, 0);
        topping.transform.localScale = new Vector3(0.45f, 0.25f, 0.45f);
        
        Color tColor = Color.white;
        if (variant == "Tortilla") tColor = new Color(1f, 0.85f, 0f);
        else if (variant == "Chaca") tColor = new Color(1f, 0.95f, 0.9f);
        else if (variant == "Pimiento") tColor = new Color(0.8f, 0.1f, 0f);
        else if (variant == "Jamon") tColor = new Color(0.5f, 0.1f, 0.15f);
        else if (variant == "Chorizo") tColor = new Color(0.9f, 0.3f, 0f);
        
        topping.GetComponent<Renderer>().material.color = tColor;

        // Adorno extra (Anchoa u objeto)
        if (variant == "Pimiento") {
            GameObject anchovy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            anchovy.transform.SetParent(topping.transform); anchovy.transform.localPosition = new Vector3(0, 0.6f, 0);
            anchovy.transform.localScale = new Vector3(1.1f, 0.2f, 0.3f); anchovy.GetComponent<Renderer>().material.color = new Color(0.2f, 0.15f, 0.1f);
        }

        // Palillo
        GameObject stick = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stick.transform.SetParent(pintxo.transform); stick.transform.localPosition = new Vector3(0, 0.45f, 0);
        stick.transform.localScale = new Vector3(0.02f, 0.4f, 0.02f); stick.GetComponent<Renderer>().material.color = new Color(0.9f, 0.8f, 0.6f);
    }

    private void CreateGlass(string n, Vector3 p, Transform par)
    {
        GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        glass.name = n; glass.transform.SetParent(par); glass.transform.position = p;
        glass.transform.localScale = new Vector3(0.15f, 0.25f, 0.15f);
        glass.GetComponent<Renderer>().material.color = new Color(0.8f, 0.9f, 1f, 0.4f);
    }

    private void CreateBottle(string n, Vector3 p, Transform par)
    {
        GameObject bottle = new GameObject(n);
        bottle.transform.SetParent(par); bottle.transform.position = p;
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.transform.SetParent(bottle.transform); body.transform.localPosition = new Vector3(0, 0.4f, 0);
        body.transform.localScale = new Vector3(0.15f, 0.4f, 0.15f); body.GetComponent<Renderer>().material.color = new Color(0.1f, 0.3f, 0.1f);
        GameObject neck = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        neck.transform.SetParent(bottle.transform); neck.transform.localPosition = new Vector3(0, 0.9f, 0);
        neck.transform.localScale = new Vector3(0.05f, 0.15f, 0.05f); neck.GetComponent<Renderer>().material.color = new Color(0.1f, 0.3f, 0.1f);
    }

    private void CreateTiledFloor(string n, Vector3 pos, Vector3 totalSize, Color c, Transform par)
    {
        GameObject floorGrid = new GameObject(n);
        floorGrid.transform.SetParent(par); floorGrid.transform.position = pos;
        
        int tilesX = 10; int tilesZ = 10;
        float stepX = totalSize.x / tilesX;
        float stepZ = totalSize.z / tilesZ;

        for (int x = 0; x < tilesX; x++) {
            for (int z = 0; z < tilesZ; z++) {
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.name = n + "_Baldosa_" + x + "_" + z;
                tile.transform.SetParent(floorGrid.transform);
                // Posicionamos cada baldosa con una pequeña separación (junta)
                Vector3 tPos = new Vector3(
                    (-totalSize.x/2) + (x * stepX) + stepX/2,
                    0,
                    (-totalSize.z/2) + (z * stepZ) + stepZ/2
                );
                tile.transform.localPosition = tPos;
                tile.transform.localScale = new Vector3(stepX * 0.96f, 0.1f, stepZ * 0.96f);
                tile.GetComponent<Renderer>().material.color = c;
            }
        }
        // Fondo oscuro para las juntas
        GameObject grout = GameObject.CreatePrimitive(PrimitiveType.Cube);
        grout.name = "Juntas"; grout.transform.SetParent(floorGrid.transform);
        grout.transform.localPosition = new Vector3(0, -0.05f, 0);
        grout.transform.localScale = new Vector3(totalSize.x, 0.05f, totalSize.z);
        grout.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.1f);
    }

    private void CreateGlassWindow(string n, Vector3 p, Vector3 s, Transform par)
    {
        GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Cube);
        glass.name = n; glass.transform.SetParent(par); glass.transform.position = p;
        glass.transform.localScale = s;
        
        Renderer rend = glass.GetComponent<Renderer>();
        
        // Búsqueda Ultra-Robusta de Shader (Anti-Rosa definitivo)
        Shader shader = null;
        
        // 1. Intentar URP si está activo
        if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null)
            shader = Shader.Find("Universal Render Pipeline/Lit");
            
        // 2. Intentar Shaders Estándar y Legacy
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
        if (shader == null) shader = Shader.Find("Transparent/Diffuse");
        if (shader == null) shader = Shader.Find("Unlit/Transparent");
        
        // 3. Fail-safe absoluto (Sprites/Default siempre existe y es transparente)
        if (shader == null) shader = Shader.Find("Sprites/Default");

        Material mat = new Material(shader);
        mat.color = new Color(0.8f, 0.9f, 1f, 0.1f); // Muy transparente e invisible

        // Configuración específica según el shader encontrado
        if (mat.HasProperty("_Mode")) mat.SetFloat("_Mode", 3); // Para Standard
        if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1); // Para URP (1 = Transparent)
        
        // Forzar opacidad y renderizado
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        
        rend.material = mat;
    }

    private void CreatePicnicTable(string n, Vector3 p, Transform par)
    {
        GameObject group = new GameObject(n);
        group.transform.SetParent(par); group.transform.position = p;
        Color wood = new Color(0.4f, 0.25f, 0.15f);

        // Mesa
        GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
        top.transform.SetParent(group.transform); top.transform.localPosition = new Vector3(0, 1.25f, 0);
        top.transform.localScale = new Vector3(4, 0.2f, 6); top.GetComponent<Renderer>().material.color = wood;

        // Bancos (Lados)
        for(int i=-1; i<=1; i+=2) {
            GameObject bench = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bench.transform.SetParent(group.transform); bench.transform.localPosition = new Vector3(i * 3, 0.6f, 0);
            bench.transform.localScale = new Vector3(1.5f, 0.15f, 6); bench.GetComponent<Renderer>().material.color = wood;
            // Patas bancos
            for(int j=-1; j<=1; j+=2) {
                GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pole.transform.SetParent(bench.transform); pole.transform.localPosition = new Vector3(0, -2, j * 0.4f);
                pole.transform.localScale = new Vector3(0.2f, 4, 0.1f); pole.GetComponent<Renderer>().material.color = Color.black;
            }
        }
    }

    private void CreatePoolArea(string n, Vector3 p, Vector3 s, Transform par)
    {
        GameObject area = new GameObject(n);
        area.transform.SetParent(par); area.transform.position = p;

        // 1. LA PISCINA PROFUNDA (3m de profundidad real)
        GameObject pool = new GameObject("Contenedor_Piscina");
        pool.transform.SetParent(area.transform); pool.transform.localPosition = Vector3.zero;
        
        // Borde superior
        GameObject border = GameObject.CreatePrimitive(PrimitiveType.Cube);
        border.transform.SetParent(pool.transform); border.transform.localScale = new Vector3(s.x + 2, 0.2f, s.z + 2);
        border.GetComponent<Renderer>().material.color = Color.white;
        
        // Agua (Transparente y profunda con FIX ANTI-ROSA)
        GameObject water = GameObject.CreatePrimitive(PrimitiveType.Cube);
        water.name = "Agua"; water.transform.SetParent(pool.transform);
        water.transform.localPosition = new Vector3(0, 0, 0); water.transform.localScale = new Vector3(s.x, 0.1f, s.z);
        
        Renderer rend = water.GetComponent<Renderer>();
        Shader wShader = null;
        if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null)
            wShader = Shader.Find("Universal Render Pipeline/Lit");
        if (wShader == null) wShader = Shader.Find("Standard");
        if (wShader == null) wShader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
        if (wShader == null) wShader = Shader.Find("Sprites/Default");
        
        Material mat = new Material(wShader);
        mat.color = new Color(0, 0.6f, 1f, 0.4f);
        if (mat.HasProperty("_Mode")) mat.SetFloat("_Mode", 3);
        if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        rend.material = mat;

        // Fondo (Foso de baldosas azules)
        GameObject bottom = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bottom.transform.SetParent(pool.transform); bottom.transform.localPosition = new Vector3(0, -3.0f, 0);
        bottom.transform.localScale = new Vector3(s.x, 0.1f, s.z); bottom.GetComponent<Renderer>().material.color = new Color(0.1f, 0.3f, 0.6f);
        
        // Paredes del foso
        for(int i=-1; i<=1; i+=2) {
            GameObject wX = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wX.transform.SetParent(pool.transform); wX.transform.localPosition = new Vector3(i * s.x/2, -1.5f, 0);
            wX.transform.localScale = new Vector3(0.1f, 3.0f, s.z); wX.GetComponent<Renderer>().material.color = new Color(0.8f, 0.95f, 1f);
            
            GameObject wZ = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wZ.transform.SetParent(pool.transform); wZ.transform.localPosition = new Vector3(0, -1.5f, i * s.z/2);
            wZ.transform.localScale = new Vector3(s.x, 3.0f, 0.1f); wZ.GetComponent<Renderer>().material.color = new Color(0.8f, 0.95f, 1f);
        }

        // ESCALERAS DE CROMO
        for (int i = -1; i <= 1; i += 2) {
            GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rail.transform.SetParent(pool.transform);
            rail.transform.localPosition = new Vector3((s.x/2) - 1, 1.2f, (i * 1.5f));
            rail.transform.localScale = new Vector3(0.15f, 1.2f, 0.15f);
            rail.GetComponent<Renderer>().material.color = new Color(0.85f, 0.85f, 0.85f);
        }

        // 2. VALLADO Y PUERTA (Zona Privada)
        float fX = s.x/2 + 6; float fZ = s.z/2 + 6;
        CreateFencePerimeter(area.transform, fX, fZ);

        // 3. DUCHAS (Entrada)
        CreateShower("Ducha_Doble", new Vector3(fX - 4, 0, fZ - 4), area.transform);

        // 4. SOCORRISTA
        CreateLifeguardStation(new Vector3(0, 0, fZ - 3), area.transform);
    }

    private void CreateFencePerimeter(Transform par, float dx, float dz)
    {
        Color grey = new Color(0.2f, 0.2f, 0.2f);
        CreateWall("Fence_N", new Vector3(0, 1.5f, dz), new Vector3(dx*2, 3, 0.2f), par, grey);
        CreateWall("Fence_S", new Vector3(0, 1.5f, -dz), new Vector3(dx*2, 3, 0.2f), par, grey);
        CreateWall("Fence_W", new Vector3(-dx, 1.5f, 0), new Vector3(0.2f, 3, dz*2), par, grey);
        // Lado con Puerta
        CreateWall("Fence_E1", new Vector3(dx, 1.5f, -dz/2 - 2.5f), new Vector3(0.2f, 3, dz - 5), par, grey);
        CreateWall("Fence_E2", new Vector3(dx, 1.5f, dz/2 + 2.5f), new Vector3(0.2f, 3, dz - 5), par, grey);
        CreateDoor("Puerta_Acceso_Piscina", new Vector3(dx, 1.5f, 0), new Vector3(0.2f, 3, 5), new Color(0.3f, 0.3f, 0.3f), par);
    }

    private void CreateShower(string n, Vector3 p, Transform par)
    {
        GameObject sh = new GameObject(n);
        sh.transform.SetParent(par); sh.transform.position = p;
        // Base baldosas
        GameObject b = GameObject.CreatePrimitive(PrimitiveType.Cube);
        b.transform.SetParent(sh.transform); b.transform.localPosition = new Vector3(0, 0.05f, 0);
        b.transform.localScale = new Vector3(3, 0.1f, 2); b.GetComponent<Renderer>().material.color = new Color(0.8f, 0.85f, 0.9f);
        // Poste y grifo
        GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.transform.SetParent(sh.transform); pole.transform.localPosition = new Vector3(0, 1.5f, -0.7f);
        pole.transform.localScale = new Vector3(0.12f, 1.5f, 0.12f); pole.GetComponent<Renderer>().material.color = Color.gray;
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.transform.SetParent(sh.transform); head.transform.localPosition = new Vector3(0, 3.0f, -0.4f);
        head.transform.localScale = new Vector3(0.4f, 0.1f, 0.4f); head.GetComponent<Renderer>().material.color = Color.white;
    }

    private void CreateLifeguardStation(Vector3 p, Transform par)
    {
        GameObject st = new GameObject("Puesto_Socorrista");
        st.transform.SetParent(par); st.transform.position = p;
        // Silla alta blanca
        GameObject baseS = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseS.transform.SetParent(st.transform); baseS.transform.localPosition = new Vector3(0, 1.5f, 0);
        baseS.transform.localScale = new Vector3(1.2f, 3f, 1.2f); baseS.GetComponent<Renderer>().material.color = Color.white;
        GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        seat.transform.SetParent(st.transform); seat.transform.localPosition = new Vector3(0, 3.1f, 0);
        seat.transform.localScale = new Vector3(1.5f, 0.2f, 1.5f); seat.GetComponent<Renderer>().material.color = Color.red;

        // Socorrista sentado (ajuste de escala para simular pose)
        GameObject guard = CreatePerson("Socorrista", p + new Vector3(0, 3.3f, 0), Color.red, st.transform);
        guard.transform.localScale = new Vector3(0.85f, 0.75f, 0.85f);
        guard.transform.localRotation = Quaternion.Euler(15, 0, 0); // Vigilando el agua
    }

    private void CreateParkingLot(string n, Vector3 p, int spaces, Transform par)
    {
        GameObject lot = new GameObject(n);
        lot.transform.SetParent(par); lot.transform.position = p;
        float lotWidth = spaces * 5;
        float lotDepth = 20;

        // Asfalto
        GameObject asphalt = GameObject.CreatePrimitive(PrimitiveType.Cube);
        asphalt.transform.SetParent(lot.transform); asphalt.transform.localPosition = Vector3.zero;
        asphalt.transform.localScale = new Vector3(lotWidth + 4, 0.1f, lotDepth);
        asphalt.GetComponent<Renderer>().material.color = new Color(0.15f, 0.15f, 0.18f);

        // Líneas de Parking
        for (int i = 0; i <= spaces; i++) {
            GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            line.transform.SetParent(lot.transform);
            line.transform.localPosition = new Vector3((-lotWidth / 2) + (i * 5), 0.06f, 0);
            line.transform.localScale = new Vector3(0.15f, 0.02f, lotDepth * 0.7f);
            line.GetComponent<Renderer>().material.color = Color.white;
        }
    }

    private void CreateCar(string n, Vector3 p, Color c, Transform par)
    {
        GameObject car = new GameObject(n);
        car.transform.SetParent(par); car.transform.position = p;
        
        // Chasis
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.transform.SetParent(car.transform); body.transform.localPosition = new Vector3(0, 0.8f, 0);
        body.transform.localScale = new Vector3(2.2f, 0.9f, 4.5f); body.GetComponent<Renderer>().material.color = c;
        // Cabina
        GameObject cab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cab.transform.SetParent(car.transform); cab.transform.localPosition = new Vector3(0, 1.5f, -0.4f);
        cab.transform.localScale = new Vector3(2f, 0.7f, 2.4f); cab.GetComponent<Renderer>().material.color = c;
        // Cristales
        GameObject win = GameObject.CreatePrimitive(PrimitiveType.Cube);
        win.transform.SetParent(car.transform); win.transform.localPosition = new Vector3(0, 1.5f, 0.8f);
        win.transform.localScale = new Vector3(1.8f, 0.55f, 0.05f); win.GetComponent<Renderer>().material.color = new Color(0.1f, 0.15f, 0.2f);

        // Ruedas (4)
        for(int x=-1; x<=1; x+=2) {
            for(int z=-1; z<=1; z+=2) {
                GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                wheel.transform.SetParent(car.transform);
                wheel.transform.localPosition = new Vector3(x * 1.1f, 0.4f, z * 1.5f);
                wheel.transform.localScale = new Vector3(0.8f, 0.1f, 0.8f);
                wheel.transform.localRotation = Quaternion.Euler(0, 0, 90);
                wheel.GetComponent<Renderer>().material.color = Color.black;
            }
        }
    }

    private void CreateTree(string n, Vector3 p, Transform par)
    {
        GameObject tree = new GameObject(n);
        tree.transform.SetParent(par); tree.transform.position = p;
        // Tronco
        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.transform.SetParent(tree.transform); trunk.transform.localPosition = new Vector3(0, 2, 0);
        trunk.transform.localScale = new Vector3(0.8f, 2, 0.8f); trunk.GetComponent<Renderer>().material.color = new Color(0.3f, 0.2f, 0.1f);
        // Hojas
        GameObject leaves = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leaves.transform.SetParent(tree.transform); leaves.transform.localPosition = new Vector3(0, 5, 0);
        leaves.transform.localScale = new Vector3(5, 5, 5); leaves.GetComponent<Renderer>().material.color = new Color(0.1f, 0.4f, 0.1f);
    }

    private void CreateStation(string n, Vector3 p, Color c, System.Type t, Transform par, string ing = "")
    {
        GameObject s = new GameObject(n);
        s.transform.SetParent(par);
        s.transform.position = p;
        
        if (t == typeof(DispenserStation)) {
            // Visual de la comida real
            GameObject visual = new GameObject("VisualFood");
            visual.transform.SetParent(s.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = Vector3.one * 1.5f; // Más grande en la encimera
            IngredientVisualizer.BuildVisual(visual, ing);
            
            // Trigger invisible para interactuar
            BoxCollider trigger = s.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(2, 2, 2);
            
            DispenserStation ds = s.AddComponent<DispenserStation>();
            ds.ingredient = ScriptableObject.CreateInstance<IngredientSO>();
            ds.ingredient.ingredientName = ing; ds.ingredient.canBeCut = true;
        } else {
            // Estaciones normales como tabla de cortar
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "Base_" + n;
            cube.transform.SetParent(s.transform);
            cube.transform.localPosition = Vector3.zero;
            cube.transform.localScale = new Vector3(2f, 1.2f, 2f);
            cube.GetComponent<Renderer>().material.color = c;
            s.AddComponent(t);
        }
    }
}
