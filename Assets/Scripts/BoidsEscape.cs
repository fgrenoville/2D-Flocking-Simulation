// Copyright (c) [2024] [Federico Grenoville]

using Unity.Collections;
using Unity.Mathematics;

public struct BoidsEscape
{
    public float PerceptionRadius;
    public float SpeedFactor;
    public float MaxSteeringForce;
    
    public NativeArray<float2> Steering;
}