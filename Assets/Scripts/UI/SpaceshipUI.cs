using UnityEngine;
using UnityEngine.UI;

public class SpaceshipUI : MonoBehaviour
{
    [Header("Vida Nave")]
    [SerializeField] private Slider _healthSlider;

    [Header("Energia Escudo")]
    [SerializeField] private Slider _shieldSlider;

    private void OnEnable()
    {
        SpaceshipHealth.OnSpaceshipHealthChanged += UpdateHealthUI;
        GasoilStation.OnShieldEnergyChanged += UpdateShieldUI;
    }

    private void OnDisable()
    {
        SpaceshipHealth.OnSpaceshipHealthChanged -= UpdateHealthUI;
        GasoilStation.OnShieldEnergyChanged -= UpdateShieldUI;
    }

    private void UpdateHealthUI(int current, int max)
    {
        if (_healthSlider == null) return;
        _healthSlider.maxValue = max;
        _healthSlider.value = current;
    }

    private void UpdateShieldUI(float current, float max)
    {
        if (_shieldSlider == null) return;
        _shieldSlider.maxValue = max;
        _shieldSlider.value = current;
    }
}
