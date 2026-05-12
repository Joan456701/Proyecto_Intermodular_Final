using UnityEngine;

public enum ToolType
{
    None,       
    Pickaxe,    
    Axe,        
    Spear       
}

[CreateAssetMenu(menuName = "Inventory System/Inventory Item")]
public class InventoryItemData : ScriptableObject
{
    [SerializeField] private string _id;
    public string ID => _id;
    public string displayName;
    [TextArea(4, 4)]
    public string description;
    public Sprite icon;
    public int maxStackSize;

    public GameObject itemPrefab;
    public ToolType toolType = ToolType.None;

    [Header("Consumible")]
    public bool isConsumable;
    public float hungerAmount;
    public int healAmount;

    [Header("Herramientas")]
    public WeaponDataSO weaponData;
}
