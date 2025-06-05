// Copyright (c) [2024] [Federico Grenoville]

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

/// <summary>
/// Applies an attraction steering force to each entity in the simulation.
/// Entities within a specified radius are influenced to move toward a target position,
/// orbiting it if they remain within the orbit threshold.
/// 
/// This job is typically triggered when the user holds the right mouse button.
/// The computed force is modulated by a distance-based curve to simulate natural orbiting behavior.
/// </summary>
[BurstCompile(CompileSynchronously = true)]
struct AttractionJob : IJobParallelFor
{
    private NativeArray<float2> _attractionSteering;

    [ReadOnly] private NativeArray<float2> _positions;
    [ReadOnly] private NativeArray<float2> _velocities;
    [ReadOnly] private float _interactionRadius;
    [ReadOnly] private float _speedFactor;
    [ReadOnly] private float _minSteeringForce;
    [ReadOnly] private float _maxSteeringForce;
    [ReadOnly] private float _orbitRadius;
    [ReadOnly] private float2 _attractionPosition;
    [ReadOnly] private bool _compute; 
    
    /// <summary>
    /// Starts and schedules the job for parallel execution.
    /// </summary>
    public static JobHandle Begin(NativeArray<float2> positions, 
        NativeArray<float2> velocities, 
        NativeArray<float2> steering,
        float interactionRadius,
        float speedFactor,
        float minSteeringForce,
        float maxSteeringForce,
        float orbitRadius,
        float2 attractionPosition,
        bool compute)
    {
        var job = new AttractionJob()
        {
            _positions = positions,
            _velocities = velocities,
            _attractionSteering = steering,
            _interactionRadius = interactionRadius,
            _speedFactor = speedFactor,
            _minSteeringForce = minSteeringForce,
            _maxSteeringForce = maxSteeringForce,
            _orbitRadius = orbitRadius,
            _attractionPosition = attractionPosition,
            _compute = compute
        };

        return IJobParallelForExtensions.Schedule(job, positions.Length, 32);
    }

    /// <summary>
    /// Calculates the steering force that causes each entity to orbit around a target position
    /// (e.g., the mouse pointer when right-click is held). Entities inside the interaction radius
    /// experience a force pulling them toward the orbit path, simulating attraction.
    /// </summary>
    public void Execute(int index)
    {
        // If attraction is not active this frame, return zero steering
        if (!_compute)
        {
            _attractionSteering[index] = float2.zero;
            return;
        }
        
        float2 currentPos = _positions[index];
        float2 currentVel = _velocities[index];

        float2 steering = float2.zero; 
        
        float2 distToAttractionPos = _attractionPosition - currentPos;
        
        float distance = math.length(distToAttractionPos);
        if (distance < _interactionRadius)
        {
            float2 desired = math.normalize(distToAttractionPos) * _speedFactor;
            
            steering = desired - currentVel;
            float t = math.length(distToAttractionPos) / _orbitRadius;

            if (t < 0.35f)
            {
                // Inside orbit: stop attraction to simulate orbital stasis
                steering = float2.zero;
            }
            else
            {
                // Outside orbit: apply a steering force based on squared distance ratio
                steering = distToAttractionPos * t * t;
                float magn = math.clamp(math.length(steering), _minSteeringForce, _maxSteeringForce);
                steering = math.normalize(steering) * magn;
            }
        }
        _attractionSteering[index] = steering;
    }
}