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

using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// A Quadtree is a spatial index structure for efficient range querying of items bounded by 2D rectangles.
/// </summary>
public class QuadTree : MonoBehaviour
{
    [SerializeField] private int _nodeCapacity;
    [SerializeField] private Rect _bounds;
    [SerializeField] private float _nodeMinSize;
 
    public int NodeCapacity => _nodeCapacity;
    public Rect Bounds => _bounds;
    public float NodeMinSize => _nodeMinSize;

    private NodeTree _root;
    private QuadTreeRenderer _quadTreeRendererComponent;

    private bool _needRedraw;
    private Rect _previousFrameBounds;

    void Start()
    {
        _quadTreeRendererComponent = GetComponent<QuadTreeRenderer>();
        _needRedraw = true;
        _previousFrameBounds = Bounds;
    }

    void Update()
    {
        if (_previousFrameBounds.position != Bounds.position ||
            _previousFrameBounds.size != Bounds.size)
        {
            _previousFrameBounds = Bounds;
            _needRedraw = true;
        }
    }

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
    
    public void QueryRange(Rect range, NativeArray<float2> boidPositions, NativeList<int> found)
    {
        _root.QueryRange(range, boidPositions, found);
    }
}
