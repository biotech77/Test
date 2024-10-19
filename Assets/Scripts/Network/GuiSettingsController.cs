using System.Globalization;
using Configs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GuiSettingsController : MonoBehaviour
{

    private LevelConfig _levelConfig;
    
    public Slider MaxFishCountSlider;
    public TextMeshProUGUI TextCurrentMaxFishCount;
    
    public Slider ProjectileSpreadSlider;
    public TextMeshProUGUI TextCurrentProjectileSpread;
    
    public Slider FireRateSlider;
    public TextMeshProUGUI TextCurrentFireRate;
    
    // Size
    public TextMeshProUGUI TextCurrentMaxFishSize;
    public TextMeshProUGUI TextCurrentMinFishSize;
    public Button ButtonIncreaseMinFishSize;
    public Button ButtonDecreaseMinFishSize;
    public Button ButtonIncreaseMaxFishSize;
    public Button ButtonDecreaseMaxFishSize;
    
    // Speed
    public TextMeshProUGUI TextCurrentMaxFishSpeed;
    public TextMeshProUGUI TextCurrentMinFishSpeed;
    public Button ButtonIncreaseMinFishSpeed;
    public Button ButtonDecreaseMinFishSpeed;
    public Button ButtonIncreaseMaxFishSpeed;
    public Button ButtonDecreaseMaxFishSpeed;
    
    private void Awake()
    {
        _levelConfig = Resources.Load<LevelConfig>("LevelConfig");
        if (_levelConfig == null) Debug.LogError("LevelConfig not found");
        
        
    }

    void Start()
    {
        TextCurrentMaxFishCount.text = _levelConfig.MaxFishInPond.ToString();
        MaxFishCountSlider.value = _levelConfig.MaxFishInPond;
        
        TextCurrentProjectileSpread.text = _levelConfig.MaxSpreadAngle.ToString();
        ProjectileSpreadSlider.value = _levelConfig.MaxSpreadAngle;

        TextCurrentMaxFishSize.text = _levelConfig.MaxFishSize.ToString(CultureInfo.InvariantCulture);
        TextCurrentMinFishSize.text = _levelConfig.MinFishSize.ToString(CultureInfo.InvariantCulture);
        TextCurrentMaxFishSpeed.text = _levelConfig.MaxFishSpeed.ToString(CultureInfo.InvariantCulture);
        TextCurrentMinFishSpeed.text = _levelConfig.MinFishSpeed.ToString(CultureInfo.InvariantCulture);

        FireRateSlider.value = _levelConfig.FireRate;
        TextCurrentFireRate.text = _levelConfig.FireRate.ToString();
    }

    public void Reset()
    {
        _levelConfig.FireRate = 10;
        _levelConfig.MaxFishInPond = 3;
        _levelConfig.MinFishSpeed = 3;
        _levelConfig.MaxFishSpeed = 10;
        _levelConfig.MaxFishSize = 0.30f;
        _levelConfig.MinFishSize = 0.15f;
        _levelConfig.MaxSpreadAngle = 10;

        Refresh();
    }

    private void Refresh()
    {
        FireRateSlider.SetValueWithoutNotify(_levelConfig.FireRate); 
        TextCurrentFireRate.text = _levelConfig.FireRate.ToString();
        
        ProjectileSpreadSlider.SetValueWithoutNotify(_levelConfig.MaxSpreadAngle);
        TextCurrentProjectileSpread.text = _levelConfig.MaxSpreadAngle.ToString(CultureInfo.InvariantCulture);
        
        MaxFishCountSlider.SetValueWithoutNotify(_levelConfig.MaxFishInPond);
        TextCurrentMaxFishCount.text = _levelConfig.MaxFishInPond.ToString();
        
        TextCurrentMaxFishSize.text = _levelConfig.MaxFishSize.ToString("F1");
        TextCurrentMinFishSize.text = _levelConfig.MinFishSize.ToString("F1");
        TextCurrentMaxFishSpeed.text = _levelConfig.MaxFishSpeed.ToString("F1");
        TextCurrentMinFishSpeed.text = _levelConfig.MinFishSpeed.ToString("F1");
        TextCurrentMaxFishCount.text = _levelConfig.MaxFishInPond.ToString();
        
    }

    public void OnFireRateSliderValueChanged()
    {
        _levelConfig.FireRate = (int)FireRateSlider.value;
        TextCurrentFireRate.text = _levelConfig.FireRate.ToString();
    }
    
    public void OnMaxFishCountSliderValueChanged()
    {
        _levelConfig.MaxFishInPond = (int)MaxFishCountSlider.value;
        TextCurrentMaxFishCount.text = _levelConfig.MaxFishInPond.ToString();
    }
    
    public void OnProjectileSpreadSliderValueChanged()
    {
        _levelConfig.MaxSpreadAngle = (int)ProjectileSpreadSlider.value;
        TextCurrentProjectileSpread.text = _levelConfig.MaxSpreadAngle.ToString(CultureInfo.InvariantCulture);
    }

    public void IncreaseMinFishSize()
    {
        if (_levelConfig.MinFishSize + 0.01f > _levelConfig.MaxFishSize) return;
        
        _levelConfig.MinFishSize += 0.01f;
        TextCurrentMinFishSize.text = _levelConfig.MinFishSize.ToString("F2");
    }
    
    public void IncreaseMaxFishSize()
    {
        _levelConfig.MaxFishSize += 0.01f;
        TextCurrentMaxFishSize.text = _levelConfig.MaxFishSize.ToString("F2");
    }
    
    public void DecreaseMinFishSize()
    {
        if (_levelConfig.MinFishSize - 0.01f <= 0f) return;
        
        _levelConfig.MinFishSize -= 0.01f;
        TextCurrentMinFishSize.text = _levelConfig.MinFishSize.ToString("F2");
    }
    
    public void DecreaseMaxFishSize()
    {
        if (_levelConfig.MaxFishSize - 0.01f <= _levelConfig.MinFishSize) return;
        _levelConfig.MaxFishSize -= 0.01f;
        TextCurrentMaxFishSize.text = _levelConfig.MaxFishSize.ToString("F2");
    }
    
    public void IncreaseMinFishSpeed()
    {
        if (_levelConfig.MinFishSpeed + 0.1f > _levelConfig.MaxFishSpeed) return;
        
        _levelConfig.MinFishSpeed += 0.1f;
        TextCurrentMinFishSpeed.text = _levelConfig.MinFishSpeed.ToString("F1");
    }
    
    public void IncreaseMaxFishSpeed()
    {
        _levelConfig.MaxFishSpeed += 0.1f;
        TextCurrentMaxFishSpeed.text = _levelConfig.MaxFishSpeed.ToString("F1");
    }
    
    public void DecreaseMinFishSpeed()
    {
        if (_levelConfig.MinFishSpeed - 0.1f <= 0f) return;
        
        _levelConfig.MinFishSpeed -= 0.1f;
        TextCurrentMinFishSpeed.text = _levelConfig.MinFishSpeed.ToString("F1");
    }
    
    public void DecreaseMaxFishSpeed()
    {
        if (_levelConfig.MaxFishSpeed - 0.1f <= _levelConfig.MinFishSpeed) return;
        _levelConfig.MaxFishSpeed -= 0.1f;
        TextCurrentMaxFishSpeed.text = _levelConfig.MaxFishSpeed.ToString("F1");
    }
}
