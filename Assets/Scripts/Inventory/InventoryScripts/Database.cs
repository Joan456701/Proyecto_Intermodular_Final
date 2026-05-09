using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory System/Item Database")]
public class Database : ScriptableObject
{
    [SerializeField] private List<InventoryItemData> _itemDatabase;
    public List<InventoryItemData> ItemDatabase => _itemDatabase;

    [ContextMenu("Load Items Automatically")]
    public void LoadItems()
    {
        _itemDatabase = new List<InventoryItemData>();

        var foundItems = Resources.LoadAll<InventoryItemData>("ItemData");

        _itemDatabase.AddRange(foundItems);
    }

    public InventoryItemData GetItem(string searchID)
    {
        return _itemDatabase.FirstOrDefault(i => i.ID == searchID);
    }
}
