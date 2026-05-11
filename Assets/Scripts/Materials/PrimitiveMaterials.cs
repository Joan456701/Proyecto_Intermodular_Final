using UnityEngine;
using UnityEngine.InputSystem.iOS;

public enum TimeCondition
{
    Always,
    OnlyDay,
    OnlyNight
}
[System.Serializable]
public struct LootObject
{
    public InventoryItemData itemData;
    public int minAmount;
    public int maxAmount;

    [Range(0, 100)]
    public int probability;

    public TimeCondition timeCondition;
}
public class PrimitiveMaterials : MonoBehaviour, IDamagable, ITargetable
{
    [Header("Material Settings")]
    [SerializeField] private int _maxHealth;

    private int _currentHealth;

    [Header("Botin estandar")]
    [SerializeField] private LootObject[] _standardDrops;

    [Header("Botin exclusivo")]
    [SerializeField] private LootObject[] _exclusiveDrops;

    [SerializeField] private TargetType _targetType = TargetType.Rock;
    public TargetType TargetType => _targetType;

    void Start()
    {
        _currentHealth = _maxHealth;
    }
    public void DamageRecived(int damage)
    {
        _currentHealth -= damage;

        if (_currentHealth <= 0)
        {
            GiveLoot();
            Destroy(gameObject);
        }
    }

    private void GiveLoot()
    {
        PlayerInventoryHolder playerInventory = FindFirstObjectByType<PlayerInventoryHolder>();
        DayNightSpawnManager timeManager = FindFirstObjectByType<DayNightSpawnManager>();

        if (playerInventory == null) return;

        foreach (LootObject item in _standardDrops)
        {
            TryDropItem(item, playerInventory, timeManager);
        }

        foreach (LootObject item in _exclusiveDrops)
        {
            if (TryDropItem(item, playerInventory, timeManager))
            {
                break;
            }
        }
    }

    private bool TryDropItem(LootObject item, PlayerInventoryHolder inventory, DayNightSpawnManager timeManager)
    {
        if (timeManager != null)
        {
            bool isDay = timeManager.CurrentState == DayNightSpawnManager.DayCycleState.Day;

            if (item.timeCondition == TimeCondition.OnlyNight && isDay) return false;
            if (item.timeCondition == TimeCondition.OnlyDay && !isDay) return false;
        }

        int die = Random.Range(0, 100);

        if (die <= item.probability)
        {
            int dropAmount = Random.Range(item.minAmount, item.maxAmount + 1);

            if (item.itemData != null && dropAmount > 0)
            {
                bool addedToPrimary = inventory.PrimaryInventorySystem.AddToInventory(item.itemData, dropAmount);

                if (!addedToPrimary)
                {
                    inventory.SecondaryInventorySystem.AddToInventory(item.itemData, dropAmount);
                }
                return true; 
            }
        }
        return false;
    }
}
