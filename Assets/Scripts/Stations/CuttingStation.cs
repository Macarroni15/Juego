using UnityEngine;

public class CuttingStation : MonoBehaviour, IInteractable
{
    public void Interact(PlayerController player)
    {
        GameObject held = player.GetHeldItem();
        if (held != null && held.TryGetComponent(out IngredientItem item))
        {
            if (item.data.canBeCut && !item.isCut)
            {
                item.isCut = true;
                held.transform.localScale = new Vector3(0.4f, 0.1f, 0.4f);
                held.name = "Cut " + item.data.ingredientName;
            }
        }
    }
}
