using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public struct ExtractableResources
{
    public InventoryItemData itemData; 
    public int amount;
}

public class ExtractorController : InventoryHolder, IInteractable
{
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    [Header("Items que puede extraer (Arrastrar ScriptableObjects)")]
    [SerializeField] private InventoryItemData _stoneData;
    [SerializeField] private InventoryItemData _coalData;
    [SerializeField] private InventoryItemData _ironData;

    [Header("Variables del funcionamiento")]
    [SerializeField] private float _extractorSpeed;

    [Header("Probabilidades de cada material")]
    [SerializeField] private int _stoneChance = 50;
    [SerializeField] private int _coalChance = 30;
    [SerializeField] private int _metalChance = 20;

    [Header("Probabilidad de la cantidad")]
    [SerializeField] private int _littleAmount = 80;
    [SerializeField] private int _mediumAmount = 18;
    [SerializeField] private int _maxAmount = 2;

    private List<ExtractableResources> _extractedItems = new List<ExtractableResources>();

    private float _timer;

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= _extractorSpeed)
        {
            _timer = 0;

            ExtractTheMaterial();
        }
    }

    private void ExtractTheMaterial()
    {
        InventoryItemData extractedItem = RerollMaterials();
        int materialAmount = RerollAmount();

        if (extractedItem != null)
        {
            PrimaryInventorySystem.AddToInventory(extractedItem, materialAmount);
        }
    }

    private InventoryItemData RerollMaterials()
    {
        int roll = Random.Range(0, 100);
        if (roll < _stoneChance) return _stoneData;
        if (roll < _stoneChance + _coalChance) return _coalData;
        else return _ironData;
    }

    private int RerollAmount()
    {
        int roll = Random.Range(0, 100);
        if (roll < _littleAmount) return 1;
        if (roll < _littleAmount + _mediumAmount) return 3;
        else return 5;
    }

    public void Interact(Interactor interactor, out bool interactSuccessful)
    {
        OnDynamicInventoryDisplayRequested?.Invoke(PrimaryInventorySystem);
        interactSuccessful = true;
    }

    public void EndInteraction() { }
}
