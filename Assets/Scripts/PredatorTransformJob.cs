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
using UnityEngine;
using UnityEngine.Jobs;

/// <summary>
/// PredatorTransformJob: calculate the predator's new position and move accordingly
/// its transform.
/// </summary>
[BurstCompile(CompileSynchronously = true)]
public struct PredatorTransformJob : IJobParallelForTransform
{
    private NativeArray<float2> _positions;
    private NativeArray<float2> _velocities;

    [ReadOnly] private NativeArray<float2> _steering;
    [ReadOnly] private float _minSpeed;
    [ReadOnly] private float _maxSpeed;
    [ReadOnly] private float2 _xBounds;
    [ReadOnly] private float2 _yBounds;
    [ReadOnly] private float _dt;
    
    public static JobHandle Begin(NativeArray<float2> positions, 
        NativeArray<float2> velocities,
        NativeArray<float2> steering,
        TransformAccessArray transformAccArray,
        JobHandle dependencies,
        float minSpeed,
        float maxSpeed,
        float2 xBounds,
        float2 yBounds,
        float dt)
    {
        var job = new PredatorTransformJob()
        {
            _positions = positions,
            _velocities = velocities,
            _steering = steering,
            _minSpeed = minSpeed,
            _maxSpeed = maxSpeed,
            _xBounds = xBounds,
            _yBounds = yBounds,
            _dt = dt
        };
        
        return IJobParallelForTransformExtensions.Schedule(job, transformAccArray, dependencies);
    }

    public void Execute(int index, TransformAccess transform)
    {
        float2 acceleration = float2.zero;
        float2 velocity = _velocities[index];
        float2 pos = _positions[index];
        
        acceleration += _steering[index];

        // Consideration: if you are looking for a more accurate physics simulation
        // you should multiply acceleration times delta-time.
        // Keep in mind that doing that lead you to re-tune every simulation
        // parameter.
        velocity += acceleration;
        
        float2 newVelocityNormalized = math.normalize(velocity);
        
        float magnitude = math.length(velocity);
        velocity = newVelocityNormalized *
                   math.clamp(magnitude, _minSpeed, _maxSpeed);
        
        float angle = math.atan2(newVelocityNormalized.y, newVelocityNormalized.x) * MathConstants.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        pos += velocity * _dt;
        
        transform.position = new Vector3(pos.x, pos.y, 0);
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