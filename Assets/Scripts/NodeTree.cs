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

using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Each Quadtree internal node has exactly four children used to partition a
/// two-dimensional space by recursively subdividing it into four quadrants
/// or regions.
/// Each cell (or region) has a maximum <c>capacity</c>. When maximum capacity is reached,
/// the cell splits.
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
    
    public NodeTree(Rect bounds, int nodeCapacity, float nodeMinSize, int depth)
    {
        _bounds = bounds;
        _nodeCapacity = nodeCapacity;
        _nodeMinSize = nodeMinSize;
        _nodeDepth = depth;
        _isDivided = false;
        _points = new List<int>();
    }
    
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
     *  Uncomment this method to use a Point Quadtree (also known as Loose Quadtree).
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
    
    
    /*
     *  Bucket Quadtree (also known as Cell Quadtree)
     */
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
    
    private bool CanDivide()
    {
        return (_bounds.width >= _nodeMinSize && _bounds.height >= _nodeMinSize);
    }

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
