using UnityEngine;

[RequireComponent (typeof(SphereCollider))]
public class PickUpItem : MonoBehaviour
{
    public float pickUpRadius = 1f;
    public InventoryItemData itemData;

    private SphereCollider _collider;

    private void Awake()
    {
        _collider = GetComponent<SphereCollider>();
        _collider.isTrigger = true;
        _collider.radius = pickUpRadius;
    }

    private void OnTriggerEnter(Collider other)
    {
        var inventory = other.transform.GetComponent<PlayerInventoryHolder>();
        
        if (!inventory) return;

        if (inventory.AddToInventory(itemData, 1))
        {
            Destroy(this.gameObject);
        }
    }
}
