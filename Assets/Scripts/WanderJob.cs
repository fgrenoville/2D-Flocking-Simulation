/*
 *  MIT License
 *
 *  Copyright (c) [2024] [Federico Grenoville]
 *   
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in all
 *  copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 *  SOFTWARE.
 */

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

/// <summary>
/// WanderJob: compute predator's next move without a specific behavior.
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

    public void Execute(int index)
    {
        float2 currentPos = _positions[index];
        float2 currentVel = _velocities[index];
        float wanderTheta = _wanderTheta[index];
        
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