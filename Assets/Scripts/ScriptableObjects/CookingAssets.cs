using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ingredient", menuName = "Cooking/Ingredient")]
public class IngredientSO : ScriptableObject
{
    public string ingredientName;
    public bool canBeCut;
}

[CreateAssetMenu(fileName = "Recipe", menuName = "Cooking/Recipe")]
public class RecipeSO : ScriptableObject
{
    public string recipeName;
    public List<IngredientSO> ingredients;
}
