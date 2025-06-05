// Copyright (c) [2024] [Federico Grenoville]

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

/// <summary>
/// Computes the alignment steering force for each agent (boid) in the simulation.
/// Each boid attempts to align its velocity with the average direction of neighboring boids
/// within its perception radius and field of view. The influence of each neighbor is weighted
/// based on its distance.
/// </summary>
[BurstCompile(CompileSynchronously = true)]
public struct AlignmentJob : IJobParallelFor
{
    private NativeArray<float2> _alignmentSteering;
 
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
    /// Starts and schedules the job for parallel execution.
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
        var job = new AlignmentJob()
        {
            _positions = positions,
            _velocities = velocities,
            _neighborsArray = neighborsArray,
            _neighborsCount = neighborsCount,
            _neighborsStartIndices = neighborsStartIndices,
            _alignmentSteering = steering,
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
    /// Calculates the alignment steering force for the boid at the specified index.
    /// This force is based on the average velocity direction of neighboring boids within
    /// the perception radius and angle. The further a neighbor is, the less influence it has,
    /// modulated by a remapped distance weight. The final steering vector is scaled and clamped
    /// to a maximum force.
    /// </summary>
    public void Execute(int index)
    {
        float2 currentPos = _positions[index];
        float2 currentVel = _velocities[index];
        
        int count = _neighborsCount[index];
        int startIdx = _neighborsStartIndices[index];

        if (count <= 0)
        {
            _alignmentSteering[index] = float2.zero;
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
                
                if (angleDegToNeighbor < (_perceptionAngle / 2))
                {
                    float weightedDistance = math.remap(0f, _perceptionRadius, _minDistanceWeight, _maxDistanceWeight, distance);
                    
                    desired += math.normalize(_velocities[neighborIndex]) * weightedDistance;
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

            _alignmentSteering[index] = steering;
        }
        else
        {
            _alignmentSteering[index] = float2.zero;
        }
    }
}