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

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ToggleComponent : MonoBehaviour
{
    [SerializeField] private Text _txtFieldWidget;
    [SerializeField] private Toggle _toggleWidget;

    [SerializeField] private string _textFieldDisplay;

    public UnityEvent<bool> onToggleChanged;

    void Start()
    {
        _toggleWidget = GetComponentInChildren<Toggle>();

        if (_toggleWidget != null)
        {
            _txtFieldWidget = _toggleWidget.GetComponentInChildren<Text>();
            _toggleWidget.onValueChanged.AddListener(HandleToggleChanged);
        }
        
        if (_txtFieldWidget != null)
        {
            _txtFieldWidget.text = _textFieldDisplay;
        }
    }

    public void HandleToggleChanged(bool value)
    {
        _toggleWidget.isOn = value;
        
        onToggleChanged?.Invoke(value);
    }

    void OnDisable()
    {
        _toggleWidget.onValueChanged.RemoveAllListeners();
    }
}
