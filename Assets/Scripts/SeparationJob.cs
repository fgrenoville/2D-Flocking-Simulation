// Copyright (c) [2024] [Federico Grenoville]

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

/// <summary>
/// Computes a steering force that keeps agents away from neighbors to avoid crowding.
/// Implements separation behavior as part of the flocking system.
/// </summary>
[BurstCompile(CompileSynchronously = true)]
struct SeparationJob : IJobParallelFor
{
    private NativeArray<float2> _separationSteering;

    [ReadOnly] private NativeArray<float2> _positions;
    [ReadOnly] private NativeArray<float2> _velocities;
    [ReadOnly] private NativeArray<int> _neighborsArray;
    [ReadOnly] private NativeArray<int> _neighborsCount;
    [ReadOnly] private NativeArray<int> _neighborsStartIndices;
    [ReadOnly] private int _movementAccuracy;
    [ReadOnly] private float _perceptionRadius;
    [ReadOnly] private float _perceptionAngle;
    [ReadOnly] private float _minDistanceWeight;
    [ReadOnly] private float _maxDistanceWeight;
    [ReadOnly] private float _desiredMagnitude;
    [ReadOnly] private float _maxSteeringForce;
    [ReadOnly] private float _scaleForce;
   
    /// <summary>
    /// Schedules the separation behavior job.
    /// </summary>
    public static JobHandle Begin(NativeArray<float2> positions, 
        NativeArray<float2> velocities, 
        NativeArray<int> neighborsArray,
        NativeArray<int> neighborsCount,
        NativeArray<int> neighborsStartIndices,
        NativeArray<float2> steering,
        int movementAccuracy,
        float perceptionRadius,
        float perceptionAngle,
        float minDistanceWeight,
        float maxDistanceWeight,
        float desiredMagnitude,
        float maxSteeringForce,
        float scaleForce)
    {
        var job = new SeparationJob()
        {
            _positions = positions,
            _velocities = velocities,
            _neighborsArray = neighborsArray,
            _neighborsCount = neighborsCount,
            _neighborsStartIndices = neighborsStartIndices,
            _separationSteering = steering,
            _movementAccuracy = movementAccuracy,
            _perceptionRadius = perceptionRadius,
            _perceptionAngle = perceptionAngle,
            _minDistanceWeight = minDistanceWeight,
            _maxDistanceWeight = maxDistanceWeight,
            _desiredMagnitude = desiredMagnitude,
            _maxSteeringForce = maxSteeringForce,
            _scaleForce = scaleForce
        };

        return IJobParallelForExtensions.Schedule(job, positions.Length, 32);
    }
    
    /// <summary>
    /// Computes the separation steering vector for each entity.
    /// </summary>
    public void Execute(int index)
    {
        float2 currentPos = _positions[index];
        float2 currentVel = _velocities[index];
        
        int count = _neighborsCount[index];
        int startIdx = _neighborsStartIndices[index];

        // If no neighbors, steering is zero
        if (count <= 0)
        {
            _separationSteering[index] = float2.zero;
            return;
        }

        bool ran = false;
        float2 desired = float2.zero;
        
        for (int i = 0; i < count && i < _movementAccuracy; i++)
        {
            int neighborIndex = _neighborsArray[startIdx + i];

            float2 neighborPosition = _positions[neighborIndex];
            float2 distToNeighbor = currentPos - neighborPosition;
            
            float distance = math.length(distToNeighbor);
            if (distance < _perceptionRadius)
            {
                float2 dirToNeighbor = (_positions[neighborIndex] - currentPos);
                dirToNeighbor = math.normalize(dirToNeighbor);

                float2 forward = math.normalize(currentVel);
                float dot = math.dot(forward, dirToNeighbor);
                float angleToNeighbor = math.acos(dot);
                float angleDegToNeighbor = math.degrees(angleToNeighbor);
                
                // If the neighbor is within field of view
                if (angleDegToNeighbor < (_perceptionAngle / 2))
                {
                    float weightedDistance = math.remap(0f, _perceptionRadius, _minDistanceWeight, _maxDistanceWeight, distance);
                    
                    desired += math.normalize(distToNeighbor) * weightedDistance;
                    ran = true;
                }
            }
        }

        if (ran)
        {
            desired = math.normalize(desired) * _desiredMagnitude;
            
            float2 steering = desired - currentVel;
            
            float steeringMagnitude = math.length(steering);
            if (steeringMagnitude > _maxSteeringForce)
                steering = math.normalize(steering) * _maxSteeringForce;

            steering *= _scaleForce;

            _separationSteering[index] = steering;
        }
        else
        {
            _separationSteering[index] = float2.zero;
        }
    }
}