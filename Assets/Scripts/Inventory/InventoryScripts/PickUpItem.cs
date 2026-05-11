using UnityEngine;

[RequireComponent (typeof(SphereCollider))]
[RequireComponent (typeof(Rigidbody))]
public class PickUpItem : MonoBehaviour
{
    public float pickUpRadius = 1f;
    public InventoryItemData itemData;

    private SphereCollider _collider;
    
    [SerializeField] private float _pickupDelay = .75f;
    private bool _canPickUp = false;

    private void Awake()
    {
        _collider = GetComponent<SphereCollider>();
        _collider.isTrigger = true;
        _collider.radius = pickUpRadius;
    }

    private void Update()
    {
        if (!_canPickUp)
        {
            _pickupDelay -= Time.deltaTime;
            if (_pickupDelay <= 0)
            {
                _canPickUp = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_canPickUp) return;

        var inventory = other.transform.GetComponent<PlayerInventoryHolder>();
        
        if (!inventory) return;

        if (inventory.AddToInventory(itemData, 1))
        {
            Destroy(this.gameObject);
        }
    }
}
