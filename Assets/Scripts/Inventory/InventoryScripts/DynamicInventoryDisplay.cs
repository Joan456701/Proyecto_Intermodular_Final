using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DynamicInventoryDisplay : InventoryDisplay
{
    [SerializeField] protected InventorySlot_UI _slotPrefab;
    protected override void Start()
    {
        base.Start();
    }

    public void RefreshDynamicInventory(InventorySystem invToDisplay)
    {
        if (_inventorySystem != null) _inventorySystem.OnInventorySlotChanged -= UpdateSlot;

        ClearSlot();
        _inventorySystem = invToDisplay;

        if (_inventorySystem != null) _inventorySystem.OnInventorySlotChanged += UpdateSlot;

        AssignSlot(invToDisplay);
    }

    public override void AssignSlot(InventorySystem invToDisplay)
    {
        _slotsDiccionary = new Dictionary<InventorySlot_UI, InventorySlot>();

        if (invToDisplay == null) return;

        for (int i = 0; i < invToDisplay.InventorySize; i++)
        {
            var uiSlot = Instantiate(_slotPrefab, transform);
            _slotsDiccionary.Add(uiSlot, invToDisplay.InventorySlots[i]);
            uiSlot.Init(invToDisplay.InventorySlots[i]);
            uiSlot.UpdateUISlot();
        }
    }

    private void ClearSlot()
    {
        foreach (var item in transform.Cast<Transform>())
        {
            Destroy(item.gameObject); //Nota: Hay un metodo que es objects pulling que es mejor
        }

        if (_slotsDiccionary != null) _slotsDiccionary.Clear();
    }

    private void OnDisable()
    {
        if (_inventorySystem != null) _inventorySystem.OnInventorySlotChanged -= UpdateSlot;
    }
}
