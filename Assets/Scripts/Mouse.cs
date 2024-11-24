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
using UnityEngine.EventSystems;

public class Mouse : MonoBehaviour
{
    [SerializeField] private float _leftClickThresholdTime;
    [SerializeField] private float _leftDoubleClickThresholdTime;
    
    [SerializeField] private float _zoomSpeed;
    [SerializeField] private float _minZoom;
    [SerializeField] private float _maxZoom;
    
    public UnityEvent<Vector2> onMouseLeftClick;
    public UnityEvent<Vector2> onMouseLeftDoubleClick;

    public UnityEvent<Vector2> onMouseMiddleClick;
    public UnityEvent<Vector2> onMouseRightClick;
    
    public UnityEvent<Vector2> onMouseRightBeginInteraction;
    public UnityEvent<Vector2> onMouseRightInteract;
    public UnityEvent<Vector2> onMouseRightEndInteraction;
    
    private float _leftButtonTimestamp;
    private bool _checkForDoubleClick;
    private Vector2 mousePos;
    private bool _isDragging;

    private const int LEFTBUTTON = 0;
    private const int MIDDLEBUTTON = 2;
    private const int RIGHTBUTTON = 1;
    
    private Camera _camera;

    void Start()
    {
        _camera = Camera.main;
    }

    void HandleZoom()
    {
        float scrollData = Input.GetAxis("Mouse ScrollWheel");

        if (scrollData != 0.0f)
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                float newSize = _camera.orthographicSize - scrollData * _zoomSpeed;
                newSize = Mathf.Clamp(newSize, _minZoom, _maxZoom);
                _camera.orthographicSize = newSize;
            }
        }
    }
    
    void Update()
    {
        HandleZoom();
        HandleLeftButton();
        HandleMiddleButton();
        HandleRightButton();
    }

    private void HandleLeftButton()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButtonDown(LEFTBUTTON))
            {
                _isDragging = true;
                mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
            }
            else
            {
                if (_checkForDoubleClick)
                {
                    if ((Time.time - _leftButtonTimestamp) > _leftDoubleClickThresholdTime)
                    {
                        _checkForDoubleClick = false;

                        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(LEFTBUTTON))
                        {
                            _isDragging = true;
                        }
                        else
                        {
                            onMouseLeftClick?.Invoke(mousePos);
                        }
                    }
                }
            
                if (Input.GetMouseButtonDown(LEFTBUTTON))
                {
                    if (_checkForDoubleClick)
                    {
                        _checkForDoubleClick = false;
                        onMouseLeftDoubleClick?.Invoke(mousePos);
                    }
                    else
                    {
                        if (!_isDragging)
                        {
                            _leftButtonTimestamp = Time.time;
                            _checkForDoubleClick = true;
                            mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
                        }
                    }
                }
            }
            
            if (_isDragging)
            {
                if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(LEFTBUTTON))
                {
                    Vector2 currentMousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
                    Vector3 difference = mousePos - currentMousePos;

                    if (difference.sqrMagnitude > Mathf.Epsilon)
                    {
                        _camera.transform.position += difference;
                        mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
                    }
                }
                else
                {
                    _isDragging = false;
                }
            }
        }
    }
    
    private void HandleMiddleButton()
    {
        if (Input.GetMouseButtonUp(MIDDLEBUTTON))
        {
            Vector2 mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
            onMouseMiddleClick?.Invoke(mousePos);
        }
    }

    private void HandleRightButton()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(RIGHTBUTTON))
            {
                Vector2 mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
                onMouseRightBeginInteraction?.Invoke(mousePos);
            }

            if (Input.GetMouseButton(RIGHTBUTTON))
            {
                Vector2 mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
                onMouseRightInteract?.Invoke(mousePos);
            }

            if (Input.GetMouseButtonUp(RIGHTBUTTON))
            {
                Vector2 mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
                onMouseRightEndInteraction?.Invoke(mousePos);
            }
        }
    }
}
