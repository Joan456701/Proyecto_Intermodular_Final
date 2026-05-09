using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInventoryHolder : InventoryHolder
{
    [SerializeField] protected int _secondaryInventorySize;
    [SerializeField] protected InventorySystem _secondaryInventorySystem;

    private PlayerInputHandler _pInputHandler;
    public InventorySystem SecondaryInventorySystem => _secondaryInventorySystem;
    public static UnityAction<InventorySystem> OnPlayerBackpackDisplayRequested;

    protected override void Awake()
    {
        base.Awake();

        _secondaryInventorySystem = new InventorySystem(_secondaryInventorySize);
        _pInputHandler = GetComponent<PlayerInputHandler>();
    }

    public bool TryConsumeItem(InventoryItemData itemToConsume, int amount)
    {
        if (ConsumeFromSystem(_primaryInventorySystem, itemToConsume, amount)) return true;

        return ConsumeFromSystem(_secondaryInventorySystem, itemToConsume, amount);
    }

    private bool ConsumeFromSystem(InventorySystem system, InventoryItemData itemToConsume, int amount)
    {
        foreach (var slot in system.InventorySlots)
        {
            if (slot.ItemData == itemToConsume && slot.StackSize >= amount)
            {
                slot.RemoveFromStack(amount);
                if (slot.StackSize <= 0) slot.ClearSlot();
                system.OnInventorySlotChanged?.Invoke(slot);
                return true;
            }
        }
        return false;
    }

    public bool AddToInventory(InventoryItemData data, int amount)
    {
        if (_primaryInventorySystem.AddToInventory(data, amount)) 
            return true;

        else if (_secondaryInventorySystem.AddToInventory(data, amount))
            return true;

        return false;
    }

    public InventorySlot GetSlotFromHotbar(int index)
    {
        if (index < 0 || index >= _primaryInventorySystem.InventorySlots.Count) return null;
        return _primaryInventorySystem.InventorySlots[index];
    }
}
