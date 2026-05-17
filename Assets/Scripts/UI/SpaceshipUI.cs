using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpaceshipUI : MonoBehaviour
{
    [Header("Vida Nave")]
    [SerializeField] private Slider _healthSlider;

    [Header("Energia Escudo")]
    [SerializeField] private Slider _shieldSlider;

    [Header("Mejora de Base")]
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private Image _material1Icon;
    [SerializeField] private Image _material2Icon;

    private void OnEnable()
    {
        SpaceshipHealth.OnSpaceshipHealthChanged += UpdateHealthUI;
        GasoilStation.OnShieldEnergyChanged += UpdateShieldUI;
        BaseLevelUpStation.OnBaseLevelChanged += UpdateBaseLevelUI;
    }

    private void OnDisable()
    {
        SpaceshipHealth.OnSpaceshipHealthChanged -= UpdateHealthUI;
        GasoilStation.OnShieldEnergyChanged -= UpdateShieldUI;
        BaseLevelUpStation.OnBaseLevelChanged -= UpdateBaseLevelUI;
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

    private void UpdateBaseLevelUI(int displayLevel, Sprite icon1, Sprite icon2, bool isMaxLevel)
    {
        if (_levelText != null)
        {
            _levelText.text = "Actual base level: " + displayLevel.ToString();
        }

        if (isMaxLevel)
        {
            if (_material1Icon != null) _material1Icon.gameObject.SetActive(false);
            if (_material2Icon != null) _material2Icon.gameObject.SetActive(false);
        }
        else
        {
            if (_material1Icon != null)
            {
                _material1Icon.gameObject.SetActive(true);
                _material1Icon.sprite = icon1;
            }

            if (_material2Icon != null)
            {
                _material2Icon.gameObject.SetActive(true);
                _material2Icon.sprite = icon2;
            }
        }
    }
}
