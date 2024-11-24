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
/// AttractionJob: every entity orbits around a specific location (your mouse pointer
/// when you keep pressing the right button).
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

    public void Execute(int index)
    {
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
                steering = float2.zero;
            }
            else
            {
                steering = distToAttractionPos * t * t;
                float magn = math.clamp(math.length(steering), _minSteeringForce, _maxSteeringForce);
                steering = math.normalize(steering) * magn;
            }
        }
        _attractionSteering[index] = steering;
    }
}