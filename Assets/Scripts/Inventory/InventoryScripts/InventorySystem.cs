using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using System.Linq;
using UnityEditor.Search;

[System.Serializable]
public class InventorySystem 
{
    [SerializeField] private List<InventorySlot> _inventorySlots;

    public  List<InventorySlot> InventorySlots => _inventorySlots;
    public int InventorySize => InventorySlots.Count;

    public UnityAction<InventorySlot> OnInventorySlotChanged;

    public InventorySystem(int size)
    {
        _inventorySlots = new List<InventorySlot>(size);

        for (int i = 0; i < size; i++)
        {
            InventorySlots.Add(new InventorySlot());
        }
    }

    public bool AddToInventory(InventoryItemData itemToAdd, int amountToAdd)
    {
        if (ContainsItem(itemToAdd, out List<InventorySlot> invSlot))
        {
            foreach (var slot in invSlot)
            {
                if (slot.EnoughRoomLeftInStack(amountToAdd))
                {
                    slot.AddToStack(amountToAdd);
                    OnInventorySlotChanged?.Invoke(slot);
                    return true;
                }
            }
            
        }
        
        if (HasFreeSlot(out InventorySlot freeSlot))
        {
            freeSlot.UpdateInventorySlot(itemToAdd, amountToAdd);
            OnInventorySlotChanged?.Invoke(freeSlot);
            return true;
        }

        return false;
    }

    public bool ContainsItem(InventoryItemData itemToAdd, out List<InventorySlot> invSlot)
    {
        invSlot = InventorySlots.Where(i => i.ItemData == itemToAdd).ToList();

        return invSlot == null ? false : true;
    }

    public bool HasFreeSlot(out InventorySlot freeSlot)
    {
        freeSlot = InventorySlots.FirstOrDefault(i => i.ItemData == null);
        return freeSlot == null ? false : true;
    }
}
