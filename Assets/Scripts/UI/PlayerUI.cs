using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private Slider _healthSlider;

    [Header("Hambre")]
    [SerializeField] private Slider _hungerSlider;

    [Header("Oxigeno")]
    [SerializeField] private Image _oxygenBarImage;
    [SerializeField] private Image _tankIcon;
    [Header("Colores oxigeno")]
    [SerializeField] private Color _oxygenActiveColor;
    [SerializeField] private Color _oxygenInactiveColor;
    [SerializeField] private Color _oxygenRechargingColor;

    private bool _isRecharging = false;

    private void OnEnable()
    {
        FirstPersonController.OnHealthChanged += UpdateHealthUI;
        HungerSystem.OnHungerChanged += UpdateHungerUI;
        OxygenSystem.OnOxygenChanged += UpdateOxygenUI;
        OxygenSystem.OnTankEquippedChanged += UpdateTankUI;
        OxygenSystem.OnOxygenRecharging += UpdateRechargeUI;
    }

    private void OnDisable()
    {
        FirstPersonController.OnHealthChanged -= UpdateHealthUI;
        HungerSystem.OnHungerChanged -= UpdateHungerUI;
        OxygenSystem.OnOxygenChanged -= UpdateOxygenUI;
        OxygenSystem.OnTankEquippedChanged -= UpdateTankUI;
        OxygenSystem.OnOxygenRecharging -= UpdateRechargeUI;
    }

    private void UpdateHealthUI(float current, float max)
    {
        if (_healthSlider == null) return;
        _healthSlider.maxValue = max;
        _healthSlider.value = current;
    }

    private void UpdateHungerUI(float current, float max)
    {
        if (_hungerSlider == null) return;
        _hungerSlider.maxValue = max;
        _hungerSlider.value = current;
    }
    private void UpdateOxygenUI(float current, float max)
    {
        if (_oxygenBarImage == null) return;
        _oxygenBarImage.fillAmount = current / max;
    }

    private void UpdateTankUI(bool hasTank)
    {
        if (_tankIcon != null)
            _tankIcon.gameObject.SetActive(hasTank);

        if (_oxygenBarImage != null && !_isRecharging)
            _oxygenBarImage.color = hasTank ? _oxygenActiveColor : _oxygenInactiveColor;
    }

    private void UpdateRechargeUI(bool isRecharging)
    {
        _isRecharging = isRecharging;

        if (_tankIcon != null)
            _tankIcon.gameObject.SetActive(isRecharging || OxygenSystem.Instance.HasOxygenTank);

        if (_oxygenBarImage != null)
            _oxygenBarImage.color = isRecharging ? _oxygenRechargingColor : _oxygenActiveColor;
    }
}
