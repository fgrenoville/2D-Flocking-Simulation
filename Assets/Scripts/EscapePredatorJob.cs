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
/// Escape: every entity tries to escape from a predator, a different entity that
/// wander in the same space but without a specific behavior.
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
                float2 desired = math.normalize(distToPredator) * _speedFactor;
                float2 partialSteering = desired - currentVel;
                
                float steeringMagnitude = math.length(partialSteering);
                if (steeringMagnitude > _maxSteeringForce)
                    partialSteering = math.normalize(partialSteering) * _maxSteeringForce;

                steering += partialSteering;
            }
        }
        _escapeSteering[index] = steering;
    }
}