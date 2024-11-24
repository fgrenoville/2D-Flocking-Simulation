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
using UnityEngine;

public class QuadTreeRenderer : MonoBehaviour
{
    [SerializeField] private GameObject _quadTreeRectRendererPrefab;
        
    private List<GameObject> _quadTreeRectRenderers;
 
    void Start()
    {
        _quadTreeRectRenderers = new List<GameObject>();
    }
    
    public void DrawRectsBoundary(Rect bounds)
    {
        ClearRenderer();
        
        DrawQuadBound(bounds);
    }
    
    private void ClearRenderer()
    {
        foreach (GameObject go in _quadTreeRectRenderers)
            Destroy(go);

        _quadTreeRectRenderers.Clear();
    }

    private void DrawQuadBound(Rect rect)
    {
        Vector2 coord1 = new Vector2(rect.x, rect.yMax);
        Vector2 coord2 = new Vector2(rect.xMax, rect.yMax);
        Vector2 coord3 = new Vector2(rect.x, rect.y);
        Vector2 coord4 = new Vector2(rect.xMax, rect.y);

        GameObject lineRendGO = CreateRendererObject("Bounds", gameObject.transform);

        if (lineRendGO != null)
        {
            LineRenderer lineRendComponent = lineRendGO.GetComponent<LineRenderer>();

            if (lineRendComponent != null)
            {
                lineRendComponent.positionCount = 5;
                lineRendComponent.SetPosition(0, coord1);
                lineRendComponent.SetPosition(1, coord2);
                lineRendComponent.SetPosition(2, coord4);
                lineRendComponent.SetPosition(3, coord3);
                lineRendComponent.SetPosition(4, coord1);
            }
        }
    }

    private GameObject CreateRendererObject(string goName, Transform parent)
    {
        GameObject lineRendGO = Instantiate<GameObject>(_quadTreeRectRendererPrefab, transform.position, Quaternion.identity);
        lineRendGO.transform.SetParent(parent);
        lineRendGO.name = goName;
        _quadTreeRectRenderers.Add(lineRendGO);
        return lineRendGO;
    }
}
