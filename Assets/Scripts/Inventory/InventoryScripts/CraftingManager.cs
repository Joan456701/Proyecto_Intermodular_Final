using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CraftingManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerInventoryHolder _playerInventory;

    [Header("Recetas Disponibles")]
    [SerializeField] private List<CraftingRecipeData> _availableRecipes;

    [Header("Referencias de la Interfaz (UI)")]
    [SerializeField] private Transform _categoriesContainer;
    [SerializeField] private GameObject _categoryPrefab;
    [SerializeField] private GameObject _recipeButtonPrefab;

    private void Start()
    {
        GenerateCraftingUI();
    }
    private void GenerateCraftingUI()
    {
        Dictionary<RecipeCategory, RecipeCategory_UI> categoryPanels = new Dictionary<RecipeCategory, RecipeCategory_UI>();

        foreach (CraftingRecipeData recipe in _availableRecipes)
        {
            if (!categoryPanels.ContainsKey(recipe.category))
            {
                GameObject newCategoryObj = Instantiate(_categoryPrefab, _categoriesContainer);
                RecipeCategory_UI categoryUI = newCategoryObj.GetComponent<RecipeCategory_UI>();

                categoryUI.SetupCategory(recipe.category.ToString());
                categoryPanels.Add(recipe.category, categoryUI);
            }

            Transform gridForThisRecipe = categoryPanels[recipe.category].GridContainer;
            GameObject newRecipeButton = Instantiate(_recipeButtonPrefab, gridForThisRecipe);

            RecipeSlot_UI recipeSlotUI = newRecipeButton.GetComponent<RecipeSlot_UI>();
            recipeSlotUI.Init(recipe, this);
        }
    }
    public bool CanCraft(CraftingRecipeData recipe, InventorySystem inventory)
    {
        foreach (var ingredient in recipe.ingredients)
        {
            int count = 0;
            foreach (var slot in inventory.InventorySlots)
            {
                if (slot.ItemData == ingredient.item) count += slot.StackSize;
            }

            if (count < ingredient.amount) return false;
        }
        return true;
    }
    public void Craft(CraftingRecipeData recipe, InventorySystem inventory)
    {
        if (!CanCraft(recipe, inventory)) return;

        foreach (var ingredient in recipe.ingredients)
        {
            int amountToRemove = ingredient.amount;
            foreach (var slot in inventory.InventorySlots)
            {
                if (slot.ItemData == ingredient.item)
                {
                    int toTake = Mathf.Min(slot.StackSize, amountToRemove);
                    slot.RemoveFromStack(toTake);
                    amountToRemove -= toTake;

                    if (slot.StackSize <= 0) slot.ClearSlot();
                    inventory.OnInventorySlotChanged?.Invoke(slot);

                    if (amountToRemove <= 0) break;
                }
            }
        }
        _playerInventory.AddToInventory(recipe.resultItem, recipe.resultAmount);
    }
    public void OnCraftButtonClicked(int recipeIndex)
    {
        if (recipeIndex >= 0 && recipeIndex < _availableRecipes.Count)
        {
            CraftingRecipeData selectedRecipe = _availableRecipes[recipeIndex];
            Craft(selectedRecipe, _playerInventory.PrimaryInventorySystem);
        }
    }
}