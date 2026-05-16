using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class OxygenRecharger : InventoryHolder, IInteractable
{
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    [Header("Configuración de Recarga")]
    [SerializeField] private InventoryItemData _oxygenTankData; 
    [SerializeField] private float _rechargeRate = 5;

    [Header("Efectos")]
    [SerializeField] private Renderer _indicatorLight;
    [SerializeField] private Color _readyColor = Color.green;
    [SerializeField] private Color _rechargingColor = Color.orange;
    [SerializeField] private Color _notReadyColor = Color.red;

    private void Start()
    {
        if (_indicatorLight != null)
        {
            _indicatorLight.material.color = _notReadyColor;
        }
    }

    private void Update()
    {
        if (HasTankInInventory())
        {
            if (OxygenSystem.Instance != null && OxygenSystem.Instance.CurrentOxygenTime < OxygenSystem.Instance.MaxOxygenTime)
            { 
                OxygenSystem.Instance.AddOxygen(_rechargeRate * Time.deltaTime);
                OxygenSystem.InvokeOxygenRecharging(true);

                if (_indicatorLight != null)
                { 
                    _indicatorLight.material.color = _rechargingColor;
                    _indicatorLight.material.SetColor("_EmissionColor", _rechargingColor);
                }
            }
            else if (OxygenSystem.Instance != null && OxygenSystem.Instance.CurrentOxygenTime >= OxygenSystem.Instance.MaxOxygenTime)
            {
                OxygenSystem.InvokeOxygenRecharging(false);

                if (_indicatorLight != null)
                { 
                    _indicatorLight.material.color = _readyColor;
                    _indicatorLight.material.SetColor("_EmissionColor", _readyColor);
                }
            }
        }
        else
        {
            OxygenSystem.InvokeOxygenRecharging(false);

            if (_indicatorLight != null)
            { 
                _indicatorLight.material.color = _notReadyColor;
                _indicatorLight.material.SetColor("_EmissionColor", _notReadyColor);
            }
        }
    }
    
    private bool HasTankInInventory()
    {
        if (_oxygenTankData == null) return false;

        foreach (var slot in PrimaryInventorySystem.InventorySlots)
        {
            if (slot.ItemData == _oxygenTankData && slot.StackSize > 0)
            {
                return true;
            }
        }
        return false;
    }

    public void Interact(Interactor interactor, out bool interactSuccessful)
    {
        OnDynamicInventoryDisplayRequested?.Invoke(PrimaryInventorySystem);
        interactSuccessful = true;
    }

    public void EndInteraction() { }
}
