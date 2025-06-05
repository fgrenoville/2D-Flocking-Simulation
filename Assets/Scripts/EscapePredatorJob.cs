// Copyright (c) [2024] [Federico Grenoville]

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

/// <summary>
/// Calculates a steering force that moves each agent away from nearby predators.
/// Each predator is treated as a threat, and agents will try to flee when they enter the perception radius.
/// </summary>
[BurstCompile(CompileSynchronously = true)]
struct EscapePredatorJob : IJobParallelFor
{
    private NativeArray<float2> _escapeSteering;

    [ReadOnly] private NativeArray<float2> _positions;
    [ReadOnly] private NativeArray<float2> _velocities;
    [ReadOnly] private NativeArray<float2> _predatorsPositions;
    [ReadOnly] private float _perceptionRadius;
    [ReadOnly] private float _speedFactor;
    [ReadOnly] private float _maxSteeringForce;   
    
    /// <summary>
    /// Schedules the EscapePredatorJob for parallel execution.
    /// </summary>
    public static JobHandle Begin(NativeArray<float2> positions, 
        NativeArray<float2> velocities, 
        NativeArray<float2> predatorsPositions,
        NativeArray<float2> escapeSteering,
        float perceptionRadius,
        float speedFactor,
        float maxSteeringForce)
    {
        var job = new EscapePredatorJob()
        {
            _positions = positions,
            _velocities = velocities,
            _predatorsPositions = predatorsPositions,
            _escapeSteering = escapeSteering,
            _perceptionRadius = perceptionRadius,
            _speedFactor = speedFactor,
            _maxSteeringForce = maxSteeringForce
        };

        return IJobParallelForExtensions.Schedule(job, positions.Length, 32);
    }

    /// <summary>
    /// Computes the total steering force for a boid based on nearby predators.
    /// The force is the sum of individual escape forces from each predator in range.
    /// </summary>
    public void Execute(int index)
    {
        float2 currentPos = _positions[index];
        float2 currentVel = _velocities[index];
        
        float2 steering = float2.zero;
        
        for (int i = 0; i < _predatorsPositions.Length; i++)
        {
            float2 distToPredator = currentPos - _predatorsPositions[i];
            
            float distance = math.length(distToPredator); 
            if (distance < _perceptionRadius)
            {
                // Compute desired velocity away from predator
                float2 desired = math.normalize(distToPredator) * _speedFactor;
                // Steering is the difference from current velocity
                float2 partialSteering = desired - currentVel;
                
                // Clamp steering to max force
                float steeringMagnitude = math.length(partialSteering);
                if (steeringMagnitude > _maxSteeringForce)
                    partialSteering = math.normalize(partialSteering) * _maxSteeringForce;

                // Accumulate steering from multiple predators
                steering += partialSteering;
            }
        }
        _escapeSteering[index] = steering;
    }
}