/*
 *  MIT License
 *
 *  Copyright (c) [2024] [Federico Grenoville]
 *   
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in all
 *  copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 *  SOFTWARE.
 */

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
