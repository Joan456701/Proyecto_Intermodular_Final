using System;
using UnityEngine;

public class OxygenSystem : MonoBehaviour
{
    public static event Action<float, float> OnOxygenChanged;
    public static event Action<bool> OnTankEquippedChanged;
    public static event Action<bool> OnOxygenRecharging;

    [Header("Oxygen Settings")]
    [SerializeField] private float _maxOxygenTime = 60f;
    [SerializeField] private float _currentOxygenTime;
    [SerializeField] private bool _isPaused;
    [SerializeField] private bool _hasOxygenTank;

    [Header("Damage Settings")]
    [SerializeField] private float _damagePerSecond = 5f;
    [SerializeField] private bool _isTakingOxygenDamage;
    [SerializeField] private float _oxygenDamageTimer;

    [Header("Death Settings")]
    [SerializeField] private FirstPersonController _firstPersonController;

    private static OxygenSystem _instance;
    public static OxygenSystem Instance => _instance;

    public float CurrentOxygenTime => _currentOxygenTime;
    public float MaxOxygenTime => _maxOxygenTime;

    public bool IsPaused => _isPaused;
    public bool HasOxygenTank => _hasOxygenTank;

    private PlayerInventoryHolder _playerInventory;
    [SerializeField] private InventoryItemData _oxygenTankData; 

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    private void Start()
    {
        _currentOxygenTime = _maxOxygenTime;
        _hasOxygenTank = false;
        _isPaused = false;
        _isTakingOxygenDamage = false;
        _oxygenDamageTimer = 0f;

        if (_firstPersonController == null)
        {
            _firstPersonController = FindFirstObjectByType<FirstPersonController>();
        }

        _playerInventory = FindFirstObjectByType<PlayerInventoryHolder>();
        OnTankEquippedChanged?.Invoke(_hasOxygenTank);
    }

    private void Update()
    {
        CheckForTank();

        if (Time.timeScale <= 0) return;

        if (_isPaused)
        {
            _isTakingOxygenDamage = false;
            _oxygenDamageTimer = 0f; 
            return; 
        }


        if (_hasOxygenTank && _currentOxygenTime > 0)
        {
            _currentOxygenTime -= Time.deltaTime;
            OnOxygenChanged?.Invoke(_currentOxygenTime, _maxOxygenTime);
            _isTakingOxygenDamage = false;
            _oxygenDamageTimer = 0f;

            if (_currentOxygenTime <= 0)
            {
                _currentOxygenTime = 0;
            }
        }
        else
        {
            _isTakingOxygenDamage = true;
            _oxygenDamageTimer += Time.deltaTime;

            if (_oxygenDamageTimer >= 1f)
            {
                _oxygenDamageTimer = 0f;
                TakeOxygenDamage();
            }
        }
    }

    private void CheckForTank()
    {
        if (_playerInventory == null || _oxygenTankData == null) return;

        bool inPrimary = _playerInventory.PrimaryInventorySystem.ContainsItem(_oxygenTankData, out _);
        bool inSecondary = _playerInventory.SecondaryInventorySystem.ContainsItem(_oxygenTankData, out _);

        bool hasTank = inPrimary || inSecondary;

        if (hasTank != _hasOxygenTank)
        {
            _hasOxygenTank = hasTank;
            OnTankEquippedChanged?.Invoke(_hasOxygenTank);
        }
    }

    private void TakeOxygenDamage()
    {
        if (_firstPersonController != null)
        {
            IDamagable damagable = _firstPersonController.GetComponent<IDamagable>();
            if (damagable != null)
            {
                int damage = Mathf.RoundToInt(_damagePerSecond);
                damagable.DamageRecived(damage);
            }
        }
    }

    public void SetPaused(bool paused)
    {
        _isPaused = paused;
    }

    public void ConsumeOxygen(float amount)
    {
        if (_hasOxygenTank && !_isPaused)
        {
            _currentOxygenTime = Mathf.Max(0, _currentOxygenTime - amount);
            OnOxygenChanged?.Invoke(_currentOxygenTime, _maxOxygenTime);
        }
    }

    public void RefillOxygen()
    {
        _currentOxygenTime = _maxOxygenTime;
        _hasOxygenTank = true;
    }

    public void AddOxygen(float amount)
    {
        _currentOxygenTime += amount;

        if (_currentOxygenTime > _maxOxygenTime)
        {
            _currentOxygenTime = _maxOxygenTime;
        }

        _hasOxygenTank = true;
        OnOxygenChanged?.Invoke(_currentOxygenTime, _maxOxygenTime);
    }

    public void RemoveOxygenTank() { }
    public void GiveOxygenTank() { }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    public static void InvokeOxygenRecharging(bool isRecharging)
    {
        OnOxygenRecharging?.Invoke(isRecharging);
    }
}