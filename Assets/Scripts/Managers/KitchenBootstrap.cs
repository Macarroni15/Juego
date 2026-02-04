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

        GameObject lightObj = new GameObject("Luz");
        lightObj.transform.SetParent(container.transform);
        Light l = lightObj.AddComponent<Light>();
        l.type = LightType.Directional;
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

        // 2. Suelo
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Suelo";
        floor.transform.SetParent(container.transform);
        floor.transform.localScale = new Vector3(10, 1, 10);
        floor.GetComponent<Renderer>().material.color = new Color(0.2f, 0.2f, 0.2f);

        // 3. INDICADOR DE COCINA
        GameObject kitchenArea = new GameObject("Zona_Cocina");
        kitchenArea.transform.SetParent(container.transform);
        
        GameObject kitchenFloor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        kitchenFloor.name = "Zona_Cocina_Suelo"; kitchenFloor.transform.SetParent(kitchenArea.transform);
        kitchenFloor.transform.position = new Vector3(8, 0.01f, 40);
        kitchenFloor.transform.localScale = new Vector3(3f, 1, 3.5f);
        kitchenFloor.GetComponent<Renderer>().material.color = new Color(0.25f, 0.25f, 0.25f);
        
        // PARED DE AZULEJOS (Backsplash) CORREGIDA
        GameObject backsplash = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backsplash.name = "Backsplash_Cocina"; backsplash.transform.SetParent(kitchenArea.transform);
        backsplash.transform.position = new Vector3(12, 4, 54.4f); backsplash.transform.localScale = new Vector3(30, 8, 0.2f);
        backsplash.GetComponent<Renderer>().material.color = new Color(0.9f, 0.95f, 1f);

        // 4. PAREDES EXTERIORES (Ajustadas para no colisionar con muebles)
        CreateWall("Restaurante_Pared_Fondo", new Vector3(0, 4, 55), new Vector3(64, 8, 1), container.transform);
        CreateWall("Restaurante_Pared_Izquierda", new Vector3(-32, 4, 2.5f), new Vector3(1, 8, 92), container.transform);
        CreateWall("Restaurante_Pared_Derecha", new Vector3(32, 4, 2.5f), new Vector3(1, 8, 92), container.transform);
        CreateWall("Restaurante_Pared_Frontal_L", new Vector3(-18, 4, -43), new Vector3(28, 8, 1), container.transform);
        CreateWall("Restaurante_Pared_Frontal_R", new Vector3(18, 4, -43), new Vector3(28, 8, 1), container.transform);
        CreateWall("Restaurante_Pared_Frontal_Top", new Vector3(0, 7, -43), new Vector3(10, 2, 1), container.transform);

        // 5. OFICINA Y BAÑO
        GameObject backRoom = new GameObject("Oficina_Y_Baño");
        backRoom.transform.SetParent(container.transform);
        CreateWall("Cuarto_Pared_Derecha", new Vector3(-10, 4, 39), new Vector3(1, 8, 18), backRoom.transform, new Color(0.7f, 0.7f, 0.7f));
        CreateWall("Cuarto_Pared_Frontal_L", new Vector3(-25.5f, 4, 30), new Vector3(13, 8, 1), backRoom.transform, new Color(0.7f, 0.7f, 0.7f));
        CreateWall("Cuarto_Pared_Frontal_Top", new Vector3(-15, 7, 30), new Vector3(8, 2, 1), backRoom.transform, new Color(0.7f, 0.7f, 0.7f));

        GameObject desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
        desk.name = "Escritorio"; desk.transform.SetParent(backRoom.transform);
        desk.transform.position = new Vector3(-27, 0.8f, 42); desk.transform.localScale = new Vector3(3, 0.2f, 2);
        desk.GetComponent<Renderer>().material.color = new Color(0.2f, 0.1f, 0f);
        CreateChair("Oficina_Silla", new Vector3(-27, 0.5f, 44), Vector3.forward, backRoom.transform);

        CreateToilet("Váter", new Vector3(-15, 0.5f, 44), backRoom.transform);
        CreateSink("Lavabo", new Vector3(-15, 1.2f, 38), backRoom.transform);
        CreateBucket("Balde_1", new Vector3(-12, 0.4f, 44), backRoom.transform);
        CreateBucket("Balde_2", new Vector3(-12, 0.4f, 41), backRoom.transform);

        // 6. MOBILIARIO DE COCINA
        GameObject kitchenFurniture = new GameObject("Muebles_Cocina");
        kitchenFurniture.transform.SetParent(kitchenArea.transform);
        // 6. NEVERA Y ARMARIOS BASE
        GameObject fridge = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fridge.name = "Nevera"; fridge.transform.SetParent(kitchenFurniture.transform);
        fridge.transform.position = new Vector3(22, 2.5f, 45); fridge.transform.localScale = new Vector3(3, 5, 3);
        fridge.GetComponent<Renderer>().material.color = new Color(0.85f, 0.85f, 0.85f);

        // Pared del fondo: Estaciones de trabajo premium (Movidas a la derecha para no chocar con el baño)
        for (int i = 0; i < 5; i++)
        {
            float xPos = 0 + (i * 6); // Comienza en 0 en lugar de -12
            if (i == 0) CreateAppliance("Fregadero_Industrial", new Vector3(xPos, 1f, 52), new Color(0.75f, 0.75f, 0.75f), "Sink", kitchenFurniture.transform);
            else if (i == 1 || i == 3) CreateAppliance("Fogon_Pro_" + i, new Vector3(xPos, 1f, 52), new Color(0.2f, 0.2f, 0.2f), "Stove", kitchenFurniture.transform);
            else {
                GameObject cabinet = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cabinet.name = "Mueble_Inox_" + i; cabinet.transform.SetParent(kitchenFurniture.transform);
                cabinet.transform.position = new Vector3(xPos, 1f, 52); cabinet.transform.localScale = new Vector3(5.5f, 2, 4f);
                cabinet.GetComponent<Renderer>().material.color = new Color(0.7f, 0.7f, 0.7f);
                GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
                top.transform.SetParent(cabinet.transform); top.transform.localPosition = new Vector3(0, 0.55f, 0);
                top.transform.localScale = new Vector3(1.02f, 0.08f, 1.05f); top.GetComponent<Renderer>().material.color = new Color(0.15f, 0.15f, 0.15f);
            }
        }

        // Armarios altos corregidos
        for (int i = 0; i < 5; i++)
        {
            GameObject wallCabinet = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallCabinet.name = "Armario_Alto_" + i; wallCabinet.transform.SetParent(kitchenFurniture.transform);
            wallCabinet.transform.position = new Vector3(0 + (i * 6), 5.5f, 53.5f);
            wallCabinet.transform.localScale = new Vector3(5.5f, 3, 1.5f);
            wallCabinet.GetComponent<Renderer>().material.color = new Color(0.95f, 0.95f, 0.95f);
        }

        // 7. ISLA PROFESIONAL (Movida a la derecha)
        GameObject island = new GameObject("IslaCocina_Premium");
        island.transform.SetParent(kitchenFurniture.transform);
        island.transform.position = new Vector3(12, 0, 35);
        
        // Base de la isla
        GameObject baseIsland = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseIsland.transform.SetParent(island.transform);
        baseIsland.transform.localPosition = new Vector3(0, 1f, 0);
        baseIsland.transform.localScale = new Vector3(12f, 2f, 5f);
        baseIsland.GetComponent<Renderer>().material.color = new Color(0.6f, 0.62f, 0.65f);

        // Encimera superior de mármol/negro
        GameObject islandTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        islandTop.transform.SetParent(baseIsland.transform);
        islandTop.transform.localPosition = new Vector3(0, 0.55f, 0);
        islandTop.transform.localScale = new Vector3(1.05f, 0.1f, 1.05f);
        islandTop.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.1f);

        // 8. MOSTRADOR DE PEDIDOS
        GameObject orderCounter = GameObject.CreatePrimitive(PrimitiveType.Cube);
        orderCounter.name = "Mostrador_Pedidos"; orderCounter.transform.SetParent(container.transform);
        orderCounter.transform.position = new Vector3(-15, 1.5f, -5);
        orderCounter.transform.localScale = new Vector3(10, 3, 2);
        orderCounter.GetComponent<Renderer>().material.color = new Color(0.5f, 0.3f, 0.2f);
        GameObject counterTopMostrador = GameObject.CreatePrimitive(PrimitiveType.Cube);
        counterTopMostrador.transform.SetParent(orderCounter.transform);
        counterTopMostrador.transform.localPosition = new Vector3(0, 0.55f, 0);
        counterTopMostrador.transform.localScale = new Vector3(1.1f, 0.1f, 1.1f);
        counterTopMostrador.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.1f);

        // 9. LOS 3 COCINEROS (Ajustados a las nuevas posiciones)
        Vector3[] chefPos = { new Vector3(12, 0.1f, 30), new Vector3(12, 0.1f, 40), new Vector3(5, 0.1f, 40) };
        for (int i = 0; i < 3; i++)
        {
            string n = "Cocinero_" + i;
            GameObject oldChef = GameObject.Find(n); if (oldChef != null) DestroyImmediate(oldChef);
            GameObject chef = CreatePerson(n, chefPos[i], Color.white, null);
            chef.AddComponent<PlayerController>(); chef.AddComponent<PlayerInput>();
            Transform head = chef.transform.Find("Visuals/Head");
            if (head != null) {
                GameObject hat = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                hat.transform.SetParent(head); hat.transform.localPosition = new Vector3(0, 0.45f, 0);
                hat.transform.localScale = new Vector3(0.7f, 0.5f, 0.7f);
            }
        }

        // 10. PERSONA EN LA BARRA (Fija)
        GameObject barPerson = CreatePerson("StaffBarra", new Vector3(10, 0.1f, -4), new Color(0.2f, 0.5f, 0.8f), null);
        barPerson.transform.rotation = Quaternion.Euler(0, 180, 0);
        barPerson.transform.localScale = new Vector3(1.1f, 1.4f, 1.1f);

        // 11. ELEMENTOS BARRA
        CreateCashRegister("CajaRegistradora", new Vector3(5, 1.6f, -5), container.transform);
        CreateOrderScreen("PantallaPedidos", new Vector3(10, 1.6f, -5.2f), container.transform);
        CreateTPV("TPV_Pago", new Vector3(15, 1.6f, -5), container.transform);
        CreateBar("Barra_Cocina", new Vector3(10, 1.5f, -5), new Vector3(40, 3, 1), container.transform);

        // 12. ESTACIONES ORGANIZADAS (Look Profesional)
        float dispY = 2.1f;
        float dispZ = 52.0f; 
        
        // Ingredientes en la pared del fondo (Centrados con los nuevos muebles)
        CreateStation("Tomate", new Vector3(6, dispY, dispZ), Color.red, typeof(DispenserStation), kitchenArea.transform, "Tomato");
        CreateStation("Lechuga", new Vector3(12, dispY, dispZ), Color.green, typeof(DispenserStation), kitchenArea.transform, "Lettuce");
        CreateStation("Pan", new Vector3(18, dispY, dispZ), new Color(0.8f, 0.5f, 0.2f), typeof(DispenserStation), kitchenArea.transform, "Bread");
        
        // Estaciones en la Isla (Ajustadas a la nueva X de la isla)
        CreateStation("Tabla de Cortar 1", new Vector3(10, dispY, 35), Color.gray, typeof(CuttingStation), kitchenArea.transform);
        CreateStation("Tabla de Cortar 2", new Vector3(14, dispY, 35), Color.gray, typeof(CuttingStation), kitchenArea.transform);
        
        CreateStation("Pollo", new Vector3(15, dispY, 32.5f), new Color(1f, 0.8f, 0.6f), typeof(DispenserStation), kitchenArea.transform, "Chicken");
        CreateStation("Brocoli", new Vector3(9, dispY, 32.5f), new Color(0, 0.4f, 0), typeof(DispenserStation), kitchenArea.transform, "Broccoli");

        CreateStation("Basura", new Vector3(28, 1, 10), Color.black, typeof(TrashStation), kitchenArea.transform);
        CreateStation("Ventanilla Entrega", new Vector3(0, 1.5f, -5.5f), Color.blue, typeof(DeliveryStation), container.transform);

        // 13. MESAS Y SILLAS
        int tablesCount = 0;
        for (int x = -2; x <= 2; x += 2)
        {
            for (int z = -1; z <= 1; z++)
            {
                Vector3 tablePos = new Vector3(x * 12, 0.8f, z * 10 - 25);
                CreateTableWithChairs("Mesa_" + (tablesCount++), tablePos, container.transform);
            }
        }

        Debug.Log("URGENTE: Restaurante con 3 Cocineros Controlables Generado.");
    }

    private void CreateToilet(string n, Vector3 p, Transform par)
    {
        GameObject toilet = new GameObject(n);
        toilet.transform.SetParent(par);
        toilet.transform.position = p;
        GameObject tank = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tank.transform.SetParent(toilet.transform); tank.transform.localPosition = new Vector3(0, 0.5f, -0.4f);
        tank.transform.localScale = new Vector3(0.8f, 1f, 0.4f); tank.GetComponent<Renderer>().material.color = Color.white;
        GameObject bowl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bowl.transform.SetParent(toilet.transform); bowl.transform.localPosition = new Vector3(0, 0f, 0.2f);
        bowl.transform.localScale = new Vector3(0.7f, 0.4f, 0.7f); bowl.GetComponent<Renderer>().material.color = Color.white;
    }

    private void CreateSink(string n, Vector3 p, Transform par)
    {
        GameObject sink = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sink.name = n; sink.transform.SetParent(par); sink.transform.position = p;
        sink.transform.localScale = new Vector3(1.2f, 0.2f, 1f); sink.GetComponent<Renderer>().material.color = Color.white;
        GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        leg.transform.SetParent(sink.transform); leg.transform.localPosition = new Vector3(0, -4.5f, 0);
        leg.transform.localScale = new Vector3(0.2f, 4.5f, 0.2f); leg.GetComponent<Renderer>().material.color = Color.gray;
    }

    private void CreateBucket(string n, Vector3 p, Transform par)
    {
        GameObject bucket = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bucket.name = n; bucket.transform.SetParent(par); bucket.transform.position = p;
        bucket.transform.localScale = new Vector3(0.6f, 0.4f, 0.6f); bucket.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.5f);
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
                burner.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.1f);
            }
            // Campana extractora
            GameObject vent = GameObject.CreatePrimitive(PrimitiveType.Cube);
            vent.name = "Campana"; vent.transform.SetParent(app.transform);
            vent.transform.localPosition = new Vector3(0, 2.5f, 0.2f);
            vent.transform.localScale = new Vector3(1f, 0.5f, 0.6f);
            vent.GetComponent<Renderer>().material.color = new Color(0.7f, 0.7f, 0.7f);
        } else if (type == "Sink") {
            GameObject basin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            basin.transform.SetParent(app.transform);
            basin.transform.localPosition = new Vector3(0, 0.45f, 0);
            basin.transform.localScale = new Vector3(0.8f, 0.1f, 0.7f);
            basin.GetComponent<Renderer>().material.color = new Color(0.4f, 0.5f, 0.6f);
            // Grifo
            GameObject tap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tap.transform.SetParent(app.transform);
            tap.transform.localPosition = new Vector3(0, 0.8f, 0.4f);
            tap.transform.localScale = new Vector3(0.05f, 0.3f, 0.05f);
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
