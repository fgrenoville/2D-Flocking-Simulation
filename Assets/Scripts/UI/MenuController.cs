// Copyright (c) [2024] [Federico Grenoville]

using UnityEngine;

public class MenuController : MonoBehaviour
{
    public bool IsOpen { get; private set; }
    
    private Animator _menu;
    private static readonly int Open = Animator.StringToHash("Open");

    void Start()
    {
        _menu = GetComponent<Animator>();
    }

    public void ToggleMenu()
    {
        if (_menu != null)
        {
            IsOpen = !IsOpen;
            _menu.SetBool(Open, IsOpen);
        }
    }
}
