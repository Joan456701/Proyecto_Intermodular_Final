using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class HotbarDisplay : StaticInventoryDisplay
{
    private int currentIndex = 0;
    private int maxIndexSize;
    private PlayerInputHandler _pInputHandler;

    public static event Action<InventoryItemData> OnHotbarSlotChanged;

    protected override void Start()
    {
        base.Start();

        maxIndexSize = _slots.Length - 1;
        _slots[currentIndex].ToggleHighlight();
        _pInputHandler = FindFirstObjectByType<PlayerInputHandler>();

        Invoke(nameof(NotifyCurrentSlot), 0.5f);
    }

    private void Update()
    {
        if (_pInputHandler == null) return;

        if (_pInputHandler.slot1Triggered) ChangeIndex(0);
        if (_pInputHandler.slot2Triggered) ChangeIndex(1);
        if (_pInputHandler.slot3Triggered) ChangeIndex(2);
        if (_pInputHandler.slot4Triggered) ChangeIndex(3);
        if (_pInputHandler.slot5Triggered) ChangeIndex(4);
    }

    private void ChangeIndex(int newIndex)
    {
        if (newIndex > maxIndexSize || currentIndex == newIndex) return;

        _slots[currentIndex].ToggleHighlight();
        currentIndex = newIndex;
        _slots[currentIndex].ToggleHighlight();

        NotifyCurrentSlot();
    }

    private void NotifyCurrentSlot()
    {
        InventoryItemData itemData = _slots[currentIndex].AssignedInventorySlot?.ItemData;
        OnHotbarSlotChanged?.Invoke(itemData);
    }
}
