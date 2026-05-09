using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public abstract class InventoryDisplay : MonoBehaviour
{
    [SerializeField] private MouseItemData _mouseInventoryItem;
    protected InventorySystem _inventorySystem;
    protected Dictionary<InventorySlot_UI, InventorySlot> _slotsDiccionary;
    public InventorySystem InventorySystem => _inventorySystem;
    public Dictionary<InventorySlot_UI, InventorySlot> SlotDiccionary => _slotsDiccionary;
    public abstract void AssignSlot(InventorySystem invToDisplay);

    protected virtual void Start()
    {

    }

    protected virtual void UpdateSlot(InventorySlot updateSlot)
    {
        foreach (var slot in SlotDiccionary)
        {
            if (slot.Value == updateSlot)
            {
                slot.Key.UpdateUISlot(updateSlot);
            }
        }
    }

    public void SlotClicked(InventorySlot_UI clickedUISlot)
    {
        bool isShiftPressed = Keyboard.current.leftShiftKey.isPressed; //Esto habra que cambiarlo introduciendolo en el mapeado que tengo
        if (clickedUISlot.AssignedInventorySlot.ItemData != null && _mouseInventoryItem.AssignedInventorySlot.ItemData == null)
        {
            if(isShiftPressed && clickedUISlot.AssignedInventorySlot.SplitStack(out InventorySlot halfStackSlot))
            {
                _mouseInventoryItem.UpdateMouseSlot(halfStackSlot);
                clickedUISlot.UpdateUISlot();
                return;
            }
            else
            {
                _mouseInventoryItem.UpdateMouseSlot(clickedUISlot.AssignedInventorySlot);
                clickedUISlot.ClearSlot();
                return;
            }
        }

        if (clickedUISlot.AssignedInventorySlot.ItemData == null && _mouseInventoryItem.AssignedInventorySlot.ItemData != null)
        {
            clickedUISlot.AssignedInventorySlot.AssignItem(_mouseInventoryItem.AssignedInventorySlot);
            clickedUISlot.UpdateUISlot();

            _mouseInventoryItem.ClearSlot();
            return;
        }

        if (clickedUISlot.AssignedInventorySlot.ItemData != null && _mouseInventoryItem.AssignedInventorySlot.ItemData != null)
        {
            bool isSameItem = clickedUISlot.AssignedInventorySlot.ItemData == _mouseInventoryItem.AssignedInventorySlot.ItemData;
            if (isSameItem && clickedUISlot.AssignedInventorySlot.EnoughRoomLeftInStack(_mouseInventoryItem.AssignedInventorySlot.StackSize))
            {
                clickedUISlot.AssignedInventorySlot.AssignItem(_mouseInventoryItem.AssignedInventorySlot);
                clickedUISlot.UpdateUISlot();

                _mouseInventoryItem.ClearSlot();
                return;
            }
            else if (isSameItem && 
                !clickedUISlot.AssignedInventorySlot.EnoughRoomLeftInStack(_mouseInventoryItem.AssignedInventorySlot.StackSize, out int leftInStack))
            {
                if (leftInStack < 1) SwapSlots(clickedUISlot);
                else
                {
                    int remainingOnMouse = _mouseInventoryItem.AssignedInventorySlot.StackSize - leftInStack;
                    clickedUISlot.AssignedInventorySlot.AddToStack(leftInStack);
                    clickedUISlot.UpdateUISlot();

                    var newItem = new InventorySlot(_mouseInventoryItem.AssignedInventorySlot.ItemData, remainingOnMouse);
                    _mouseInventoryItem.ClearSlot();
                    _mouseInventoryItem.UpdateMouseSlot(newItem);
                    return;
                }
            }
            else if (!isSameItem)
            {
                SwapSlots(clickedUISlot);
                return;
            }
        }
        
    }

    private void SwapSlots(InventorySlot_UI clickedUISlot)
    {
        var clonedSlot = new InventorySlot(_mouseInventoryItem.AssignedInventorySlot.ItemData, _mouseInventoryItem.AssignedInventorySlot.StackSize);

        _mouseInventoryItem.ClearSlot();

        _mouseInventoryItem.UpdateMouseSlot(clickedUISlot.AssignedInventorySlot);

        clickedUISlot.ClearSlot();

        clickedUISlot.AssignedInventorySlot.AssignItem(clonedSlot);

        clickedUISlot.UpdateUISlot();
    }
}
