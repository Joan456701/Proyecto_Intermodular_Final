using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class BaseLevelUpStation : MonoBehaviour, IInteractable
{
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    [Header("References")]
    [SerializeField] private SaveZone _saveZone;

    [Header("Level Settings")]
    [SerializeField] private int _currentLevel = 1;
    [SerializeField] private int _maxLevel = 3;

    [Header("Upgrade Cost")]
    [SerializeField] private InventoryItemData _stickItemData;
    [SerializeField] private InventoryItemData _stoneItemData;
    [SerializeField] private int _baseStickCost = 2;
    [SerializeField] private int _baseStoneCost = 3;

    public int CurrentLevel => _currentLevel;

    private void Awake()
    {
        if (_saveZone == null)
        {
            _saveZone = FindFirstObjectByType<SaveZone>();
        }
        ApplyCurrentLevelToSaveZone();
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
        if (playerInventory == null || _stickItemData == null || _stoneItemData == null) return;

        int stickCost = GetStickCostForNextLevel();
        int stoneCost = GetStoneCostForNextLevel();

        if (GetItemCount(playerInventory, _stickItemData) < stickCost ||
            GetItemCount(playerInventory, _stoneItemData) < stoneCost)
        {
            Debug.Log("Faltan materiales para mejorar la base. Necesitas " + stickCost + " palos y " + stoneCost + " piedras.");
            return;
        }

        ConsumeItem(playerInventory, _stickItemData, stickCost);
        ConsumeItem(playerInventory, _stoneItemData, stoneCost);

        _currentLevel++;
        ApplyCurrentLevelToSaveZone();

        Debug.Log("Base mejorada al nivel " + _currentLevel + ".");
        interactSuccessful = true;
        OnInteractionComplete?.Invoke(this);
    }

    public void EndInteraction() { }

    private int GetItemCount(PlayerInventoryHolder inventory, InventoryItemData itemToCheck)
    {
        int count = 0;
        foreach (var slot in inventory.PrimaryInventorySystem.InventorySlots)
        {
            if (slot.ItemData == itemToCheck) count += slot.StackSize;
        }
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

                if (amountToRemove <= 0) break;
            }
        }
    }

    private int GetStickCostForNextLevel() => GetScaledCost(_baseStickCost);
    private int GetStoneCostForNextLevel() => GetScaledCost(_baseStoneCost);
    private int GetScaledCost(int baseCost)
    {
        int upgradesAlreadyDone = Mathf.Max(0, _currentLevel - 1);
        return Mathf.Max(1, baseCost * (int)Mathf.Pow(2, upgradesAlreadyDone));
    }

    private void ApplyCurrentLevelToSaveZone()
    {
        if (_saveZone == null) return;
        _currentLevel = Mathf.Clamp(_currentLevel, 1, Mathf.Max(1, _maxLevel));
        _saveZone.ApplyBaseLevel(_currentLevel);
    }
}
