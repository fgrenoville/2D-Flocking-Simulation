// Copyright (c) [2024] [Federico Grenoville]

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

/// <summary>
/// Updates the position, velocity, and rotation of each boid in the simulation.
/// This job runs in parallel using Unity's Job System and processes each boid's movement based on the
/// combined steering forces from various behaviors (alignment, separation, cohesion, attraction, repulsion, escape).
/// 
/// Features:
/// - Applies accumulated steering vectors to velocity.
/// - Applies drag to simulate fluid resistance.
/// - Enforces a minimum speed to prevent stalling.
/// - Updates rotation to match movement direction.
/// - Clamps position within defined simulation bounds and reflects velocity on border collision.
/// 
/// This job directly modifies the associated Transform via TransformAccessArray to reflect
/// the updated position and heading of the boid in the Unity scene.
/// </summary>
[BurstCompile(CompileSynchronously = true)]
public struct BoidTransformJob : IJobParallelForTransform
{
    private NativeArray<float2> _positions;
    private NativeArray<float2> _velocities;

    [ReadOnly] private NativeArray<float2> _alignmentSteering;
    [ReadOnly] private NativeArray<float2> _separationSteering;
    [ReadOnly] private NativeArray<float2> _cohesionSteering;
    [ReadOnly] private NativeArray<float2> _attractionSteering;
    [ReadOnly] private NativeArray<float2> _repulsionSteering;
    [ReadOnly] private NativeArray<float2> _escapeSteering;
    [ReadOnly] private float _minSpeed;
    [ReadOnly] private float _dragFactor;
    [ReadOnly] private float2 _xBounds;
    [ReadOnly] private float2 _yBounds;
    [ReadOnly] private float _dt;
    
    /// <summary>
    /// Starts and schedules the job for parallel execution.
    /// </summary>
    public static JobHandle Begin(NativeArray<float2> positions, 
        NativeArray<float2> velocities, 
        NativeArray<float2> alignmentSteering,
        NativeArray<float2> separationSteering,
        NativeArray<float2> cohesionSteering,
        NativeArray<float2> attractionSteering,
        NativeArray<float2> repulsionSteering,
        NativeArray<float2> escapeSteering,
        TransformAccessArray transformAccArray,
        JobHandle dependencies,
        float minSpeed,
        float dragFactor,
        float2 xBounds,
        float2 yBounds,
        float dt)
    {
        var job = new BoidTransformJob()
        {
            _positions = positions,
            _velocities = velocities,
            _alignmentSteering = alignmentSteering,
            _separationSteering = separationSteering,
            _cohesionSteering = cohesionSteering,
            _attractionSteering = attractionSteering,
            _repulsionSteering = repulsionSteering,
            _escapeSteering = escapeSteering,
            _minSpeed = minSpeed,
            _dragFactor = dragFactor,
            _xBounds = xBounds,
            _yBounds = yBounds,
            _dt = dt
        };
        
        return IJobParallelForTransformExtensions.Schedule(job, transformAccArray, dependencies);
    }

    /// <summary>
    /// Executes the movement and orientation logic for a single boid.
    /// Applies all active steering behaviors, drag, minimum speed enforcement,
    /// and boundary clamping. Updates the boid's transform accordingly.
    /// </summary>
    public void Execute(int index, TransformAccess transform)
    {
        float2 acceleration = float2.zero;
        float2 velocity = _velocities[index];
        float2 pos = _positions[index];
        
        // Sum all steering behavior forces
        acceleration += _alignmentSteering[index] + _separationSteering[index] +
                        _cohesionSteering[index] + _attractionSteering[index] +
                        _repulsionSteering[index] + _escapeSteering[index];

        // Consideration: if you are looking for a more accurate physics simulation
        // you should multiply acceleration times delta-time.
        // Keep in mind that doing that lead you to re-tune every simulation
        // parameter.
        velocity += acceleration;

        // Drag calculation
        float sqr = math.length(velocity);
        float drag = 1f/2f * _dragFactor * sqr * sqr;
        float2 dragV = math.normalize(velocity) * -1f * drag;
        if (math.length(velocity) <= math.length(dragV))
        {
            velocity = math.normalize(velocity) * 0.001f;
        }
        else
        {
            velocity += dragV;
        }
        
        float2 newVelocityNormalized = math.normalize(velocity);
        
        // Enforce minimum speed
        float magnitude = math.length(velocity);
        if (magnitude < _minSpeed)
            velocity = newVelocityNormalized * _minSpeed;
        
        // Rotation update
        float angle = math.atan2(newVelocityNormalized.y, newVelocityNormalized.x) * MathConstants.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        
        // Position update
        pos += velocity * _dt;
        transform.position = new Vector3(pos.x, pos.y, 0);
        
        // Store updated state
        _positions[index] = pos;
        _velocities[index] = velocity;
        
        bool ran = false;
        if (transform.position.x < (_xBounds.x) || transform.position.x > (_xBounds.y))
        {
            velocity = new float2(velocity.x * -1, velocity.y);
            ran = true;
        }
        if (transform.position.y < (_yBounds.x) || transform.position.y > (_yBounds.y))
        {
            velocity = new float2(velocity.x, velocity.y * -1);
            ran = true;
        }

        if (ran)
        {
            pos = new float2(math.clamp(transform.position.x, _xBounds.x, _xBounds.y),
                math.clamp(transform.position.y, _yBounds.x, _yBounds.y));
            
            transform.position = new Vector3(pos.x, pos.y, 0);
            _positions[index] = pos;
            _velocities[index] = velocity;
        }
    }
}