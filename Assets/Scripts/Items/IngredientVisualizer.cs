using UnityEngine;

public static class IngredientVisualizer
{
    public static void BuildVisual(GameObject container, string name, bool isGroup = true)
    {
        // Remove existing renderer if it's the primitive one
        Renderer r = container.GetComponent<Renderer>();
        if (r != null) r.enabled = false;

        int count = isGroup ? 5 : 1;
        float spread = 0.3f;

        for (int i = 0; i < count; i++)
        {
            GameObject item = new GameObject(name + "_" + i);
            item.transform.SetParent(container.transform);
            item.transform.localPosition = isGroup ? new Vector3(Random.Range(-spread, spread), 0, Random.Range(-spread, spread)) : Vector3.zero;
            item.transform.localScale = Vector3.one * (isGroup ? 0.4f : 1f);

            switch (name.ToLower())
            {
                case "tomato":
                    CreateTomato(item.transform);
                    break;
                case "lettuce":
                    CreateLettuce(item.transform);
                    break;
                case "broccoli":
                    CreateBroccoli(item.transform);
                    break;
                case "bread":
                    CreateBread(item.transform);
                    break;
                case "chicken":
                    CreateChicken(item.transform);
                    break;
                case "meat":
                    CreateMeat(item.transform);
                    break;
                case "cheese":
                    CreateCheese(item.transform);
                    break;
                case "onion":
                    CreateOnion(item.transform);
                    break;
                default:
                    if (r != null) r.enabled = true;
                    break;
            }
        }
    }

    private static void CreateTomato(Transform parent)
    {
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        body.transform.SetParent(parent);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = Vector3.one;
        body.GetComponent<Renderer>().material.color = Color.red;

        for (int i = 0; i < 4; i++)
        {
            GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leaf.transform.SetParent(parent);
            leaf.transform.localPosition = new Vector3(0, 0.48f, 0);
            leaf.transform.localRotation = Quaternion.Euler(0, i * 45, 0);
            leaf.transform.localScale = new Vector3(0.4f, 0.05f, 0.1f);
            leaf.GetComponent<Renderer>().material.color = new Color(0, 0.5f, 0);
        }
    }

    private static void CreateLettuce(Transform parent)
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leaf.transform.SetParent(parent);
            leaf.transform.localPosition = Random.insideUnitSphere * 0.1f;
            leaf.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            leaf.GetComponent<Renderer>().material.color = new Color(0.2f, 0.8f, 0.2f);
        }
    }

    private static void CreateBroccoli(Transform parent)
    {
        GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stem.transform.SetParent(parent);
        stem.transform.localPosition = new Vector3(0, -0.2f, 0);
        stem.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);
        stem.GetComponent<Renderer>().material.color = new Color(0.4f, 0.6f, 0.2f);

        for (int i = 0; i < 6; i++)
        {
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.transform.SetParent(parent);
            float angle = i * Mathf.PI * 2f / 6f;
            head.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.2f, 0.3f, Mathf.Sin(angle) * 0.2f);
            head.transform.localScale = Vector3.one * 0.5f;
            head.GetComponent<Renderer>().material.color = new Color(0.1f, Random.Range(0.3f, 0.5f), 0.1f);
        }
        
        GameObject center = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        center.transform.SetParent(parent);
        center.transform.localPosition = new Vector3(0, 0.4f, 0);
        center.transform.localScale = Vector3.one * 0.6f;
        center.GetComponent<Renderer>().material.color = new Color(0, 0.35f, 0);
    }

    private static void CreateBread(Transform parent)
    {
        // Barra de pan mas realista
        GameObject bread = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        bread.transform.SetParent(parent);
        bread.transform.localPosition = Vector3.zero;
        bread.transform.localScale = new Vector3(0.6f, 0.4f, 1.2f);
        bread.transform.localRotation = Quaternion.Euler(0, 0, 90);
        bread.GetComponent<Renderer>().material.color = new Color(0.82f, 0.55f, 0.28f);

        // Cortes superficiales
        for (int i = 0; i < 3; i++) {
            GameObject cut = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cut.transform.SetParent(bread.transform);
            cut.transform.localPosition = new Vector3(0.4f, 0, -0.4f + (i * 0.4f));
            cut.transform.localScale = new Vector3(0.1f, 0.8f, 0.05f);
            cut.GetComponent<Renderer>().material.color = new Color(0.6f, 0.4f, 0.2f);
        }
    }

    private static void CreateChicken(Transform parent)
    {
        // Muslito de pollo
        GameObject bone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bone.transform.SetParent(parent);
        bone.transform.localPosition = new Vector3(0, 0, 0.4f);
        bone.transform.localScale = new Vector3(0.15f, 0.4f, 0.15f);
        bone.transform.localRotation = Quaternion.Euler(90, 0, 0);
        bone.GetComponent<Renderer>().material.color = Color.white;

        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.transform.SetParent(parent);
        head.transform.localPosition = new Vector3(0, 0, -0.1f);
        head.transform.localScale = new Vector3(0.7f, 0.6f, 0.8f);
        head.GetComponent<Renderer>().material.color = new Color(1f, 0.85f, 0.65f);
    }

    private static void CreateMeat(Transform parent)
    {
        // Filete con veteado
        GameObject steak = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        steak.transform.SetParent(parent);
        steak.transform.localPosition = Vector3.zero;
        steak.transform.localScale = new Vector3(0.8f, 0.1f, 0.6f);
        steak.GetComponent<Renderer>().material.color = new Color(0.6f, 0.2f, 0.2f);

        // Grasa (veteado blanco)
        for (int i = 0; i < 2; i++) {
            GameObject fat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fat.transform.SetParent(steak.transform);
            fat.transform.localPosition = new Vector3(Random.Range(-0.2f, 0.2f), 0.51f, Random.Range(-0.2f, 0.2f));
            fat.transform.localScale = new Vector3(0.4f, 0.05f, 0.05f);
            fat.GetComponent<Renderer>().material.color = new Color(1f, 0.95f, 0.9f);
        }
    }

    private static void CreateCheese(Transform parent)
    {
        // Cuña de queso
        GameObject wedge = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wedge.transform.SetParent(parent);
        wedge.transform.localPosition = Vector3.zero;
        wedge.transform.localScale = new Vector3(0.8f, 0.6f, 1f);
        wedge.GetComponent<Renderer>().material.color = new Color(1f, 0.85f, 0.2f);

        // Agujeros (Spheres negativas/pequeñas)
        for (int i = 0; i < 3; i++) {
            GameObject hole = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hole.transform.SetParent(wedge.transform);
            hole.transform.localPosition = new Vector3(Random.Range(-0.4f, 0.4f), 0.3f, Random.Range(-0.4f, 0.4f));
            hole.transform.localScale = Vector3.one * 0.2f;
            hole.GetComponent<Renderer>().material.color = new Color(0.9f, 0.75f, 0.1f);
        }
    }

    private static void CreateOnion(Transform parent)
    {
        // Cebolla con capas
        GameObject outer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        outer.transform.SetParent(parent);
        outer.transform.localScale = Vector3.one * 0.8f;
        outer.GetComponent<Renderer>().material.color = new Color(0.9f, 0.9f, 0.8f);

        GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        root.transform.SetParent(outer.transform);
        root.transform.localPosition = new Vector3(0, -0.45f, 0);
        root.transform.localScale = new Vector3(0.2f, 0.1f, 0.2f);
        root.GetComponent<Renderer>().material.color = new Color(0.4f, 0.3f, 0.2f);
    }
}
