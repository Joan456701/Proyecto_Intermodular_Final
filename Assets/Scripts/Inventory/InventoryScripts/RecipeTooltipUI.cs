using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class RecipeTooltipUI : MonoBehaviour
{
    public static RecipeTooltipUI Instance { get; private set; }

    [Header("Referencias de UI")]
    [SerializeField] private TextMeshProUGUI _recipeNameText;
    [SerializeField] private TextMeshProUGUI _recipeDescriptionText;

    [Header("Ingredientes")]
    [SerializeField] private Transform _ingredientsContainer;
    [SerializeField] private GameObject _ingredientPrefab;

    [Header("Ajustes")]
    [SerializeField] private Vector2 _mouseOffset = new Vector2(15f, -15f); 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        gameObject.SetActive(false); 
    }

    private void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            transform.position = mousePos + _mouseOffset;
        }
    }

    public void ShowTooltip(CraftingRecipeData recipe)
    {
        _recipeNameText.text = recipe.resultItem.displayName;
        _recipeDescriptionText.text = recipe.resultItem.description;

        foreach (Transform child in _ingredientsContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (Ingredient ingredient in recipe.ingredients)
        {
            GameObject newIngObj = Instantiate(_ingredientPrefab, _ingredientsContainer);
            IngredientUI ingredientUI = newIngObj.GetComponent<IngredientUI>();

            ingredientUI.Setup(ingredient.item, ingredient.amount);
        }

        gameObject.SetActive(true);
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }
}
