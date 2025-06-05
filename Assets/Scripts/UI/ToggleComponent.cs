// Copyright (c) [2024] [Federico Grenoville]

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
