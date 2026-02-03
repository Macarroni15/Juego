using UnityEngine;

namespace CookingGame
{
    [CreateAssetMenu(fileName = "NewIngredient", menuName = "CookingGame/Ingredient")]
    public class IngredientSO : ScriptableObject
    {
        public string ingredientName;
        public Sprite icon;
        public GameObject prefab;
    }
}
