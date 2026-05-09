using UnityEngine;

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
}
