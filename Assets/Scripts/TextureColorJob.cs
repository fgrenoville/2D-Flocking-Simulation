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

/// <summary>
/// TextureColorJob: compute entity's color based on its speed.
/// </summary>
[BurstCompile(CompileSynchronously = true)]
public struct TextureColorJob : IJobParallelFor
{
    private NativeArray<float3> _texColor;
    private NativeArray<float> _intensity;

    [ReadOnly] private NativeArray<float2> _velocities;
    [ReadOnly] private float _minSpeed;
    [ReadOnly] private float _maxSpeed;
    
    public static JobHandle Begin(NativeArray<float2> velocities,
        float minSpeed,
        float maxSpeed,
        NativeArray<float3> texColor,
        NativeArray<float> intensity)
    {
        var job = new TextureColorJob()
        {
            _velocities = velocities,
            _minSpeed = minSpeed,
            _maxSpeed = maxSpeed,
            _texColor = texColor,
            _intensity = intensity
        };

        return IJobParallelForExtensions.Schedule(job, velocities.Length, 32);
    }

    public void Execute(int index)
    {
        float2 velocity = _velocities[index];
        
        float t = Mathf.InverseLerp(_minSpeed, _maxSpeed, math.length(velocity));

        float red = math.lerp(1f, 1f, t);
        float green = math.lerp(1, 0f, t);
        float blue = math.lerp(0.5f, 1f, t);

        _texColor[index] = new float3(red, green, blue);
        _intensity[index] = t;
    }
}