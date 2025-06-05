// Copyright (c) [2024] [Federico Grenoville]

using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// A node in a Quadtree data structure that recursively partitions a 2D space.
/// Each node may contain a list of point indices until a maximum capacity is reached.
/// Once that limit is exceeded and the region is large enough, it subdivides into four child nodes.
/// </summary>
public class NodeTree
{
    public Rect Bounds => _bounds;
    public NodeTree[] Children => _children;
    public bool IsDivided => _isDivided;
    
    private Rect _bounds;
    private readonly List<int> _points;
    private NodeTree[] _children;
    private readonly int _nodeCapacity;
    private readonly float _nodeMinSize;
    private bool _isDivided;
    private readonly int _nodeDepth;
    
    /// <summary>
    /// Initializes a new node with spatial boundaries and configuration parameters.
    /// </summary>
    /// <param name="bounds">The region this node covers.</param>
    /// <param name="nodeCapacity">Maximum number of points this node can hold before subdividing.</param>
    /// <param name="nodeMinSize">Minimum allowed size for subdivision.</param>
    /// <param name="depth">Current recursion depth of the node.</param>
    public NodeTree(Rect bounds, int nodeCapacity, float nodeMinSize, int depth)
    {
        _bounds = bounds;
        _nodeCapacity = nodeCapacity;
        _nodeMinSize = nodeMinSize;
        _nodeDepth = depth;
        _isDivided = false;
        _points = new List<int>();
    }
    
    /// <summary>
    /// Subdivides this node into four equal-sized child nodes.
    /// </summary>
    public void Subdivide()
    {
        Vector2 halfBound = new Vector2(_bounds.width, _bounds.height) / 2;

        Rect nw = new Rect(new Vector2(_bounds.x, _bounds.y + halfBound.y), halfBound);
        Rect ne = new Rect(new Vector2(_bounds.x + halfBound.x, _bounds.y + halfBound.y), halfBound);
        Rect sw = new Rect(new Vector2(_bounds.x, _bounds.y), halfBound);
        Rect se = new Rect(new Vector2(_bounds.x + halfBound.x, _bounds.y), halfBound);
        
        _children = new NodeTree[4];
        _children[0] = new NodeTree(nw, _nodeCapacity, _nodeMinSize, _nodeDepth + 1);
        _children[1] = new NodeTree(ne, _nodeCapacity, _nodeMinSize, _nodeDepth + 1);
        _children[2] = new NodeTree(sw, _nodeCapacity, _nodeMinSize, _nodeDepth + 1);
        _children[3] = new NodeTree(se, _nodeCapacity, _nodeMinSize, _nodeDepth + 1);

        _isDivided = true;
    }
    
    /*
     *  Bucket Quadtree (also known as Cell Quadtree)
     */
    /// <summary>
    /// Inserts a point into this node (Bucket Quadtree).
    /// </summary>
    /// <param name="index">Index of the point in the external array.</param>
    /// <param name="point">The actual point position (float2).</param>
    /// <returns>True if the point was successfully inserted.</returns>
    public bool Insert(int index, float2 point)
    {
        if (!_bounds.Contains(new Vector2(point.x, point.y)))
            return false;
        
        if (_points.Count < _nodeCapacity || !CanDivide())
        {
            _points.Add(index);
            return true;
        }
        else 
        {
            if (!_isDivided)
                Subdivide();
    
            foreach (var child in _children)
            {
                if (child.Insert(index, point))
                    return true;
            }
        }
        return false;
    }
    
    /*
     * OPTIONAL: Point-based (Loose) Quadtree version of Insert()
     * If you want to support it, uncomment and use this version instead.
     */
    // public void Insert(int index, NativeArray<float2> points)
    // {
    //     if (!_bounds.Contains(new Vector2(points[index].x, points[index].y)))
    //         return;
    //     
    //     if (!_isDivided)
    //     {
    //         if (_points.Count < _nodeCapacity || !CanDivide())
    //         {
    //             _points.Add(index);
    //             return;
    //         }
    //         
    //         Subdivide();
    //         foreach (var p in _points)
    //         {
    //             foreach (var child in _children)
    //             {
    //                 child.Insert(p, points);
    //             }
    //         }
    //         _points.Clear();
    //     }
    //     
    //     foreach (var child in _children)
    //     {
    //         child.Insert(index, points);
    //     }
    // }
    
    /// <summary>
    /// Checks whether this node can be subdivided based on its size.
    /// </summary>
    private bool CanDivide()
    {
        return (_bounds.width >= _nodeMinSize && _bounds.height >= _nodeMinSize);
    }

    /// <summary>
    /// Queries all point indices that lie within the given range.
    /// </summary>
    /// <param name="range">Area to search within.</param>
    /// <param name="boidPositions">Array of all point positions.</param>
    /// <param name="found">List to store the indices of found points.</param>
    public void QueryRange(Rect range, NativeArray<float2> boidPositions, List<int> found)
    {
        if (!_bounds.Overlaps(range, true))
            return;

        foreach (var p in _points)
        {
            if (range.Contains(boidPositions[p]))
            {
                found.Add(p);
            }
        }

        if (_isDivided)
        {
            foreach (var child in _children)
            {
                child.QueryRange(range, boidPositions, found);
            }
        }
    }
    
    /// <summary>
    /// Same as <see cref="QueryRange(Rect, NativeArray{float2}, List{int})"/>, but using a NativeList.
    /// </summary>
    public void QueryRange(Rect range, NativeArray<float2> boidPositions, NativeList<int> found)
    {
        if (!_bounds.Overlaps(range, true))
            return;

        foreach (var p in _points)
        {
            if (range.Contains(boidPositions[p]))
            {
                found.Add(p);
            }
        }

        if (_isDivided)
        {
            foreach (var child in _children)
            {
                child.QueryRange(range, boidPositions, found);
            }
        }
    }
}
