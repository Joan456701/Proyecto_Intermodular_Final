using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RecipeSlot_UI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image _recipeIcon;
    private CraftingRecipeData _assignedRecipe;
    private CraftingManager _craftingManager;

    public void Init(CraftingRecipeData recipe, CraftingManager manager)
    {
        _assignedRecipe = recipe;
        _craftingManager = manager;
        _recipeIcon.sprite = recipe.resultItem.icon;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            _craftingManager.Craft(_assignedRecipe, FindFirstObjectByType<PlayerInventoryHolder>().PrimaryInventorySystem);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_assignedRecipe != null && RecipeTooltipUI.Instance != null)
        {
            RecipeTooltipUI.Instance.ShowTooltip(_assignedRecipe);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (RecipeTooltipUI.Instance != null)
        {
            RecipeTooltipUI.Instance.HideTooltip();
        }
    }
}
