// Copyright (c) [2024] [Federico Grenoville]

using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// A QuadTree is a spatial partitioning structure used to efficiently query objects
/// located in a 2D space. This implementation supports dynamic updates and visualization.
/// </summary>
public class QuadTree : MonoBehaviour
{
    [Header("QuadTree Settings")]
    
    [Tooltip("Maximum number of elements per node before subdivision.")]
    [SerializeField] private int _nodeCapacity;

    [Tooltip("Boundary of the entire QuadTree.")]
    [SerializeField] private Rect _bounds;
    
    [Tooltip("Minimum allowed size of a QuadTree node. Prevents infinite subdivision.")]
    [SerializeField] private float _nodeMinSize;
 
    /// <summary>
    /// Maximum elements per node before it splits.
    /// </summary>
    public int NodeCapacity => _nodeCapacity;
    
    /// <summary>
    /// Bounds of the top-level QuadTree node.
    /// </summary>
    public Rect Bounds => _bounds;
    
    /// <summary>
    /// Smallest possible node dimension to avoid over-subdivision.
    /// </summary>
    public float NodeMinSize => _nodeMinSize;

    private NodeTree _root;
    private QuadTreeRenderer _quadTreeRendererComponent;

    private bool _needRedraw;
    private Rect _previousFrameBounds;

    /// <summary>
    /// Initializes the QuadTree and caches required components.
    /// </summary>
    void Start()
    {
        _quadTreeRendererComponent = GetComponent<QuadTreeRenderer>();
        _needRedraw = true;
        _previousFrameBounds = Bounds;
    }

    /// <summary>
    /// Checks if the bounds have changed between frames and flags a redraw if needed.
    /// </summary>
    void Update()
    {
        if (_previousFrameBounds.position != Bounds.position ||
            _previousFrameBounds.size != Bounds.size)
        {
            _previousFrameBounds = Bounds;
            _needRedraw = true;
        }
    }

    /// <summary>
    /// Builds the QuadTree structure from a list of 2D positions (e.g., boid positions).
    /// </summary>
    /// <param name="boidPositions">Native array of positions to insert into the tree.</param>
    public void Build(NativeArray<float2> boidPositions)
    {
        int boidCount = boidPositions.Length;
        
        _root = new NodeTree(_bounds, _nodeCapacity, _nodeMinSize, 0);
        for (int i = 0; i < boidCount; i++)
        {
            //_root.Insert(i, boidPositions);
            _root.Insert(i, boidPositions[i]);
        }

        if (_quadTreeRendererComponent != null && _needRedraw)
        {
            _quadTreeRendererComponent.DrawRectsBoundary(Bounds);
            _needRedraw = false;
        }
    }
    
    /// <summary>
    /// Queries the QuadTree for points within a specified range.
    /// </summary>
    /// <param name="range">Rectangular area to search within.</param>
    /// <param name="boidPositions">Source array of positions.</param>
    /// <param name="found">List of indices found inside the range.</param>
    public void QueryRange(Rect range, NativeArray<float2> boidPositions, NativeList<int> found)
    {
        _root.QueryRange(range, boidPositions, found);
    }
}
