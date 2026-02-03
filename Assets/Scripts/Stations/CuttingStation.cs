using UnityEngine;
using System.Collections.Generic;

namespace CookingGame
{
    public class CuttingStation : BaseStation
    {
        [SerializeField] private List<RecipeSO> recipes; // Or specific cut logic

        public override void Interact(PlayerController player)
        {
            // Logic for cutting ingredients
            Debug.Log("Cutting item...");
        }
    }
}
