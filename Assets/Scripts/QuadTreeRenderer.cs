// Copyright (c) [2024] [Federico Grenoville]

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsible for rendering the boundaries of a Quadtree structure
/// using LineRenderers. Designed to assist with debugging or visualization.
/// </summary>
public class QuadTreeRenderer : MonoBehaviour
{
    [Header("Rendering Settings")]
    
    [Tooltip("Prefab with a LineRenderer used to draw the boundary rectangles.")]
    [SerializeField] private GameObject _quadTreeRectRendererPrefab;
        
    private List<GameObject> _quadTreeRectRenderers;
 
    /// <summary>
    /// Initializes the internal list of renderable rectangle objects.
    /// </summary>
    void Start()
    {
        _quadTreeRectRenderers = new List<GameObject>();
    }
    
    /// <summary>
    /// Clears previous rectangle renderers and draws the updated bounds.
    /// </summary>
    /// <param name="bounds">Rectangle representing the current quadtree bounds.</param>
    public void DrawRectsBoundary(Rect bounds)
    {
        ClearRenderer();
        
        DrawQuadBound(bounds);
    }
    
    /// <summary>
    /// Removes all previously instantiated rectangle LineRenderers from the scene.
    /// </summary>
    private void ClearRenderer()
    {
        foreach (GameObject go in _quadTreeRectRenderers)
            Destroy(go);

        _quadTreeRectRenderers.Clear();
    }

    /// <summary>
    /// Creates a rectangle using a LineRenderer, based on the given rect coordinates.
    /// </summary>
    /// <param name="rect">The rectangle area to visualize.</param>
    private void DrawQuadBound(Rect rect)
    {
        // Define the corners in clockwise order
        Vector2 coord1 = new Vector2(rect.x, rect.yMax);        // Top-left
        Vector2 coord2 = new Vector2(rect.xMax, rect.yMax);     // Top-right
        Vector2 coord3 = new Vector2(rect.x, rect.y);           // Bottom-left
        Vector2 coord4 = new Vector2(rect.xMax, rect.y);        // Bottom-right

        GameObject lineRendGO = CreateRendererObject("Bounds", gameObject.transform);

        if (lineRendGO != null)
        {
            LineRenderer lineRendComponent = lineRendGO.GetComponent<LineRenderer>();

            if (lineRendComponent != null)
            {
                // Draw the rectangle with the LineRenderer
                lineRendComponent.positionCount = 5;
                lineRendComponent.SetPosition(0, coord1);
                lineRendComponent.SetPosition(1, coord2);
                lineRendComponent.SetPosition(2, coord4);
                lineRendComponent.SetPosition(3, coord3);
                lineRendComponent.SetPosition(4, coord1);   // Close the loop
            }
        }
    }

    /// <summary>
    /// Instantiates a new GameObject with a LineRenderer to represent one rectangle.
    /// </summary>
    /// <param name="goName">Name assigned to the GameObject.</param>
    /// <param name="parent">Transform to parent the created object to.</param>
    /// <returns>The newly created GameObject with a LineRenderer.</returns>
    private GameObject CreateRendererObject(string goName, Transform parent)
    {
        GameObject lineRendGO = Instantiate<GameObject>(_quadTreeRectRendererPrefab, transform.position, Quaternion.identity);
        lineRendGO.transform.SetParent(parent);
        lineRendGO.name = goName;
        _quadTreeRectRenderers.Add(lineRendGO);
        return lineRendGO;
    }
}
