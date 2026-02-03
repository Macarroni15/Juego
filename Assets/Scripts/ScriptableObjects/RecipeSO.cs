using UnityEngine;
using System.Collections.Generic;

namespace CookingGame
{
    public enum HealthProblem { Migraine, Acidity, SportRecovery }

    [CreateAssetMenu(fileName = "NewRecipe", menuName = "CookingGame/Recipe")]
    public class RecipeSO : ScriptableObject
    {
        public string dishName;
        public HealthProblem recommendedFor;
        public List<IngredientSO> requiredIngredients;
        public GameObject resultPrefab;
    }
}
