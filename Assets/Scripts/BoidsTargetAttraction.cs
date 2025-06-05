// Copyright (c) [2024] [Federico Grenoville]

using Unity.Collections;
using Unity.Mathematics;

public struct BoidsTargetAttraction
{
    public float InteractionRadius;
    public float SpeedFactor;
    public float MinSteeringForce;
    public float MaxSteeringForce;
    public float OrbitRadius;
    
    public NativeArray<float2> Steering;
}