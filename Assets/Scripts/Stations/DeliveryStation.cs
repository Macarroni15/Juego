using UnityEngine;

namespace CookingGame
{
    public class DeliveryStation : BaseStation
    {
        public override void Interact(PlayerController player)
        {
            GameObject heldItem = player.GetHeldItem();
            if (heldItem != null)
            {
                // Check if the held item is a completed dish
                // For now, assuming any item can be delivered
                Debug.Log("Item delivered!");
                Destroy(heldItem);
                player.ClearHeldItem();
            }
        }
    }
}
