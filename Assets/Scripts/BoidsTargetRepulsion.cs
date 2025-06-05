// Copyright (c) [2024] [Federico Grenoville]

using Unity.Collections;
using Unity.Mathematics;

public struct BoidsTargetRepulsion
{
    public float InteractionRadius;
    public float SpeedFactor;
    public float MaxSteeringForce;
    
    public NativeArray<float2> Steering;
}