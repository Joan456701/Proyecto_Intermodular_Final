using System;
using System.Collections.Generic;
using UnityEngine;
public enum RecipeCategory
{
    Minerales,
    Herraminetas
}

[Serializable]
public struct Ingredient
{
    public InventoryItemData item;
    public int amount;
}

[CreateAssetMenu(menuName = "Inventory System/Crafting Recipe")]
public class CraftingRecipeData : ScriptableObject
{
    public RecipeCategory category;
    public List<Ingredient> ingredients;
    public InventoryItemData resultItem;
    public int resultAmount = 1;
}
