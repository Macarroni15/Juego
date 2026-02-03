using UnityEngine;

public class DispenserStation : MonoBehaviour, IInteractable
{
    public IngredientSO ingredient;

    public void Interact(PlayerController player)
    {
        if (player.GetHeldItem() == null)
        {
            GameObject itemObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            itemObj.transform.localScale = Vector3.one * 0.4f;
            itemObj.name = ingredient.ingredientName;
            
            IngredientItem item = itemObj.AddComponent<IngredientItem>();
            item.data = ingredient;
            
            itemObj.GetComponent<Renderer>().material.color = GetColor(ingredient.ingredientName);
            player.SetHeldItem(itemObj);
        }
    }

    private Color GetColor(string name)
    {
        switch (name.ToLower())
        {
            case "tomato": return Color.red;
            case "lettuce": return Color.green;
            case "cheese": return Color.yellow;
            case "meat": return new Color(0.5f, 0.25f, 0);
            default: return Color.white;
        }
    }
}
