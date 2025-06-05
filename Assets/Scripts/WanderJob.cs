// Copyright (c) [2024] [Federico Grenoville]

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

/// <summary>
/// Compute predator's next movement direction using a wandering behavior.
/// This simulates organic, unpredictable motion by projecting a target in front of the agent
/// and applying random angular offsets each frame.
/// </summary>
[BurstCompile(CompileSynchronously = true)]
struct WanderJob : IJobParallelFor
{
    private NativeArray<float2> _steering;
    private NativeArray<float> _wanderTheta;

    [ReadOnly] private NativeArray<float2> _positions;
    [ReadOnly] private NativeArray<float2> _velocities;
    [ReadOnly] private float _maxSpeed;
    [ReadOnly] private float _wanderRadius;
    [ReadOnly] private float _wanderDistance;
    [ReadOnly] private float _maxSteeringForce;
    [ReadOnly] private float _variationRange;
    [ReadOnly] private uint _seed;
    
    /// <summary>
    /// Schedules the wander job across all agents.
    /// </summary>
    public static JobHandle Begin(NativeArray<float2> positions, 
        NativeArray<float2> velocities, 
        NativeArray<float> wanderTheta,
        NativeArray<float2> steering,
        float maxSpeed,
        float wanderRadius,
        float wanderDistance,
        float maxSteeringForce,
        float variationRange,
        uint seed)
    {
        var job = new WanderJob()
        {
            _positions = positions,
            _velocities = velocities,
            _wanderTheta = wanderTheta,
            _steering = steering,
            _maxSpeed = maxSpeed,
            _wanderRadius = wanderRadius,
            _wanderDistance = wanderDistance,
            _maxSteeringForce = maxSteeringForce,
            _variationRange = variationRange,
            _seed = seed
        };

        return IJobParallelForExtensions.Schedule(job, positions.Length, 32);
    }

    /// <summary>
    /// Executes the wander logic per predator agent.
    /// Produces a small random angle change to the projected forward direction,
    /// and steers the agent toward the updated target point.
    /// </summary>
    public void Execute(int index)
    {
        float2 currentPos = _positions[index];
        float2 currentVel = _velocities[index];
        float wanderTheta = _wanderTheta[index];
        
        // Generate a deterministic random float per-agent (based on index)
        Unity.Mathematics.Random rnd = new Unity.Mathematics.Random(_seed + (uint)index * 1000);
        float randomVal = rnd.NextFloat(-_variationRange, +_variationRange);
        wanderTheta += randomVal;

        float2 direction = math.normalize(currentVel);
        float2 circlePos = direction * _wanderDistance;
        circlePos += currentPos;
        
        float angle = math.atan2(direction.y, direction.x) * MathConstants.Rad2Deg - 90f;
        
        float2 circleOffset =
            new float2(_wanderRadius * math.cos(wanderTheta + angle), _wanderRadius * math.sin(wanderTheta + angle));

        float2 targetPos = circlePos + circleOffset;

        float2 desired = targetPos - currentPos;
        float2 desiredDir = math.normalize(desired);

        desired = desiredDir * _maxSpeed;
        float2 steering = desired - currentVel;
        
        float steeringMagnitude = math.length(steering);
        if (steeringMagnitude > _maxSteeringForce)
            steering = math.normalize(steering) * _maxSteeringForce;
        
        _steering[index] = steering;
        _wanderTheta[index] = wanderTheta;
    }
}