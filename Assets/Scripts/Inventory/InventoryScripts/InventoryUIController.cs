using UnityEngine;
using UnityEngine.InputSystem;


public class InventoryUIController : MonoBehaviour
{
    public DynamicInventoryDisplay chestPanel;
    public DynamicInventoryDisplay playerBackpackPanel;
    public GameObject craftingPanel;

    private PlayerInputHandler _inputHandler;

    private void Awake()
    {
        chestPanel.gameObject.SetActive(false);
        playerBackpackPanel.gameObject.SetActive(false);
        if (craftingPanel != null) craftingPanel.SetActive(false);
    }

    private void Start()
    {
        _inputHandler = FindFirstObjectByType<PlayerInputHandler>();
    }

    private void OnEnable()
    {
        InventoryHolder.OnDynamicInventoryDisplayRequested += DisplayInventory;
        CraftingTable.OnCraftingDisplayRequested += DisplayCraftingWindow;
    }

    private void OnDisable()
    {
        InventoryHolder.OnDynamicInventoryDisplayRequested -= DisplayInventory;
        CraftingTable.OnCraftingDisplayRequested -= DisplayCraftingWindow;
    }

    void Update()
    {
        if (_inputHandler == null) return;

        bool isCraftingOpen = craftingPanel != null && craftingPanel.activeInHierarchy;
        bool isAnyMenuOpen = chestPanel.gameObject.activeInHierarchy || playerBackpackPanel.gameObject.activeInHierarchy || isCraftingOpen;

        if (_inputHandler.ConsumeInventoryToggle())
        {
            if (isAnyMenuOpen) CloseAllMenus();
            else OpenPlayerBackpack();
        }

        if ((chestPanel.gameObject.activeInHierarchy || isCraftingOpen) && _inputHandler.ConsumeInteractTrigger())
        {
            CloseAllMenus();
        }
    }

    private void DisplayCraftingWindow()
    {
        if (craftingPanel != null) craftingPanel.SetActive(true);

        UpdateGameState(true);
    }

    private void OpenPlayerBackpack()
    {
        PlayerInventoryHolder playerInventory = FindFirstObjectByType<PlayerInventoryHolder>();
        if (playerInventory != null)
        {
            playerBackpackPanel.gameObject.SetActive(true);
            playerBackpackPanel.RefreshDynamicInventory(playerInventory.SecondaryInventorySystem);
            UpdateGameState(true);
        }
    }

    private void DisplayInventory(InventorySystem invToDesplay)
    {
        chestPanel.gameObject.SetActive(true);
        chestPanel.RefreshDynamicInventory(invToDesplay);

        PlayerInventoryHolder playerInventory = FindFirstObjectByType<PlayerInventoryHolder>();
        if (playerInventory != null)
        {
            playerBackpackPanel.gameObject.SetActive(true);
            playerBackpackPanel.RefreshDynamicInventory(playerInventory.SecondaryInventorySystem);
        }

        UpdateGameState(true);
    }

    private void CloseAllMenus()
    {
        chestPanel.gameObject.SetActive(false);
        playerBackpackPanel.gameObject.SetActive(false);
        if (craftingPanel != null) craftingPanel.SetActive(false);
        UpdateGameState(false);
    }

    private void UpdateGameState(bool isMenuOpen)
    {
        Cursor.lockState = isMenuOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isMenuOpen;

        FirstPersonController player = FindFirstObjectByType<FirstPersonController>();
        if (player != null)
        {
            player.enabled = !isMenuOpen;
        }
    }
}