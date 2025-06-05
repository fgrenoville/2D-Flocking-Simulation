// Copyright (c) [2024] [Federico Grenoville]

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Compute entity's color based on its speed.
/// The faster the agent moves, the more its color shifts from green toward blue.
/// Red remains constant to maintain a warm base hue.
/// Emission intensity also increases with speed.
/// </summary>
[BurstCompile(CompileSynchronously = true)]
public struct TextureColorJob : IJobParallelFor
{
    private NativeArray<float3> _texColor;
    private NativeArray<float> _intensity;

    [ReadOnly] private NativeArray<float2> _velocities;
    [ReadOnly] private float _minSpeed;
    [ReadOnly] private float _maxSpeed;
    
    /// <summary>
    /// Schedules the texture job.
    /// </summary>
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

    /// <summary>
    /// Executes color and intensity computation for each agent.
    /// Color hue and emission are tied to the speed of the agent.
    /// </summary>
    public void Execute(int index)
    {
        float2 velocity = _velocities[index];
        
        // Compute normalized speed (0 to 1) relative to min and max speed
        // This value 't' is used to control color intensity and hue
        float t = Mathf.InverseLerp(_minSpeed, _maxSpeed, math.length(velocity));

        // Interpolate color based on speed factor 't'
        // Red is fixed at full intensity
        // Green fades out as speed increases (from greenish to reddish)
        // Blue increases as speed increases (adds cyan at high speed)
        float red = math.lerp(1f, 1f, t);
        float green = math.lerp(1, 0f, t);
        float blue = math.lerp(0.5f, 1f, t);

        // Assign the interpolated color to the agent
        _texColor[index] = new float3(red, green, blue);
        // Set emission intensity to match speed factor (used for visual glow)
        _intensity[index] = t;
    }
}