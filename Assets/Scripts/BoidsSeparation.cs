// Copyright (c) [2024] [Federico Grenoville]

using Unity.Collections;
using Unity.Mathematics;

public struct BoidsSeparation
{
    public float PerceptionRadius;
    public float PerceptionAngle;
    public float MinDistanceWeight;
    public float MaxDistanceWeight;
    public float DesiredMagnitude;
    public float MaxSteeringForce;
    public float ScaleForce;
    
    public NativeArray<float2> Steering;
}