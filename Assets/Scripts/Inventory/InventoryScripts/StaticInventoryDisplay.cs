
using System.Collections.Generic;
using UnityEngine;

public class StaticInventoryDisplay : InventoryDisplay
{
    [SerializeField] private InventoryHolder _inventoryHolder;
    [SerializeField] protected InventorySlot_UI[] _slots;

    protected override void Start()
    {
        base.Start();

        if (_inventoryHolder != null)
        {
            _inventorySystem = _inventoryHolder.PrimaryInventorySystem;
            InventorySystem.OnInventorySlotChanged += UpdateSlot;
        }

        AssignSlot(_inventorySystem);
    }
    public override void AssignSlot(InventorySystem invToDisplay)
    {
        _slotsDiccionary = new Dictionary<InventorySlot_UI, InventorySlot>();

        for (int i = 0; i < _inventorySystem.InventorySize; i++)
        {
            _slotsDiccionary.Add(_slots[i], _inventorySystem.InventorySlots[i]);
            _slots[i].Init(_inventorySystem.InventorySlots[i]);
        }
    }
}
