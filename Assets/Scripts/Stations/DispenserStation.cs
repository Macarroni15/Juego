using UnityEngine;

public class DispenserStation : MonoBehaviour, IInteractable
{
    public IngredientSO ingredient;
    public string ingredientName; // Allow override for procedural generation

    public void Interact(PlayerController player)
    {
        if (player.GetHeldItem() == null)
        {
            // Ensure we have an IngredientSO
            if (ingredient == null && !string.IsNullOrEmpty(ingredientName))
            {
                ingredient = ScriptableObject.CreateInstance<IngredientSO>();
                ingredient.name = ingredientName;
                ingredient.ingredientName = ingredientName;
                // Default logic: most things can be cut for now
                ingredient.canBeCut = true; 
            }

            if (ingredient != null)
            {
                GameObject itemObj = new GameObject(ingredient.ingredientName);
                itemObj.transform.localScale = Vector3.one * 0.4f;
                
                IngredientItem item = itemObj.AddComponent<IngredientItem>();
                item.data = ingredient;
                
                IngredientVisualizer.BuildVisual(itemObj, ingredient.ingredientName, false);
                player.SetHeldItem(itemObj);
            }
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
