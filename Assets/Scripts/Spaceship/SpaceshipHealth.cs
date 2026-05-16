using System;
using UnityEngine;

public class SpaceshipHealth : MonoBehaviour, IDamagable, ITargetable
{
    public static event Action<int, int> OnSpaceshipHealthChanged;

    [Header("Variables de la vida")]
    [SerializeField] private int _maxHealth;
    [SerializeField] GameObject _spaceship;
    private int _currentHealth;

    [SerializeField] private TargetType _targetType = TargetType.Indestructible;
    public TargetType TargetType => _targetType;

    void Start()
    {
        _currentHealth = _maxHealth;
        OnSpaceshipHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    public void DamageRecived(int damage)
    {
        _currentHealth -= damage;
        OnSpaceshipHealthChanged?.Invoke(_currentHealth, _maxHealth);

        if (_currentHealth <= 0)
        {
            GameStateManager.Instance.ChangeGameState(GameState.StateType.OVER);
            Destroy(_spaceship);
        }
    }
}
