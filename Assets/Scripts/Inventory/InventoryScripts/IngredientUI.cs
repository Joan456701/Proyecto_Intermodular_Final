using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientUI : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _amountText;

    public void Setup(InventoryItemData itemData, int amount)
    {
        _iconImage.sprite = itemData.icon;
        _amountText.text = "x" + amount.ToString();
    }
}
