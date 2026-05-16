using System;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class GasoilStation : InventoryHolder, IInteractable
{
    public static event Action<float, float> OnShieldEnergyChanged;
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    [System.Serializable]
    public struct FuelType
    {
        public InventoryItemData itemData;
        public float energyAmount;
    }

    [Header("Configuración del Escudo")]
    [SerializeField] private float _maxShieldEnergy = 250; // Esto ahora mismp equivale a 2:30min
    [SerializeField] private float _currentShieldEnergy;

    public bool IsShieldActive => _currentShieldEnergy > 0;

    [Header("Combustibles Permitidos")]
    [SerializeField] private FuelType[] _allowedFuels;

    [Header("Ajustes de Consumo")]
    [SerializeField] private float _refuelCheckRate = 2f;
    private float _timer;

    private void Start()
    {
        _currentShieldEnergy = _maxShieldEnergy;
        OnShieldEnergyChanged?.Invoke(_currentShieldEnergy, _maxShieldEnergy);
    }
    private void Update()
    {
        if (_currentShieldEnergy > 0)
        {
            _currentShieldEnergy -= Time.deltaTime;
            OnShieldEnergyChanged?.Invoke(_currentShieldEnergy, _maxShieldEnergy);
        }
        else
        {
            _currentShieldEnergy = 0;
        }

        _timer += Time.deltaTime;
        if (_timer >= _refuelCheckRate)
        {
            _timer = 0;
            if (_currentShieldEnergy < _maxShieldEnergy)
            {
                ConsumeAvailableFuel();
            }
        }
    }

    private void ConsumeAvailableFuel()
    {
        foreach (var slot in PrimaryInventorySystem.InventorySlots)
        {
            if (slot.ItemData != null && slot.StackSize > 0)
            {
                if (TryGetFuelData(slot.ItemData, out FuelType matchedFuel))
                {
                    slot.RemoveFromStack(1);
                    if (slot.StackSize <= 0) slot.ClearSlot();
                    PrimaryInventorySystem.OnInventorySlotChanged?.Invoke(slot);

                    _currentShieldEnergy = Mathf.Min(_currentShieldEnergy + matchedFuel.energyAmount, _maxShieldEnergy);
                    OnShieldEnergyChanged?.Invoke(_currentShieldEnergy, _maxShieldEnergy);

                    return;
                }
            }
        }
    }

    private bool TryGetFuelData(InventoryItemData itemToCheck, out FuelType fuelData)
    {
        foreach (var fuel in _allowedFuels)
        {
            if (fuel.itemData == itemToCheck) { fuelData = fuel; return true; }
        }
        fuelData = default; 
        return false;
    }
    public void Interact(Interactor interactor, out bool interactSuccessful)
    {
        OnDynamicInventoryDisplayRequested?.Invoke(PrimaryInventorySystem);
        interactSuccessful = true;
    }
    public void EndInteraction() { }
}
