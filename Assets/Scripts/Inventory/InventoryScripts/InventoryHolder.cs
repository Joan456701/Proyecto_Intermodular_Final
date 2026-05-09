using JetBrains.Annotations;
using System;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class InventoryHolder : MonoBehaviour
{
    [SerializeField] private int _inventorySize;
    [SerializeField] protected InventorySystem _primaryInventorySystem;

    public InventorySystem PrimaryInventorySystem => _primaryInventorySystem;

    public static UnityAction<InventorySystem> OnDynamicInventoryDisplayRequested;

    protected virtual void Awake()
    {
        _primaryInventorySystem = new InventorySystem(_inventorySize);
    }
}
