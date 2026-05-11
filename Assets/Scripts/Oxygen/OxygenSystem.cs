using UnityEngine;

public class OxygenSystem : MonoBehaviour
{
    [Header("Oxygen Settings")]
    [SerializeField] private float _maxOxygenTime = 60f;
    [SerializeField] private float _currentOxygenTime;
    [SerializeField] private bool _isPaused;
    [SerializeField] private bool _hasOxygenTank;

    [Header("Damage Settings")]
    [SerializeField] private float _damagePerSecond = 5f;
    [SerializeField] private bool _isTakingOxygenDamage;
    [SerializeField] private float _oxygenDamageTimer;

    [Header("UI References")]
    [SerializeField] private UnityEngine.UI.Slider _oxygenSlider;
    [SerializeField] private UnityEngine.UI.Image _oxygenFillImage;
    [SerializeField] private UnityEngine.UI.Text _oxygenTimerText;

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

        UpdateOxygenUI();
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
            _isTakingOxygenDamage = false;
            _oxygenDamageTimer = 0f;

            if (_currentOxygenTime <= 0)
            {
                _currentOxygenTime = 0;
            }

            UpdateOxygenUI();
        }
        else
        {
            if (!_hasOxygenTank)
            {
                if (_oxygenSlider != null) _oxygenSlider.gameObject.SetActive(false);
                if (_oxygenTimerText != null) _oxygenTimerText.gameObject.SetActive(false);
            }
            else
            {
                UpdateOxygenUI(); 
            }

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

        _hasOxygenTank = inPrimary || inSecondary;

        if (_hasOxygenTank)
        {
            if (_oxygenSlider != null && !_oxygenSlider.gameObject.activeSelf) _oxygenSlider.gameObject.SetActive(true);
            if (_oxygenTimerText != null && !_oxygenTimerText.gameObject.activeSelf) _oxygenTimerText.gameObject.SetActive(true);
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
            UpdateOxygenUI();
        }
    }

    public void RefillOxygen()
    {
        _currentOxygenTime = _maxOxygenTime;
        _hasOxygenTank = true;
        UpdateOxygenUI();
    }

    public void AddOxygen(float amount)
    {
        _currentOxygenTime += amount;

        if (_currentOxygenTime > _maxOxygenTime)
        {
            _currentOxygenTime = _maxOxygenTime;
        }

        _hasOxygenTank = true;
        UpdateOxygenUI();
    }

    public void RemoveOxygenTank() { }
    public void GiveOxygenTank() { }

    private void UpdateOxygenUI()
    {
        if (_oxygenSlider != null)
        {
            _oxygenSlider.maxValue = _maxOxygenTime;
            _oxygenSlider.value = _currentOxygenTime;
        }

        if (_oxygenFillImage != null && _hasOxygenTank)
        {
            float oxygenPercent = _currentOxygenTime / _maxOxygenTime;
            _oxygenFillImage.color = oxygenPercent > 0.25f ? Color.cyan : Color.red;
        }

        if (_oxygenTimerText != null)
        {
            int minutes = Mathf.FloorToInt(_currentOxygenTime / 60f);
            int seconds = Mathf.FloorToInt(_currentOxygenTime % 60f);
            _oxygenTimerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
            _oxygenTimerText.gameObject.SetActive(_hasOxygenTank);
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}