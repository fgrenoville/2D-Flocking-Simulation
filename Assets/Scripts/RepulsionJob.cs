// Copyright (c) [2024] [Federico Grenoville]

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

/// <summary>
/// Applies a repulsion force to each agent away from a specified position
/// (typically triggered by a double-click or similar user input).
/// </summary>
[BurstCompile(CompileSynchronously = true)]
struct RepulsionJob : IJobParallelFor
{
    private NativeArray<float2> _repulsionSteering;

    [ReadOnly] private NativeArray<float2> _positions;
    [ReadOnly] private NativeArray<float2> _velocities;
    [ReadOnly] private float _interactionRadius;
    [ReadOnly] private float _speedFactor;
    [ReadOnly] private float _maxSteeringForce;
    [ReadOnly] private float2 _repulsionPosition;
    [ReadOnly] private bool _compute; 
    
    /// <summary>
    /// Schedules the job to compute repulsion forces.
    /// </summary>
    public static JobHandle Begin(NativeArray<float2> positions, 
        NativeArray<float2> velocities, 
        NativeArray<float2> steering,
        float interactionRadius,
        float speedFactor,
        float maxSteeringForce,
        float2 repulsionPosition,
        bool compute)
    {
        var job = new RepulsionJob()
        {
            _positions = positions,
            _velocities = velocities,
            _repulsionSteering = steering,
            _interactionRadius = interactionRadius,
            _speedFactor = speedFactor,
            _maxSteeringForce = maxSteeringForce,
            _repulsionPosition = repulsionPosition,
            _compute = compute
        };

        return IJobParallelForExtensions.Schedule(job, positions.Length, 32);
    }

    /// <summary>
    /// Computes a repulsion steering vector for each entity based on proximity
    /// to the repulsion point.
    /// </summary>
    public void Execute(int index)
    {
        // Skip computation if not active this frame
        if (!_compute)
        {
            _repulsionSteering[index] = float2.zero;
            return;
        }
        
        float2 currentPos = _positions[index];
        float2 currentVel = _velocities[index];

        float2 steering = float2.zero; 
        
        float2 distToRepulsionPos = currentPos - _repulsionPosition;
        
        float distance = math.length(distToRepulsionPos);
        if (distance < _interactionRadius)
        {
            float2 desired = math.normalize(distToRepulsionPos) * _speedFactor;
            steering = desired - currentVel;
            
            float steeringMagnitude = math.length(steering);
            if (steeringMagnitude > _maxSteeringForce)
                steering = math.normalize(steering) * _maxSteeringForce;
        }
        
        _repulsionSteering[index] = steering;
    }
}