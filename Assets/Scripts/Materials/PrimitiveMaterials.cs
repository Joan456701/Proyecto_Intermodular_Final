using UnityEngine;

[System.Serializable]
public struct LootObject
{
    public InventoryItemData itemData;
    public int minAmount;
    public int maxAmount;

    [Range(0, 100)]
    public int probability;
}
public class PrimitiveMaterials : MonoBehaviour, IDamagable
{
    [Header("Material Settings")]
    [SerializeField] private int _maxHealth;

    private int _currentHealth;

    [Header("List of Materials")]
    [SerializeField] private LootObject[] lootObject;

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

        if (playerInventory == null) return;

        foreach (LootObject item in lootObject)
        {
            int die = Random.Range(0, 100);

            if (die <= item.probability)
            {
                int dropAmount = Random.Range(item.minAmount, item.maxAmount + 1);

                if (item.itemData != null && dropAmount > 0)
                {
                    bool addToPrimary = playerInventory.PrimaryInventorySystem.AddToInventory(item.itemData, dropAmount);

                    if (!addToPrimary)
                    {
                        playerInventory.SecondaryInventorySystem.AddToInventory(item.itemData, dropAmount);
                    }
                }
            }
        }
    }
}
