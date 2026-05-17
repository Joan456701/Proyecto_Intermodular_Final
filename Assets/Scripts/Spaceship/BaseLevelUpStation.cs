using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class BaseLevelUpStation : MonoBehaviour, IInteractable
{
    public static event Action<int, Sprite, Sprite, bool> OnBaseLevelChanged;
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    [Header("References")]
    [SerializeField] private SaveZone _saveZone;

    [Header("Level Settings")]
    [SerializeField] private int _currentLevel = 1;
    [SerializeField] private int _maxLevel = 4;

    [Header("Upgrade Cost")]
    [SerializeField] private LevelUpRequirementSO[] _levelRequirements;

    public int CurrentLevel => _currentLevel;

    private void Awake()
    {
        if (_saveZone == null)
            _saveZone = FindFirstObjectByType<SaveZone>();

        ApplyCurrentLevelToSaveZone();
    }
    private void Start()
    {
        BroadcastUIUpdate();
    }

    public void Interact(Interactor interactor, out bool interactSuccessful)
    {
        interactSuccessful = false;

        if (_currentLevel >= _maxLevel)
        {
            Debug.Log("La base ya esta al nivel maximo.");
            return;
        }

        var playerInventory = interactor.GetComponent<PlayerInventoryHolder>();
        if (playerInventory == null) return;

        LevelUpRequirementSO currentRequirement = _levelRequirements[_currentLevel - 1];
        if (currentRequirement == null) return;

        foreach (var req in currentRequirement.requirements)
        {
            int count = GetItemCount(playerInventory, req.itemData);
            if (count < req.amount)
            {
                Debug.Log("Faltan materiales: necesitas " + req.amount + " de " + req.itemData.displayName);
                return;
            }
        }

        foreach (var req in currentRequirement.requirements)
            ConsumeItem(playerInventory, req.itemData, req.amount);

        _currentLevel++;
        ApplyCurrentLevelToSaveZone();
        BroadcastUIUpdate();

        Debug.Log("Base mejorada al nivel " + _currentLevel + ".");
        interactSuccessful = true;
        OnInteractionComplete?.Invoke(this);
    }

    private void BroadcastUIUpdate()
    {
        int displayLevel = _currentLevel - 1;
        bool isMax = _currentLevel >= _maxLevel;

        Sprite icon1 = null;
        Sprite icon2 = null;

        if (!isMax)
        {
            LevelUpRequirementSO currentRequirement = _levelRequirements[_currentLevel - 1];
            if (currentRequirement != null && currentRequirement.requirements.Length >= 2)
            {
                icon1 = currentRequirement.materialOne;
                icon2 = currentRequirement.materialTwo;
            }
        }

        OnBaseLevelChanged?.Invoke(displayLevel, icon1, icon2, isMax);
    }

    public void EndInteraction() { }

    private int GetItemCount(PlayerInventoryHolder inventory, InventoryItemData itemToCheck)
    {
        int count = 0;
        foreach (var slot in inventory.PrimaryInventorySystem.InventorySlots)
            if (slot.ItemData == itemToCheck) count += slot.StackSize;

        foreach (var slot in inventory.SecondaryInventorySystem.InventorySlots)
            if (slot.ItemData == itemToCheck) count += slot.StackSize;

        return count;
    }

    private void ConsumeItem(PlayerInventoryHolder inventory, InventoryItemData itemToConsume, int amount)
    {
        int amountToRemove = amount;
        foreach (var slot in inventory.PrimaryInventorySystem.InventorySlots)
        {
            if (slot.ItemData == itemToConsume)
            {
                int toTake = Mathf.Min(slot.StackSize, amountToRemove);
                slot.RemoveFromStack(toTake);
                amountToRemove -= toTake;

                if (slot.StackSize <= 0) slot.ClearSlot();
                inventory.PrimaryInventorySystem.OnInventorySlotChanged?.Invoke(slot);

                if (amountToRemove <= 0) return; 
            }
        }

        if (amountToRemove > 0 && inventory.SecondaryInventorySystem != null)
        {
            foreach (var slot in inventory.SecondaryInventorySystem.InventorySlots)
            {
                if (slot.ItemData == itemToConsume)
                {
                    int toTake = Mathf.Min(slot.StackSize, amountToRemove);
                    slot.RemoveFromStack(toTake);
                    amountToRemove -= toTake;

                    if (slot.StackSize <= 0) slot.ClearSlot();
                    inventory.SecondaryInventorySystem.OnInventorySlotChanged?.Invoke(slot);

                    if (amountToRemove <= 0) return; 
                }
            }
        }
    }
    private void ApplyCurrentLevelToSaveZone()
    {
        if (_saveZone == null) return;
        _currentLevel = Mathf.Clamp(_currentLevel, 1, Mathf.Max(1, _maxLevel));
        _saveZone.ApplyBaseLevel(_currentLevel);
    }
}
