using UnityEngine;

namespace CookingGame
{
    public class BaseStation : MonoBehaviour, IInteractable
    {
        [SerializeField] protected Transform topPoint;
        protected GameObject currentItem;

        public virtual void Interact(PlayerController player)
        {
            // Default interaction: give or take item
            if (currentItem != null)
            {
                // Logic to give item to player
            }
        }

        public virtual bool CanInteract(PlayerController player)
        {
            return true;
        }

        public bool HasItem() => currentItem != null;
    }
}
