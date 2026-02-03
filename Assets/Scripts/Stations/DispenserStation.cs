using UnityEngine;

namespace CookingGame
{
    public class DispenserStation : BaseStation
    {
        [SerializeField] private IngredientSO ingredient;

        public override void Interact(PlayerController player)
        {
            // Logic to instantiate ingredient and give it to player
            Debug.Log("Dispensing: " + ingredient.ingredientName);
        }
    }
}
