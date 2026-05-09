using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.EventSystems;
using NUnit.Framework;
using System.Collections.Generic;

public class MouseItemData : MonoBehaviour
{
    public Image itemSprite;
    public TextMeshProUGUI itemCount;
    public InventorySlot AssignedInventorySlot;

    private Camera _mainCamera;

    private void Awake()
    {
        itemSprite.preserveAspect = true;
        itemSprite.color = Color.clear;
        itemCount.text = "";
    }

    private void Start()
    {
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            _mainCamera = FindFirstObjectByType<Camera>();
        }
    }

    public void UpdateMouseSlot(InventorySlot invSlot)
    {
        AssignedInventorySlot.AssignItem(invSlot);
        itemSprite.sprite = invSlot.ItemData.icon;
        itemCount.text = invSlot.StackSize.ToString();
        itemSprite.color = Color.white;
    }

    private void Update()
    {
        if (AssignedInventorySlot.ItemData != null)
        {
            transform.position = Mouse.current.position.ReadValue();

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (!IsPointerOverUIObject())
                {
                    DropAllItems();
                }
            }
            else if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                if (!IsPointerOverUIObject())
                {
                    DropOneItem();
                }
            }
        }
    }

    public void ClearSlot()
    {
        AssignedInventorySlot.ClearSlot();
        itemCount.text = "";
        itemSprite.color = Color.clear;
        itemSprite.sprite = null;
    }

    private void DropAllItems()
    {
        InventoryItemData itemDataToDrop = AssignedInventorySlot.ItemData;
        int amountToDrop = AssignedInventorySlot.StackSize;

        if (itemDataToDrop != null && itemDataToDrop.itemPrefab != null)
        {
            if (_mainCamera != null)
            {
                for (int i = 0; i < amountToDrop; i++)
                {
                    SpawnPhysicalItem(itemDataToDrop);
                }
            }
        }
        ClearSlot();
    }

    private void DropOneItem()
    {
        InventoryItemData itemDataToDrop = AssignedInventorySlot.ItemData;

        if (itemDataToDrop != null && itemDataToDrop.itemPrefab != null)
        {
            if (_mainCamera != null)
            {
                SpawnPhysicalItem(itemDataToDrop);
            }
        }

        if (AssignedInventorySlot.StackSize > 1)
        {
            AssignedInventorySlot.RemoveFromStack(1);
            itemCount.text = AssignedInventorySlot.StackSize.ToString();
        }
        else
        {
            ClearSlot();
        }
    }

    private void SpawnPhysicalItem(InventoryItemData itemDataToDrop)
    {
        Vector3 randomOffset = new Vector3(Random.Range(-0.3f, 0.3f), 0, Random.Range(-0.3f, 0.3f));
        Vector3 spawnPosition = _mainCamera.transform.position + _mainCamera.transform.forward * 1.5f + randomOffset;
        spawnPosition.y = Mathf.Max(spawnPosition.y, 0.5f);

        GameObject droppedItem = Instantiate(itemDataToDrop.itemPrefab, spawnPosition, Quaternion.identity);

        if (!droppedItem.TryGetComponent(out Rigidbody rb))
        {
            rb = droppedItem.AddComponent<Rigidbody>();
        }

        if (droppedItem.GetComponent<Collider>() == null)
        {
            var col = droppedItem.AddComponent<BoxCollider>();
        }

        if (!droppedItem.TryGetComponent(out PickUpItem pickUpScript))
        {
            pickUpScript = droppedItem.AddComponent<PickUpItem>();
        }
        pickUpScript.itemData = itemDataToDrop;
    }

    public bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = Mouse.current.position.ReadValue();
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        for (int i = 0; i < results.Count; i++)
        {
            if (results[i].gameObject == this.gameObject || results[i].gameObject == itemSprite.gameObject || results[i].gameObject == itemCount.gameObject)
            {
                results.RemoveAt(i);
                i--;
            }
        }

        if (results.Count > 0)
        {
            return true;
        }

        return false;
    }
}
