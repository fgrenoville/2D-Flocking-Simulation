// Copyright (c) [2024] [Federico Grenoville]


using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// Mouse input handler for Unity.
/// Supports zoom, panning, and customizable mouse click events (left, middle, right).
/// Designed for top-down/2D camera control and editor-style interactions.
/// </summary>
public class Mouse : MonoBehaviour
{
    [Header("Click Timings")]
    [Tooltip("Time threshold to detect a single left click.")]
    [SerializeField] private float _leftClickThresholdTime;
    [Tooltip("Maximum time interval between clicks to register a double-click.")]
    [SerializeField] private float _leftDoubleClickThresholdTime;
    
    [Header("Zoom Settings")]
    [Tooltip("Zoom speed applied with mouse scroll wheel.")]
    [SerializeField] private float _zoomSpeed;
    [Tooltip("Minimum allowed orthographic camera zoom.")]
    [SerializeField] private float _minZoom;
    [Tooltip("Maximum allowed orthographic camera zoom.")]
    [SerializeField] private float _maxZoom;
    
    [Header("Mouse Events")]
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
    
    void Update()
    {
        HandleZoom();
        HandleLeftButton();
        HandleMiddleButton();
        HandleRightButton();
    }

    /// <summary>
    /// Zooms the camera using the scroll wheel input.
    /// </summary>
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
    
    /// <summary>
    /// Handles left mouse button input including dragging, click and double-click detection.
    /// </summary>
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
    
    /// <summary>
    /// Handles interaction with the middle mouse button.
    /// </summary>
    private void HandleMiddleButton()
    {
        if (Input.GetMouseButtonUp(MIDDLEBUTTON))
        {
            Vector2 mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
            onMouseMiddleClick?.Invoke(mousePos);
        }
    }

    /// <summary>
    /// Handles interaction with the right mouse button.
    /// </summary>
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
