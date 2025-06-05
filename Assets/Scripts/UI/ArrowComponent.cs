// Copyright (c) [2024] [Federico Grenoville]

using UnityEngine;
using UnityEngine.EventSystems;

public class ArrowComponent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private MenuController _menu;

    void Start()
    {
        _menu = GetComponentInParent<MenuController>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_menu)
        {
            _menu.ToggleMenu();    
        }
    }
}
