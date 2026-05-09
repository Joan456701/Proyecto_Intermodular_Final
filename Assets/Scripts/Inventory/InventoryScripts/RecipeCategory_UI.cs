using TMPro;
using UnityEngine;

public class RecipeCategory_UI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private Transform _gridContainer; 

    public Transform GridContainer => _gridContainer;

    public void SetupCategory(string categoryName)
    {
        _titleText.text = categoryName;
    }
}
