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
using UnityEngine.Jobs;

/// <summary>
/// Boids struct keeps track of every single entity.
/// </summary>
public struct Boids
{
    public int SpawnAtStart;
    public int Count;
    public float MaxPerceptionRadius;
    
    public NativeArray<float2> Positions;
    public NativeArray<float2> Velocities;
    public TransformAccessArray TransformsAccessArray;
    public NativeArray<float2> Steering;
    public NativeArray<float3> TextureColor;
    public NativeArray<float> Intensity;
    
    public BoidsBehavior Behavior;
    public BoidsAlignment Alignment;
    public BoidsSeparation Separation;
    public BoidsCohesion Cohesion;
    public BoidsEscape Escape;
    public BoidsTargetAttraction TargetAttraction;
    public BoidsTargetRepulsion TargetRepulsion;
    public List<GameObject> Objects;
    public List<SpriteRenderer> Renderers;
    public List<MaterialPropertyBlock> PropertyBlocks;
}