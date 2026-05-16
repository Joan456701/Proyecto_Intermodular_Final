using UnityEngine;
using System;

public class FirstPersonController : MonoBehaviour, IDamagable
{
    public static event Action<Transform> OnPlayerAttackEvent;
    public static event Action<float, float> OnHealthChanged;

    [Header("Movment Speeds")]
    [SerializeField] private float _walkSpeed = 5f;
    [SerializeField] private float _sprintMultiplier = 2f;

    [Header("Jump")]
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private float _gravityMultiplier = 1f;

    [Header("Look Parameters")]
    [SerializeField] private float _mouseSensitivity = 0.1f;
    [SerializeField] private float _upDownLookRange = 80f;

    [Header("References")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private ToolCooldawnManager _cooldawnManager;
    [SerializeField] private PlayerInputHandler _pInputHandler;
    [SerializeField] private RadialMenuManager _rMenuManager;
    [SerializeField] private Camera _mainCamera;
    private Interactor _interactor;

    [Header("Health Settings")]
    [SerializeField] private float _maxPlayerHealth = 100f;
    [SerializeField] private float _playerHealth;
    [SerializeField] private HungerSystem _hungerSystem;

    [Header("Daño reallizado")]
    [SerializeField] private int _baseDamage = 5;

    [Header("Interaction")]
    [SerializeField] private float _raycastDistance;
    private RaycastHit hitInfo;

    [Header("Equip System & Oxygen")]
    [SerializeField] private Transform _handHolder; 
    [SerializeField] private InventoryItemData _oxygenTankData;

    [Header("Modelos de Brazo")]
    [SerializeField] private GameObject _emptyHandModel;
    [SerializeField] private GameObject _pickaxeModel;
    [SerializeField] private GameObject _axeModel;
    [SerializeField] private GameObject _spearModel;

    private PlayerInventoryHolder _playerInventory;
    private int _currentHotbarIndex = 0;

    private Vector3 _currentMovement;
    private float _verticalRotation;
    private float _currentSpeed => _walkSpeed * (_pInputHandler.sprintTriggered ? _sprintMultiplier : 1f);
    void Start()
    {
        _interactor = GetComponent<Interactor>();
        _playerInventory = GetComponent<PlayerInventoryHolder>();
        _hungerSystem = GetComponent<HungerSystem>();

        _playerHealth = _maxPlayerHealth;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (_playerInventory != null && _playerInventory.PrimaryInventorySystem != null)
        {
            _playerInventory.PrimaryInventorySystem.OnInventorySlotChanged += CheckIfHandNeedsUpdate;
        }
    }

    void Update()
    {
        HandleMovement();

        if (!_rMenuManager.isMenuActive)
        {
            HandleRotation();
        }

        if (_pInputHandler.ConsumeInteractTrigger())
        {
            Vector3 origin = _mainCamera.transform.position;
            Vector3 direction = _mainCamera.transform.forward;

            if (Physics.Raycast(origin, direction, out hitInfo, _raycastDistance))
            {
                IInteractable interactable = hitInfo.collider.GetComponent<IInteractable>();

                if (interactable != null && _interactor != null)
                {
                    interactable.Interact(_interactor, out bool interactSuccessful);
                }
            }
        }

        if (_pInputHandler.eatTriggered)
        {
            Debug.Log(_pInputHandler.eatTriggered);
            _pInputHandler.eatTriggered = false;
            TryConsumeHotbarItem();
        }
    }
    
    private void OnEnable()
    {
        HotbarDisplay.OnHotbarSlotChanged += EquipItem;
        HotbarDisplay.OnHotbarIndexChanged += OnHotbarIndexUpdated;
        if (_cooldawnManager != null)
            _cooldawnManager.OnActionFired += PlayerAttack;
    }

    private void OnDisable()
    {
        HotbarDisplay.OnHotbarSlotChanged -= EquipItem;
        HotbarDisplay.OnHotbarIndexChanged -= OnHotbarIndexUpdated;
        if (_cooldawnManager != null)
            _cooldawnManager.OnActionFired -= PlayerAttack;
    }
    private void OnDestroy()
    {
        if (_playerInventory != null && _playerInventory.PrimaryInventorySystem != null)
        {
            _playerInventory.PrimaryInventorySystem.OnInventorySlotChanged -= CheckIfHandNeedsUpdate;
        }
    }

    private void OnHotbarIndexUpdated(int index)
    {
        _currentHotbarIndex = index;
    }

    private void PlayerAttack()
    {
        if (_pInputHandler.isBuildMode) return;

        OnPlayerAttackEvent?.Invoke(this.transform);

        Vector3 origin = _mainCamera.transform.position;
        Vector3 direction = _mainCamera.transform.forward;

        if (Physics.Raycast(origin, direction, out hitInfo, _raycastDistance,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            IDamagable target = hitInfo.collider.GetComponent<IDamagable>();
            if (target == null) return;
            
            int damage = CalculateDamage(hitInfo.collider.gameObject);
            target.DamageRecived(damage);
        }
    }

    private int CalculateDamage(GameObject targetObject)
    {
        InventorySlot currentSlot = _playerInventory?.GetSlotFromHotbar(_currentHotbarIndex);
        InventoryItemData currentItem = currentSlot?.ItemData;

        if (currentItem == null || currentItem.weaponData == null)
            return _baseDamage;

        ITargetable targetable = targetObject.GetComponent<ITargetable>();
        if (targetable == null) 
            return _baseDamage;

        TargetType targetType = targetable.TargetType;

        if (targetType == TargetType.Indestructible) return 0;

        bool isEffective = System.Array.IndexOf(currentItem.weaponData.effectiveWith, targetType) >= 0;
        return isEffective ? currentItem.weaponData.damage : currentItem.weaponData.damage / 2;
    }

    private Vector3 CalculateWorldDircetion()
    { 
        Vector3 inputDirection = new Vector3(_pInputHandler.movementInput.x, 0, _pInputHandler.movementInput.y);
        Vector3 worldDirection = transform.TransformDirection(inputDirection);
        return worldDirection.normalized;
    }

    private void HandleJumping()
    {
        if (_characterController.isGrounded)
        {
            _currentMovement.y = -0.5f;
            if (_pInputHandler.jumpTriggered)
            {
                _currentMovement.y = _jumpForce;
            }
        }
        else
        {
            _currentMovement.y += Physics.gravity.y * _gravityMultiplier * Time.deltaTime;
        }
    }

    private void HandleMovement()
    {
        Vector3 worldDirection = CalculateWorldDircetion();
        _currentMovement.x = worldDirection.x * _currentSpeed;
        _currentMovement.z = worldDirection.z * _currentSpeed;

        HandleJumping();
        _characterController.Move(_currentMovement * Time.deltaTime);
    }

    private void ApplyHorizontalRotation(float rotationAmount)
    {
        transform.Rotate(0, rotationAmount, 0);
    }

    private void ApplyVerticalRotation(float rotationAmount)
    {
        _verticalRotation = Mathf.Clamp(_verticalRotation + rotationAmount, -_upDownLookRange, _upDownLookRange);
        _mainCamera.transform.localRotation = Quaternion.Euler(_verticalRotation, 0, 0);
    }

    private void HandleRotation()
    {
        float mouseX = _pInputHandler.rotationInput.x * _mouseSensitivity;
        float mouseY = _pInputHandler.rotationInput.y * _mouseSensitivity;
        ApplyHorizontalRotation(mouseX);
        ApplyVerticalRotation(-mouseY);
    }
    private void DrawRaycast()
    {
        Vector3 origin = _mainCamera.transform.position;
        Vector3 direction = _mainCamera.transform.forward;
        Debug.DrawRay(origin, direction * _raycastDistance, Color.red);
    }

    private void TryConsumeHotbarItem()
    {
        if (_playerInventory == null) return;

        InventorySlot slot = _playerInventory.GetSlotFromHotbar(_currentHotbarIndex);
        if (slot == null || slot.ItemData == null) return;

        if (!slot.ItemData.isConsumable) return;

        bool canHeal = slot.ItemData.healAmount > 0 && _playerHealth < _maxPlayerHealth;
        bool canFeed = slot.ItemData.hungerAmount > 0 && _hungerSystem != null;

        if (!canHeal && !canFeed) return;

        if (canHeal) HealPlayer(slot.ItemData.healAmount);
        if (canFeed) _hungerSystem.AddHunger(slot.ItemData.hungerAmount);

        slot.RemoveFromStack(1);

        if (slot.StackSize <= 0)
            slot.ClearSlot();
        
        _playerInventory.PrimaryInventorySystem.OnInventorySlotChanged?.Invoke(slot);
    }

    public void DamageRecived(int damage)
    {
        _playerHealth -= damage;
        OnHealthChanged?.Invoke(_playerHealth, _maxPlayerHealth);

        Debug.Log("Daño recibido: -" + damage + " de vida. Vida actual: " + Mathf.RoundToInt(_playerHealth) + "/" + _maxPlayerHealth);

        if (_playerHealth <= 0)
        {
            _playerHealth = 0;
            GameStateManager.Instance.ChangeGameState(GameState.StateType.OVER);
        }
    }

    public void HealPlayer(int amount)
    {
        if (_playerHealth >= _maxPlayerHealth)
        {
            Debug.Log("Ya tienes la vida al máximo. No puedes consumir más comida.");
            return;
        }

        _playerHealth = Mathf.Min(_playerHealth + amount, _maxPlayerHealth);
        OnHealthChanged?.Invoke(_playerHealth, _maxPlayerHealth);
    }

    public float GetHealth()
    {
        return _playerHealth;
    }

    public float GetMaxHealth()
    {
        return _maxPlayerHealth;
    }

    private void EquipItem(InventoryItemData item)
    {
        if (_emptyHandModel) _emptyHandModel.SetActive(false);
        if (_pickaxeModel) _pickaxeModel.SetActive(false);
        if (_axeModel) _axeModel.SetActive(false);
        if (_spearModel) _spearModel.SetActive(false);

        if (item == null)
        {
            if (_emptyHandModel) _emptyHandModel.SetActive(true);
            return;
        }

        switch (item.toolType)
        {
            case ToolType.Pickaxe:
                if (_pickaxeModel) _pickaxeModel.SetActive(true);
                break;
            case ToolType.Axe:
                if (_axeModel) _axeModel.SetActive(true);
                break;
            case ToolType.Spear:
                if (_spearModel) _spearModel.SetActive(true);
                break;
            default:
                if (_emptyHandModel) _emptyHandModel.SetActive(true);
                break;
        }
    }

    private void CheckIfHandNeedsUpdate(InventorySlot changedSlot)
    {
        InventorySlot currentHandSlot = _playerInventory.GetSlotFromHotbar(_currentHotbarIndex);

        if (changedSlot == currentHandSlot)
        {
            EquipItem(currentHandSlot.ItemData);
        }
    }
}
