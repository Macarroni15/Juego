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
        kitchenFloor.transform.position = new Vector3(8, 0.01f, 25);
        kitchenFloor.transform.localScale = new Vector3(2.5f, 1, 2.5f);
        kitchenFloor.GetComponent<Renderer>().material.color = new Color(0.4f, 0.4f, 0.4f, 0.5f);

        // 4. PAREDES EXTERIORES
        CreateWall("Restaurante_Pared_Fondo", new Vector3(0, 4, 48), new Vector3(64, 8, 1), container.transform);
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
        GameObject fridge = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fridge.name = "Nevera"; fridge.transform.SetParent(kitchenFurniture.transform);
        fridge.transform.position = new Vector3(20, 2.5f, 40); fridge.transform.localScale = new Vector3(3, 5, 3);
        fridge.GetComponent<Renderer>().material.color = new Color(0.75f, 0.75f, 0.75f);

        for (int i = 0; i < 5; i++)
        {
            GameObject cabinet = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabinet.name = "Armario_Bajo_" + i; cabinet.transform.SetParent(kitchenFurniture.transform);
            cabinet.transform.position = new Vector3(-5 + (i * 4), 1f, 45);
            cabinet.transform.localScale = new Vector3(3.5f, 2, 3.5f);
            cabinet.GetComponent<Renderer>().material.color = Color.white;
            GameObject counterTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            counterTop.transform.SetParent(cabinet.transform);
            counterTop.transform.localPosition = new Vector3(0, 0.55f, 0);
            counterTop.transform.localScale = new Vector3(1.1f, 0.1f, 1.1f);
            counterTop.GetComponent<Renderer>().material.color = Color.black;
        }

        for (int i = 0; i < 4; i++)
        {
            GameObject wallCabinet = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallCabinet.name = "Armario_Alto_" + i; wallCabinet.transform.SetParent(kitchenFurniture.transform);
            wallCabinet.transform.position = new Vector3(-5 + (i * 5), 5.5f, 47);
            wallCabinet.transform.localScale = new Vector3(4, 3, 1.5f);
            wallCabinet.GetComponent<Renderer>().material.color = new Color(0.9f, 0.9f, 0.9f);
        }

        // 7. MOSTRADOR DE PEDIDOS
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

        // 8. LOS 3 COCINEROS (Controlables)
        Vector3[] chefPositions = { new Vector3(8, 1.1f, 15), new Vector3(4, 1.1f, 15), new Vector3(12, 1.1f, 15) };
        for (int i = 0; i < chefPositions.Length; i++)
        {
            GameObject chef = CreatePerson("Cocinero_" + i, chefPositions[i], Color.white, container.transform);
            chef.AddComponent<PlayerController>();
            chef.AddComponent<PlayerInput>();
            // Gorrito de chef
            GameObject hat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hat.name = "GorroChef"; hat.transform.SetParent(chef.transform); 
            hat.transform.localPosition = new Vector3(0, 0.8f, 0.1f);
            hat.transform.localScale = new Vector3(0.6f, 0.5f, 0.6f); 
            hat.GetComponent<Renderer>().material.color = Color.white;
        }

        // 9. Elementos Interiores Funcionales
        CreateBar("Barra_Cocina", new Vector3(10, 1.5f, -5), new Vector3(40, 3, 1), container.transform);

        CreateStation("Tomate", new Vector3(-5, 1, 35), Color.red, typeof(DispenserStation), kitchenArea.transform, "Tomato");
        CreateStation("Lechuga", new Vector3(-1, 1, 35), Color.green, typeof(DispenserStation), kitchenArea.transform, "Lettuce");
        CreateStation("Pan", new Vector3(3, 1, 35), new Color(0.8f, 0.5f, 0.2f), typeof(DispenserStation), kitchenArea.transform, "Bread");
        CreateStation("Pollo", new Vector3(7, 1, 35), new Color(1f, 0.8f, 0.6f), typeof(DispenserStation), kitchenArea.transform, "Chicken");
        CreateStation("Tabla de Cortar", new Vector3(13, 1, 35), Color.gray, typeof(CuttingStation), kitchenArea.transform);
        CreateStation("Basura", new Vector3(25, 1, 5), Color.black, typeof(TrashStation), kitchenArea.transform);
        CreateStation("Ventanilla Entrega", new Vector3(0, 1.5f, -5.5f), Color.blue, typeof(DeliveryStation), container.transform);

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
            if (Random.value > 0.5f) {
                Vector3 customerPos = chairPos + Vector3.up * 0.8f;
                GameObject customer = CreatePerson("Cliente_" + nameBase + "_" + i, customerPos, Random.ColorHSV(), par);
                customer.transform.LookAt(pos);
            }
        }
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

    private GameObject CreatePerson(string n, Vector3 p, Color c, Transform par)
    {
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = n; body.transform.SetParent(par); body.transform.position = p;
        body.GetComponent<Renderer>().material.color = c;
        GameObject eyeL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        eyeL.name = "OjoL"; eyeL.transform.SetParent(body.transform); eyeL.transform.localPosition = new Vector3(-0.2f, 0.5f, 0.4f);
        eyeL.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f); eyeL.GetComponent<Renderer>().material.color = Color.black;
        GameObject eyeR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        eyeR.name = "OjoR"; eyeR.transform.SetParent(body.transform); eyeR.transform.localPosition = new Vector3(0.2f, 0.5f, 0.4f);
        eyeR.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f); eyeR.GetComponent<Renderer>().material.color = Color.black;
        return body;
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

    private void CreateStation(string n, Vector3 p, Color c, System.Type t, Transform par, string ing = "")
    {
        GameObject s = GameObject.CreatePrimitive(PrimitiveType.Cube);
        s.name = n; s.transform.SetParent(par); s.transform.position = p; s.transform.localScale = new Vector3(2f, 1.2f, 2f);
        s.GetComponent<Renderer>().material.color = c;
        s.AddComponent(t);
        if (t == typeof(DispenserStation)) {
            DispenserStation ds = s.GetComponent<DispenserStation>();
            ds.ingredient = ScriptableObject.CreateInstance<IngredientSO>();
            ds.ingredient.ingredientName = ing; ds.ingredient.canBeCut = true;
        }
    }
}
