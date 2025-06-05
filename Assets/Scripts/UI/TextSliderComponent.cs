// Copyright (c) [2024] [Federico Grenoville]

using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TextSliderComponent : MonoBehaviour
{
    [SerializeField] private Text _txtFieldWidget;
    [SerializeField] private Text _txtValueWidget;
    [SerializeField] private Slider _sliderWidget;

    [SerializeField] private string _textFieldDisplay;
    [SerializeField] private float _sliderMinValue;
    [SerializeField] private float _sliderMaxValue;
    [SerializeField] private float _sliderStepSize;
    [SerializeField] private bool _sliderHasWholeNumbers;
    [SerializeField] private bool _isInteractable;
    
    public UnityEvent<float> onSliderValueChanged;
   
    void Start()
    {
        Text[] txts = GetComponentsInChildren<Text>();
        for (int i = 0; i < txts.Length; i++)
        {
            if (txts[i] != null)
            {
                if (i == 0)
                {
                    _txtFieldWidget = txts[i];
                }
                else if (i == 1)
                {
                    _txtValueWidget = txts[i];
                }
            }
            
            _sliderWidget = GetComponentInChildren<Slider>();
        }

        if (_txtFieldWidget != null)
        {
            _txtFieldWidget.text = _textFieldDisplay;
        }

        if (_sliderWidget != null)
        {
            _sliderWidget.minValue = _sliderMinValue;
            _sliderWidget.maxValue = _sliderMaxValue;
            _sliderWidget.wholeNumbers = _sliderHasWholeNumbers;
            _sliderWidget.onValueChanged.AddListener(HandleSliderValueChanged);
            _sliderWidget.interactable = _isInteractable;
        }

        SyncTxtValue();
    }

    private void SyncTxtValue()
    {
        if (_txtValueWidget != null)
        {
            _txtValueWidget.text = _sliderWidget.value.ToString(CultureInfo.CurrentCulture);
        }
    }
    
    public void HandleInitialization(float value)
    {
        _sliderWidget.value = Mathf.Clamp(value, _sliderMinValue, _sliderMaxValue);
        SyncTxtValue();
    }

    private void HandleSliderValueChanged(float value)
    {

        float roundedVal = Mathf.Round(value / _sliderStepSize) * _sliderStepSize;
        _sliderWidget.value = roundedVal;
        
        SyncTxtValue();
        onSliderValueChanged?.Invoke(roundedVal);
    }

    void OnDisable()
    {
        _sliderWidget.onValueChanged.RemoveAllListeners();
    }
}
