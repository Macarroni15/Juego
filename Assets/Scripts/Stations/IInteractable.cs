using UnityEngine;

namespace CookingGame
{
    public interface IInteractable
    {
        void Interact(PlayerController player);
        bool CanInteract(PlayerController player);
    }
}
