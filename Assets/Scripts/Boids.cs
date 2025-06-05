// Copyright (c) [2024] [Federico Grenoville]

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