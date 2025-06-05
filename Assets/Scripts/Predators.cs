// Copyright (c) [2024] [Federico Grenoville]

using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

public struct Predators
{
    public int SpawnAtStart;
    public int Count;
    
    public NativeArray<float2> Positions;
    public NativeArray<float2> Velocities;
    public NativeArray<float> WanderTheta;
    public TransformAccessArray TransformsAccessArray;
    public NativeArray<float2> Steering;
    
    public PredatorsBehavior Behavior;
    
    public List<GameObject> Objects;
    public List<SpriteRenderer> Renderers;
    public List<MaterialPropertyBlock> PropertyBlocks;
}