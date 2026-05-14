using UnityEngine;

public class HungerSystem : MonoBehaviour
{
    [Header("Ajustes de hambre")]
    [SerializeField] private float _maxHunger = 100;
    private float _currentHunger;

    [Header("Ajustes tiempo sin hambre")]
    [SerializeField] private float _graceTime = 20;
    private float _graceTimer;

    [Header("Ajustes de inanicion")]
    [SerializeField] private float _hungerStarvationTime = .5f;

    [Header("Ajustes de daño")]
    [SerializeField] private int _damagePerSecond = 1;
    private float _damageTimer;

    [Header("Referencias")]
    private FirstPersonController _pController;


    public float CurrentHunger => _currentHunger;
    public float MaxHunger => _maxHunger;
    public bool IsStarving => _currentHunger <= 0;

    private void Start()
    {
        _currentHunger = _maxHunger;
        _graceTimer = _graceTime;

        _pController = GetComponent<FirstPersonController>();
    }

    private void Update()
    {
        if (Time.timeScale <= 0) return;

        if (_currentHunger >= _maxHunger)
        {
            if (_graceTimer > 0)
            {
                _graceTimer -= Time.deltaTime;
                return;
            }
        }

        if (_currentHunger > 0) 
        {
            _currentHunger -= _hungerStarvationTime * Time.deltaTime;
            _currentHunger = Mathf.Max(_currentHunger, 0);
        }

        if (_currentHunger <= 0)
        {
            _damageTimer += Time.deltaTime;

            if (_damageTimer >= 1)
            {
                _damageTimer = 0;
                TakeHungerDamage();
            }
        }
        else
            _damageTimer = 0;
    }

    private void TakeHungerDamage()
    {
        if (_pController == null) return;

        IDamagable damagable = _pController.GetComponent<IDamagable>();
        if (damagable != null)
        {
            damagable.DamageRecived(_damagePerSecond);
        }
    }

    public void AddHunger(float amount)
    {
        _currentHunger = Mathf.Min(_currentHunger + amount, _maxHunger);

        if (_currentHunger >= _maxHunger)
            _graceTimer = _graceTime;
    }
}
