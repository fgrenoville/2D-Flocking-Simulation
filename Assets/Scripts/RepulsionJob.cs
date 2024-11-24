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
/// RepulsionJob: every entity gets moved away from a specific location
/// (your mouse pointer in case of a double click).
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

    public void Execute(int index)
    {
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