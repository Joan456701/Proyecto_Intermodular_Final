using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class GasoilStation : MonoBehaviour, IInteractable
{
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    [Header("Settings")]
    [SerializeField] private InventoryItemData _carbonItemData;
    [SerializeField] private int _carbonCost = 1;

    public void Interact(Interactor interactor, out bool interactSuccessful)
    {
        interactSuccessful = false;
        var playerInventory = interactor.GetComponent<PlayerInventoryHolder>();

        if (playerInventory == null || _carbonItemData == null) return;

        int currentCarbon = 0;
        foreach (var slot in playerInventory.PrimaryInventorySystem.InventorySlots)
        {
            if (slot.ItemData == _carbonItemData) currentCarbon += slot.StackSize;
        }

        if (currentCarbon < _carbonCost)
        {
            return;
        }

        int amountToRemove = _carbonCost;
        foreach (var slot in playerInventory.PrimaryInventorySystem.InventorySlots)
        {
            if (slot.ItemData == _carbonItemData)
            {
                int toTake = Mathf.Min(slot.StackSize, amountToRemove);
                slot.RemoveFromStack(toTake);
                amountToRemove -= toTake;

                if (slot.StackSize <= 0) slot.ClearSlot();
                playerInventory.PrimaryInventorySystem.OnInventorySlotChanged?.Invoke(slot);

                if (amountToRemove <= 0) break;
            }
        }

        if (OxygenSystem.Instance != null)
        {
            OxygenSystem.Instance.RefillOxygen();
        }

        Debug.Log("Has rellenado la gasolina usando carbón");
        interactSuccessful = true;
        OnInteractionComplete?.Invoke(this);
    }

    public void EndInteraction() { }
}
