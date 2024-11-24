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

using System;
using System.Collections;
using System.Collections.Generic;
using Config;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class Flocking : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private QuadTree _quadTree;
    [SerializeField] private ConfigLoader _configLoader;
    [SerializeField] private GameObject _boidsPrefab;
    [SerializeField] private GameObject _predatorsPrefab;
    [SerializeField] private Volume _globalVolume;

    [Header("General")]
    [SerializeField] private bool _usePostProcessingEffects;
    
    [Header("Flocking")]
    [SerializeField] private int _boidsSpawnAtStart = 3;
    [SerializeField] private int _predatorsSpawnAtStart = 2;

    [Header("Boids behavior")] 
    [SerializeField, Range(0f, 20f)] private float _boidsMinSpeed;
    [SerializeField, Range(0.1f, 50f)] private float _boidsMaxSpeed;
    [SerializeField, Range(3, 600f)] private int _movementAccuracy;
    [SerializeField, Range(0.0001f, 3f)] private float _dragFactor;
    
    [Header("Predators behavior")] 
    [SerializeField, Range(0f, 20f)] private float _predatorsMinSpeed;
    [SerializeField, Range(0.1f, 50f)] private float _predatorsMaxSpeed;
    [SerializeField, Range(0f, 30f)] private float _predatorsWanderRadius;
    [SerializeField, Range(0f, 60f)] private float _predatorsWanderDistance;
    [SerializeField, Range(0f, 2f)] private float _predatorsMaxSteeringForce;
    [SerializeField, Range(0f, 10f)] private float _predatorsVariationRange;

    [Header("Boids Alignment")] 
    [SerializeField, Range(0f, 50f)] private float _alignmentPerceptionRadius;
    [SerializeField, Range(0f, 360f)] private float _alignmentPerceptionAngle;
    [SerializeField, Range(0.1f, 40f)] private float _alignmentMinDistanceWeight;
    [SerializeField, Range(0.1f, 40f)] private float _alignmentMaxDistanceWeight;
    [SerializeField, Range(1f, 20f)] private float _alignmentDesiredMagnitude;
    [SerializeField, Range(0f, 2f)] private float _alignmentMaxSteeringForce;
    [SerializeField, Range(0f, 5f)] private float _alignmentScaleForce;
    
    [Header("Boids Separation")] 
    [SerializeField, Range(0f, 50f)] private float _separationPerceptionRadius;
    [SerializeField, Range(0f, 360f)] private float _separationPerceptionAngle;
    [SerializeField, Range(0.1f, 40f)] private float _separationMinDistanceWeight;
    [SerializeField, Range(0.1f, 40f)] private float _separationMaxDistanceWeight;
    [SerializeField, Range(1f, 20f)] private float _separationDesiredMagnitude;
    [SerializeField, Range(0f, 2f)] private float _separationMaxSteeringForce;
    [SerializeField, Range(0f, 5f)] private float _separationScaleForce;
    
    [Header("Boids Cohesion")] 
    [SerializeField, Range(0f, 50f)] private float _cohesionPerceptionRadius;
    [SerializeField, Range(0f, 360f)] private float _cohesionPerceptionAngle;
    [SerializeField, Range(0.1f, 40f)] private float _cohesionMinDistanceWeight;
    [SerializeField, Range(0.1f, 40f)] private float _cohesionMaxDistanceWeight;
    [SerializeField, Range(1f, 20f)] private float _cohesionDesiredMagnitude;
    [SerializeField, Range(0f, 2f)] private float _cohesionMaxSteeringForce;
    [SerializeField, Range(0f, 5f)] private float _cohesionScaleForce;

    [Header("Boids Escape")] 
    [SerializeField, Range(0f, 50f)] private float _escapePerceptionRadius;
    [SerializeField, Range(0f, 18f)] private float _escapeSpeedFactor;
    [SerializeField, Range(0f, 18f)] private float _escapeMaxSteeringForce;
    
    [Header("Target attraction")] 
    [SerializeField, Range(1f, 50f)] private float _attractionInteractionRadius;
    [SerializeField, Range(0.1f, 50f)] private float _attractionSpeedFactor;
    [SerializeField, Range(0f, 2f)] private float _attractionMinSteeringForce;
    [SerializeField, Range(0f, 2f)] private float _attractionMaxSteeringForce;
    [SerializeField, Range(0.5f, 10f)] private float _attractionOrbitRadius;
    
    [Header("Target repulsion")] 
    [SerializeField, Range(1f, 50f)] private float _repulsionInteractionRadius;
    [SerializeField, Range(0.1f, 200f)] private float _repulsionSpeedFactor;
    [SerializeField, Range(0.01f, 20f)] private float _repulsionMaxSteeringForce;

    [SerializeField] private GameObject _circlePrefab;
    [SerializeField] private float _shrinkSpeed = 2f;
    
    private NativeArray<int> _neighborsStartIndices;
    private NativeArray<int> _neighborsCounts;
    private NativeArray<int> _neighborsArray;

    private float2 _xBounds;
    private float2 _yBounds;
    
    private JobHandle _alignmentJob;
    private JobHandle _separationJob;
    private JobHandle _cohesionJob;
    private JobHandle _attractionJob;
    private JobHandle _repulsionJob;
    private JobHandle _escapeJob;
    private JobHandle _boidsTransformJob;
    private JobHandle _boidsTextureColorJob;
    
    private JobHandle _wanderJob;
    private JobHandle _predatorsTransformJob;
    
    private NativeArray<JobHandle> _boidsJobHandles;
    private NativeArray<JobHandle> _predatorsJobHandles;
    
    private bool _spawnRequested;
    private Vector2 _spawnPosition;
    
    private bool _attractionRequested;
    private float2 _attractionPosition;

    private bool _repulsionRequested;
    private float2 _repulsionPosition;
    
    private Boids _boids;
    private Predators _predators;

    // Populates variables used during the simulation with values read by
    // configuration file.  
    private void SyncParamsFromFile()
    {
        if (_configLoader != null && _configLoader.SettingsSuccessfullyRead)
        {
            _usePostProcessingEffects = _configLoader.Settings.General.UsePostProcessingEffects;
            
            _boidsSpawnAtStart = _configLoader.Settings.Boids.SpawnAtStart.value;
            _boidsMinSpeed = _configLoader.Settings.Boids.Behavior.MinSpeed.value;
            _boidsMaxSpeed = _configLoader.Settings.Boids.Behavior.MaxSpeed.value;
            _movementAccuracy = _configLoader.Settings.Boids.Behavior.MovementAccuracy.value;
            _dragFactor = _configLoader.Settings.Boids.Behavior.DragFactor.value;
            
            _alignmentPerceptionRadius = _configLoader.Settings.Boids.Alignment.PerceptionRadius.value;
            _alignmentPerceptionAngle = _configLoader.Settings.Boids.Alignment.PerceptionAngle.value;
            _alignmentMinDistanceWeight = _configLoader.Settings.Boids.Alignment.MinDistanceWeight.value;
            _alignmentMaxDistanceWeight = _configLoader.Settings.Boids.Alignment.MaxDistanceWeight.value;
            _alignmentDesiredMagnitude = _configLoader.Settings.Boids.Alignment.DesiredMagnitude.value;
            _alignmentMaxSteeringForce = _configLoader.Settings.Boids.Alignment.MaxSteeringForce.value;
            _alignmentScaleForce = _configLoader.Settings.Boids.Alignment.ScaleForce.value;
            
            _separationPerceptionRadius = _configLoader.Settings.Boids.Separation.PerceptionRadius.value;
            _separationPerceptionAngle = _configLoader.Settings.Boids.Separation.PerceptionAngle.value;
            _separationMinDistanceWeight = _configLoader.Settings.Boids.Separation.MinDistanceWeight.value;
            _separationMaxDistanceWeight = _configLoader.Settings.Boids.Separation.MaxDistanceWeight.value;
            _separationDesiredMagnitude = _configLoader.Settings.Boids.Separation.DesiredMagnitude.value;
            _separationMaxSteeringForce = _configLoader.Settings.Boids.Separation.MaxSteeringForce.value;
            _separationScaleForce = _configLoader.Settings.Boids.Separation.ScaleForce.value;
            
            _cohesionPerceptionRadius = _configLoader.Settings.Boids.Cohesion.PerceptionRadius.value;
            _cohesionPerceptionAngle = _configLoader.Settings.Boids.Cohesion.PerceptionAngle.value;
            _cohesionMinDistanceWeight = _configLoader.Settings.Boids.Cohesion.MinDistanceWeight.value;
            _cohesionMaxDistanceWeight = _configLoader.Settings.Boids.Cohesion.MaxDistanceWeight.value;
            _cohesionDesiredMagnitude = _configLoader.Settings.Boids.Cohesion.DesiredMagnitude.value;
            _cohesionMaxSteeringForce = _configLoader.Settings.Boids.Cohesion.MaxSteeringForce.value;
            _cohesionScaleForce = _configLoader.Settings.Boids.Cohesion.ScaleForce.value;
            
            _escapePerceptionRadius = _configLoader.Settings.Boids.Escape.PerceptionRadius.value;
            _escapeSpeedFactor = _configLoader.Settings.Boids.Escape.SpeedFactor.value;
            _escapeMaxSteeringForce = _configLoader.Settings.Boids.Escape.MaxSteeringForce.value;
              
            _attractionInteractionRadius = _configLoader.Settings.Boids.Attraction.InteractionRadius.value;
            _attractionSpeedFactor = _configLoader.Settings.Boids.Attraction.SpeedFactor.value;
            _attractionMinSteeringForce = _configLoader.Settings.Boids.Attraction.MinSteeringForce.value;
            _attractionMaxSteeringForce = _configLoader.Settings.Boids.Attraction.MaxSteeringForce.value;
            _attractionOrbitRadius = _configLoader.Settings.Boids.Attraction.OrbitRadius.value;
               
            _repulsionInteractionRadius = _configLoader.Settings.Boids.Repulsion.InteractionRadius.value;
            _repulsionSpeedFactor = _configLoader.Settings.Boids.Repulsion.SpeedFactor.value;
            _repulsionMaxSteeringForce = _configLoader.Settings.Boids.Repulsion.MaxSteeringForce.value;

            _predatorsSpawnAtStart = _configLoader.Settings.Predators.SpawnAtStart.value;
            _predatorsMinSpeed = _configLoader.Settings.Predators.Behavior.MinSpeed.value;
            _predatorsMaxSpeed = _configLoader.Settings.Predators.Behavior.MaxSpeed.value;
            _predatorsWanderRadius = _configLoader.Settings.Predators.Behavior.WanderRadius.value;
            _predatorsWanderDistance = _configLoader.Settings.Predators.Behavior.WanderDistance.value;
            _predatorsMaxSteeringForce = _configLoader.Settings.Predators.Behavior.MaxSteeringForce.value;
            _predatorsVariationRange = _configLoader.Settings.Predators.Behavior.VariationRange.value;
        }
    }
    
    // Synchronizes the boids values that user could change during simulation
    // (for example through the user interface). 
    // Set it to true if you are calling this method from the update loop.
    // Set it to false if you are calling this method as part of the first initialization procedure.
    private void ReadParams(bool performedInUpdateLoop)
    {
        if (!performedInUpdateLoop)
        {
            _boids.SpawnAtStart = _boidsSpawnAtStart;
            _boids.Count = _boidsSpawnAtStart;
        }
        
        _boids.Behavior.MinSpeed = _boidsMinSpeed;
        _boids.Behavior.MaxSpeed = _boidsMaxSpeed;
        _boids.Behavior.MovementAccuracy = _movementAccuracy;
        _boids.Behavior.DragFactor = _dragFactor;
        
        _boids.Alignment.PerceptionRadius = _alignmentPerceptionRadius;
        _boids.Alignment.PerceptionAngle = _alignmentPerceptionAngle;
        _boids.Alignment.MinDistanceWeight = _alignmentMinDistanceWeight;
        _boids.Alignment.MaxDistanceWeight = _alignmentMaxDistanceWeight;
        _boids.Alignment.DesiredMagnitude = _alignmentDesiredMagnitude;
        _boids.Alignment.MaxSteeringForce = _alignmentMaxSteeringForce;
        _boids.Alignment.ScaleForce = _alignmentScaleForce;
        
        _boids.Separation.PerceptionRadius = _separationPerceptionRadius;
        _boids.Separation.PerceptionAngle = _separationPerceptionAngle;
        _boids.Separation.MinDistanceWeight = _separationMinDistanceWeight;
        _boids.Separation.MaxDistanceWeight = _separationMaxDistanceWeight;
        _boids.Separation.DesiredMagnitude = _separationDesiredMagnitude;
        _boids.Separation.MaxSteeringForce = _separationMaxSteeringForce;
        _boids.Separation.ScaleForce = _separationScaleForce;

        _boids.Cohesion.PerceptionRadius = _cohesionPerceptionRadius;
        _boids.Cohesion.PerceptionAngle = _cohesionPerceptionAngle;
        _boids.Cohesion.MinDistanceWeight = _cohesionMinDistanceWeight;
        _boids.Cohesion.MaxDistanceWeight = _cohesionMaxDistanceWeight;
        _boids.Cohesion.DesiredMagnitude = _cohesionDesiredMagnitude;
        _boids.Cohesion.MaxSteeringForce = _cohesionMaxSteeringForce;
        _boids.Cohesion.ScaleForce = _cohesionScaleForce;

        _boids.Escape.PerceptionRadius = _escapePerceptionRadius;
        _boids.Escape.SpeedFactor = _escapeSpeedFactor;
        _boids.Escape.MaxSteeringForce = _escapeMaxSteeringForce;
        
        _boids.TargetAttraction.InteractionRadius = _attractionInteractionRadius;
        _boids.TargetAttraction.SpeedFactor = _attractionSpeedFactor;
        _boids.TargetAttraction.MinSteeringForce = _attractionMinSteeringForce;
        _boids.TargetAttraction.MaxSteeringForce = _attractionMaxSteeringForce;
        _boids.TargetAttraction.OrbitRadius = _attractionOrbitRadius;

        _boids.TargetRepulsion.InteractionRadius = _repulsionInteractionRadius;
        _boids.TargetRepulsion.SpeedFactor = _repulsionSpeedFactor;
        _boids.TargetRepulsion.MaxSteeringForce = _repulsionMaxSteeringForce;

        if (!performedInUpdateLoop)
        {
            _predators.SpawnAtStart = _predatorsSpawnAtStart;
            _predators.Count = _predatorsSpawnAtStart;
        }

        _predators.Behavior.MinSpeed = _predatorsMinSpeed;
        _predators.Behavior.MaxSpeed = _predatorsMaxSpeed;
        _predators.Behavior.WanderRadius = _predatorsWanderRadius;
        _predators.Behavior.WanderDistance = _predatorsWanderDistance;
        _predators.Behavior.MaxSteeringForce = _predatorsMaxSteeringForce;
        _predators.Behavior.VariationRange = _predatorsVariationRange;

        _boids.MaxPerceptionRadius =
            Mathf.Max(_alignmentPerceptionRadius * 2, _separationPerceptionRadius * 2, _cohesionPerceptionRadius * 2);
    }

    private void Initialize(bool performedAfterManualSpawn)
    {
        if (!performedAfterManualSpawn)
        {
            _boids.Positions = new NativeArray<float2>(_boids.Count, Allocator.Persistent); 
            _boids.Velocities = new NativeArray<float2>(_boids.Count, Allocator.Persistent); 
            _boids.Objects = new List<GameObject>(_boids.Count + 200);
            _boids.Renderers = new List<SpriteRenderer>(_boids.Count + 200);
            _boids.PropertyBlocks = new List<MaterialPropertyBlock>(_boids.Count + 200);
        }
        
        _boids.Steering = new NativeArray<float2>(_boids.Count, Allocator.Persistent); 
        _boids.TextureColor = new NativeArray<float3>(_boids.Count, Allocator.Persistent);
        _boids.Intensity = new NativeArray<float>(_boids.Count, Allocator.Persistent);
        _boids.Alignment.Steering = new NativeArray<float2>(_boids.Count, Allocator.Persistent); 
        _boids.Separation.Steering = new NativeArray<float2>(_boids.Count, Allocator.Persistent); 
        _boids.Cohesion.Steering = new NativeArray<float2>(_boids.Count, Allocator.Persistent); 
        _boids.Escape.Steering = new NativeArray<float2>(_boids.Count, Allocator.Persistent); 
        _boids.TargetAttraction.Steering = new NativeArray<float2>(_boids.Count, Allocator.Persistent);
        _boids.TargetRepulsion.Steering = new NativeArray<float2>(_boids.Count, Allocator.Persistent);
        
        _neighborsStartIndices = new NativeArray<int>(_boids.Count, Allocator.Persistent);
        _neighborsCounts = new NativeArray<int>(_boids.Count, Allocator.Persistent);
        _neighborsArray = new NativeArray<int>(_boids.Count * _boids.Count, Allocator.Persistent);
        
        _boids.TransformsAccessArray = new TransformAccessArray(_boids.Count); 

        if (!performedAfterManualSpawn)
        {
            _predators.Positions = new NativeArray<float2>(_predators.Count, Allocator.Persistent);
            _predators.Velocities = new NativeArray<float2>(_predators.Count, Allocator.Persistent);
            _predators.Steering = new NativeArray<float2>(_predators.Count, Allocator.Persistent);
            _predators.WanderTheta = new NativeArray<float>(_predators.Count, Allocator.Persistent);
            _predators.TransformsAccessArray = new TransformAccessArray(_predators.Count);
            _predators.Objects = new List<GameObject>(_predators.Count + 20);
            _predators.Renderers = new List<SpriteRenderer>(_predators.Count + 20);
            _predators.PropertyBlocks = new List<MaterialPropertyBlock>(_predators.Count + 20);
        }
    }
    
    private void Clean(bool performedAfterManualSpawn)
    {
        if (!performedAfterManualSpawn)
        {
            _boids.Positions.Dispose();
            _boids.Velocities.Dispose();
        }

        _boids.Steering.Dispose();
        _boids.TextureColor.Dispose();
        _boids.Intensity.Dispose();
        _boids.Alignment.Steering.Dispose(); 
        _boids.Separation.Steering.Dispose(); 
        _boids.Cohesion.Steering.Dispose(); 
        _boids.Escape.Steering.Dispose(); 
        _boids.TargetAttraction.Steering.Dispose();
        _boids.TargetRepulsion.Steering.Dispose();
        
        _neighborsStartIndices.Dispose();
        _neighborsCounts.Dispose();
        _neighborsArray.Dispose();
        
        _boids.TransformsAccessArray.Dispose();

        if (!performedAfterManualSpawn)
        {
            _predators.Positions.Dispose();
            _predators.Velocities.Dispose();
            _predators.Steering.Dispose();
            _predators.WanderTheta.Dispose();
            _predators.TransformsAccessArray.Dispose();
        }
    }

    private void BoidsSpawn(int count)
    {
        for (int i = 0; i < count; i++)
        {
            float2 pos = new float2(UnityEngine.Random.Range(_quadTree.Bounds.x, _quadTree.Bounds.xMax), UnityEngine.Random.Range(_quadTree.Bounds.y, _quadTree.Bounds.yMax));
            _boids.Positions[i] = pos;
            _boids.Velocities[i] = new float2(Random.insideUnitCircle);
            float angle = Mathf.Atan2(_boids.Velocities[i].y, _boids.Velocities[i].x) * Mathf.Rad2Deg - 90f;
            
            GameObject go = Instantiate(_boidsPrefab, (Vector2)_boids.Positions[i], Quaternion.Euler(new Vector3(0, 0, angle)));
            go.name = $"Agent{i}";
            _boids.Objects.Add(go);
            
            var transf = go.GetComponent<Transform>();
            _boids.TransformsAccessArray.Add(transf);
            
            var sr = go.GetComponentInChildren<SpriteRenderer>();
            _boids.Renderers.Add(sr);

            _boids.PropertyBlocks.Add(new MaterialPropertyBlock());
        }
    }

    private void BoidsManualSpawn(int count)
    {
        int lastElement = count - 1;
        
        float2 pos = _spawnPosition;
        _boids.Positions[lastElement] = pos;
        _boids.Velocities[lastElement] = new float2(Random.insideUnitCircle);
        float angle = Mathf.Atan2(_boids.Velocities[lastElement].y, _boids.Velocities[lastElement].x) * Mathf.Rad2Deg - 90f;
        GameObject go = Instantiate(_boidsPrefab, (Vector2)_boids.Positions[lastElement], Quaternion.Euler(new Vector3(0, 0, angle)));
        go.name = $"Agent{lastElement}";
        _boids.Objects.Add(go);

        var sr = go.GetComponentInChildren<SpriteRenderer>();
        _boids.Renderers.Add(sr);
        
        _boids.PropertyBlocks.Add(new MaterialPropertyBlock());

        for (int i = 0; i < count; i++)
        {
            var transf = _boids.Objects[i].GetComponent<Transform>();
            _boids.TransformsAccessArray.Add(transf);
        }
    }

    private void PredatorsSpawn(int count)
    {
        for (int i = 0; i < count; i++)
        {
            float2 pos = new float2(UnityEngine.Random.Range(_quadTree.Bounds.x, _quadTree.Bounds.xMax), UnityEngine.Random.Range(_quadTree.Bounds.y, _quadTree.Bounds.yMax));
            _predators.Positions[i] = pos;
            _predators.Velocities[i] = new float2(Random.insideUnitCircle); 
            float angle = Mathf.Atan2(_predators.Velocities[i].y, _predators.Velocities[i].x) * Mathf.Rad2Deg - 90f;
            
            GameObject go = Instantiate(_predatorsPrefab, (Vector2)_predators.Positions[i], Quaternion.Euler(new Vector3(0, 0, angle)));
            go.name = $"Predator{i}";
            _predators.Objects.Add(go);
            
            var transf = go.GetComponent<Transform>();
            _predators.TransformsAccessArray.Add(transf);

            var sr = go.GetComponentInChildren<SpriteRenderer>();
            _predators.Renderers.Add(sr);
            
            _predators.PropertyBlocks.Add(new MaterialPropertyBlock());

            _predators.WanderTheta[i] = 0f;
        }
    }
    
    void Start()
    {
        SyncParamsFromFile();
        ReadParams(false);
        Initialize(false);
        
        BoidsSpawn(_boids.Count);
        PredatorsSpawn(_predators.Count);

        _xBounds = new float2(_quadTree.Bounds.xMin, _quadTree.Bounds.xMax);
        _yBounds = new float2(_quadTree.Bounds.yMin, _quadTree.Bounds.yMax);
    }

    void Update()
    {
        _alignmentJob.Complete();
        _separationJob.Complete();
        _cohesionJob.Complete();
        _attractionJob.Complete();
        _repulsionJob.Complete();
        _escapeJob.Complete();
        _wanderJob.Complete();
        _boidsTransformJob.Complete();
        _boidsTextureColorJob.Complete();
        _predatorsTransformJob.Complete();
        
        if (_alignmentJob.IsCompleted && _separationJob.IsCompleted && _cohesionJob.IsCompleted 
            && _attractionJob.IsCompleted && _repulsionJob.IsCompleted && _escapeJob.IsCompleted && _wanderJob.IsCompleted && 
            _boidsTransformJob.IsCompleted && _boidsTextureColorJob.IsCompleted && _predatorsTransformJob.IsCompleted)
        {
            _boidsJobHandles.Dispose();
            _predatorsJobHandles.Dispose();
                
            ReadParams(true);
            
            _xBounds = new float2(_quadTree.Bounds.xMin, _quadTree.Bounds.xMax);
            _yBounds = new float2(_quadTree.Bounds.yMin, _quadTree.Bounds.yMax);

            if (_spawnRequested)
            {
                _boids.Count += 1;

                NativeArray<float2> boidsPositionsNew = new NativeArray<float2>(_boids.Count, Allocator.Persistent);
                NativeArray<float2> boidsVelocitiesNew = new NativeArray<float2>(_boids.Count, Allocator.Persistent);

                NativeArray<float2>.Copy(_boids.Positions, boidsPositionsNew, _boids.Positions.Length);
                NativeArray<float2>.Copy(_boids.Velocities, boidsVelocitiesNew, _boids.Velocities.Length);
                
                _boids.Positions.Dispose();
                _boids.Velocities.Dispose();
                Clean(true);
                
                _boids.Positions = boidsPositionsNew;
                _boids.Velocities = boidsVelocitiesNew;
                Initialize(true);
                
                BoidsManualSpawn(_boids.Count);

                _spawnRequested = false;
            }
            
            _boidsTextureColorJob = TextureColorJob.Begin(_boids.Velocities, 
                _boids.Behavior.MinSpeed,
                _boids.Behavior.MaxSpeed,
                _boids.TextureColor,
                _boids.Intensity);
            
            _quadTree.Build(_boids.Positions);
            
            // This loop is the trickiest part of the project. We loop over entire boids
            // collection and determine how many neighbors it has for everyone.
            // As we are using the "Jobs System" we can use flat arrays only. 
            // We need 3 arrays to pass this kind of information to jobs: 
            // 1) neighborsArray: the flattened neighbors map.
            // 2) neighborsCounts: how many neighbors i-th boid has.
            // 3) neighborsStartIndices: from which index i-th boid has to start reading on neighborsArray.
            int neighborIdx = 0;
            for (int i = 0; i < _boids.Count; i++)
            {
                _neighborsStartIndices[i] = neighborIdx;
                _neighborsCounts[i] = 0;

                NativeList<int> foundNeighbors = new NativeList<int>(Allocator.Temp);
                
                Vector2 halfPos = new Vector2(_boids.Positions[i].x - _boids.MaxPerceptionRadius/2,
                    _boids.Positions[i].y - _boids.MaxPerceptionRadius/2);

                Rect perceptionRect = new Rect(halfPos, new Vector2(_boids.MaxPerceptionRadius, _boids.MaxPerceptionRadius));
                
                _quadTree.QueryRange(perceptionRect, _boids.Positions, foundNeighbors);
                
                for (int j = 0; j < foundNeighbors.Length; j++)
                {
                    int neighborIndex = foundNeighbors[j];
                    if (neighborIndex != i) // Avoid to add itself.
                    {
                        _neighborsArray[neighborIdx] = neighborIndex;
                        neighborIdx++;
                        _neighborsCounts[i]++;
                    }
                }
                foundNeighbors.Dispose();
            }

            // Compute the boids alignment vector...
            _alignmentJob = AlignmentJob.Begin(_boids.Positions,
                _boids.Velocities,
                _neighborsArray,
                _neighborsCounts,
                _neighborsStartIndices,
                _boids.Alignment.Steering,
                _boids.Behavior.MovementAccuracy,
                _boids.Alignment.PerceptionRadius,
                _boids.Alignment.PerceptionAngle,
                _boids.Alignment.MinDistanceWeight,
                _boids.Alignment.MaxDistanceWeight,
                _boids.Alignment.DesiredMagnitude,
                _boids.Alignment.MaxSteeringForce,
                _boids.Alignment.ScaleForce);

            // Compute the boids separation vector...
            _separationJob = SeparationJob.Begin(_boids.Positions,
                _boids.Velocities,
                _neighborsArray,
                _neighborsCounts,
                _neighborsStartIndices,
                _boids.Separation.Steering,
                _boids.Behavior.MovementAccuracy,
                _boids.Separation.PerceptionRadius,
                _boids.Separation.PerceptionAngle,
                _boids.Separation.MinDistanceWeight,
                _boids.Separation.MaxDistanceWeight,
                _boids.Separation.DesiredMagnitude,
                _boids.Separation.MaxSteeringForce,
                _boids.Separation.ScaleForce);
            
            // Compute the boids cohesion vector...
            _cohesionJob = CohesionJob.Begin(_boids.Positions,
                _boids.Velocities,
                _neighborsArray,
                _neighborsCounts,
                _neighborsStartIndices,
                _boids.Cohesion.Steering,
                _boids.Behavior.MovementAccuracy,
                _boids.Cohesion.PerceptionRadius,
                _boids.Cohesion.PerceptionAngle,
                _boids.Cohesion.MinDistanceWeight,
                _boids.Cohesion.MaxDistanceWeight,
                _boids.Cohesion.DesiredMagnitude,
                _boids.Cohesion.MaxSteeringForce,
                _boids.Cohesion.ScaleForce);
            
            // Compute the boids attraction vector...
            _attractionJob = AttractionJob.Begin(_boids.Positions, 
                _boids.Velocities,
                _boids.TargetAttraction.Steering,
                _boids.TargetAttraction.InteractionRadius,
                _boids.TargetAttraction.SpeedFactor,
                _boids.TargetAttraction.MinSteeringForce,
                _boids.TargetAttraction.MaxSteeringForce,
                _boids.TargetAttraction.OrbitRadius,
                _attractionPosition, 
                _attractionRequested);
            
            // Compute the boids repulsion vector...
            _repulsionJob = RepulsionJob.Begin(_boids.Positions, 
                _boids.Velocities,
                _boids.TargetRepulsion.Steering,
                _boids.TargetRepulsion.InteractionRadius,
                _boids.TargetRepulsion.SpeedFactor,
                _boids.TargetRepulsion.MaxSteeringForce,
                _repulsionPosition, 
                _repulsionRequested);
            
            // Compute the boids escape vector...
            _escapeJob = EscapePredatorJob.Begin(_boids.Positions,
                _boids.Velocities,
                _predators.Positions,
                _boids.Escape.Steering,
                _boids.Escape.PerceptionRadius,
                _boids.Escape.SpeedFactor,
                _boids.Escape.MaxSteeringForce);
            
            // Compute the predators wander vector...
            _wanderJob = WanderJob.Begin(_predators.Positions, 
                _predators.Velocities,
                _predators.WanderTheta,
                _predators.Steering,
                _predators.Behavior.MaxSpeed,
                _predators.Behavior.WanderRadius,
                _predators.Behavior.WanderDistance,
                _predators.Behavior.MaxSteeringForce,
                _predators.Behavior.VariationRange,
                (uint)DateTime.Now.Ticks);
        }
        
        // Handle the post process effect...
        _globalVolume.enabled = _usePostProcessingEffects;
        _boidsTextureColorJob.Complete();
        if (_boidsTextureColorJob.IsCompleted)
        {
            for (int i = 0; i < _boids.Renderers.Count; i++)
            {
                SpriteRenderer sr = _boids.Renderers[i];

                if (sr != null)
                {
                    Color newColor = new Color(_boids.TextureColor[i].x, _boids.TextureColor[i].y, 0, 1);
                    _boids.PropertyBlocks[i].SetColor("_BaseColor", newColor);
                    _boids.PropertyBlocks[i].SetColor("_EmissionColor", newColor);
                    _boids.PropertyBlocks[i].SetFloat("_EmissionIntensity", _boids.Intensity[i]);

                    sr.SetPropertyBlock(_boids.PropertyBlocks[i]);
                }
            }
            
            for (int i = 0; i < _predators.Renderers.Count; i++)
            {
                SpriteRenderer sr = _predators.Renderers[i];

                if (sr != null)
                {
                    float t = Mathf.InverseLerp(_predators.Behavior.MinSpeed, _predators.Behavior.MaxSpeed,
                        math.length(_predators.Velocities[i]));

                    float blue = math.lerp(0.5f, 1f, t);
                    
                    Color newColor = new Color(0, 0, blue, 1);
                    _predators.PropertyBlocks[i].SetColor("_BaseColor", newColor);
                    _predators.PropertyBlocks[i].SetColor("_EmissionColor", newColor);
                    _predators.PropertyBlocks[i].SetFloat("_EmissionIntensity", t);

                    sr.SetPropertyBlock(_predators.PropertyBlocks[i]);
                }
            }
        }
    }
    
    void LateUpdate()
    {
        // Wait for all the jobs to get done and then change transforms...
        _boidsJobHandles = new NativeArray<JobHandle>(6, Allocator.TempJob);
        _boidsJobHandles[0] = _alignmentJob;
        _boidsJobHandles[1] = _separationJob;
        _boidsJobHandles[2] = _cohesionJob;
        _boidsJobHandles[3] = _attractionJob;
        _boidsJobHandles[4] = _repulsionJob;
        _boidsJobHandles[5] = _escapeJob;

        JobHandle boidsJobDependencies = JobHandle.CombineDependencies(_boidsJobHandles);
            
        _boidsTransformJob = BoidTransformJob.Begin(_boids.Positions,
            _boids.Velocities,
            _boids.Alignment.Steering,
            _boids.Separation.Steering,
            _boids.Cohesion.Steering,
            _boids.TargetAttraction.Steering,
            _boids.TargetRepulsion.Steering,
            _boids.Escape.Steering,
            _boids.TransformsAccessArray,
            boidsJobDependencies,
            _boids.Behavior.MinSpeed,
            _boids.Behavior.DragFactor,
            _xBounds,
            _yBounds,
            Time.deltaTime);

        if (_repulsionRequested)
        {
            _repulsionRequested = false;

            Vector3 pos = new Vector3(_repulsionPosition.x, _repulsionPosition.y, 0);
            GameObject newCircle = Instantiate(_circlePrefab, pos, quaternion.identity);

            StartCoroutine(ShrinkAndDestroy(newCircle));
        }
        
        _predatorsJobHandles = new NativeArray<JobHandle>(2, Allocator.TempJob);
        _predatorsJobHandles[0] = _escapeJob;
        _predatorsJobHandles[1] = _wanderJob;
        JobHandle predatorsJobDependencies = JobHandle.CombineDependencies(_predatorsJobHandles);
        
        _predatorsTransformJob = PredatorTransformJob.Begin(_predators.Positions,
            _predators.Velocities,
            _predators.Steering,
            _predators.TransformsAccessArray,
            predatorsJobDependencies,
            _predators.Behavior.MinSpeed,
            _predators.Behavior.MaxSpeed,
            _xBounds,
            _yBounds,
            Time.deltaTime);
    }

    IEnumerator ShrinkAndDestroy(GameObject circle)
    {
        Transform circleTransform = circle.transform;
        
        while (circleTransform.localScale.x > 0.05f)
        {
            circleTransform.localScale -= Vector3.one * (_shrinkSpeed * Time.deltaTime);
            yield return null;
        }
        Destroy(circle);
    }
    
    public void Spawn(Vector2 spawnPosition)
    {
        _spawnRequested = true;
        _spawnPosition = spawnPosition;
    }

    public void GoAway(Vector2 repulsionPosition)
    {
        _repulsionRequested = true;
        _repulsionPosition = repulsionPosition;
    }
    
    public void ToggleAttraction(Vector2 attractionPosition)
    {
        _attractionRequested = !_attractionRequested;
        _attractionPosition = attractionPosition;
    }

    public void UpdateAttractionPosition(Vector2 attractionPosition)
    {
        _attractionPosition = attractionPosition;
    }
    
    public void HandleUsePostProcessingEffectsChange(bool value)
    {
        _usePostProcessingEffects = value;
    }

    public void HandleBoidsSpawnChange(float value)
    {
        _boidsSpawnAtStart = (int)value;
    }

    public void HandleBoidsMinSpeedChange(float value)
    {
        _boidsMinSpeed = value;
    }

    public void HandleBoidsMaxSpeedChange(float value)
    {
        _boidsMaxSpeed = value;
    }

    public void HandleBoidsMovementAccuracyChange(float value)
    {
        _movementAccuracy = (int)value;
    }

    public void HandleBoidsDragFactorChange(float value)
    {
        _dragFactor = value;
    }
    
    public void HandleBoidsAlignmentPerceptionRadiusChange(float value)
    {
        _alignmentPerceptionRadius = value;
    }

    public void HandleBoidsAlignmentPerceptionAngleChange(float value)
    {
        _alignmentPerceptionAngle = value;
    }

    public void HandleBoidsAlignmentMinDistanceWeightChange(float value)
    {
        _alignmentMinDistanceWeight = value;
    }

    public void HandleBoidsAlignmentMaxDistanceWeightChange(float value)
    {
        _alignmentMaxDistanceWeight = value;
    }

    public void HandleBoidsAlignmentDesiredMagnitudeChange(float value)
    {
        _alignmentDesiredMagnitude = value;
    }

    public void HandleBoidsAlignmentMaxSteeringForceChange(float value)
    {
        _alignmentMaxSteeringForce = value;
    }

    public void HandleBoidsAlignmentScaleForceChange(float value)
    {
        _alignmentScaleForce = value;
    }
    
    public void HandleBoidsSeparationPerceptionRadiusChange(float value)
    {
        _separationPerceptionRadius = value;
    }

    public void HandleBoidsSeparationPerceptionAngleChange(float value)
    {
        _separationPerceptionAngle = value;
    }

    public void HandleBoidsSeparationMinDistanceWeightChange(float value)
    {
        _separationMinDistanceWeight = value;
    }

    public void HandleBoidsSeparationMaxDistanceWeightChange(float value)
    {
        _separationMaxDistanceWeight = value;
    }

    public void HandleBoidsSeparationDesiredMagnitudeChange(float value)
    {
        _separationDesiredMagnitude = value;
    }

    public void HandleBoidsSeparationMaxSteeringForceChange(float value)
    {
        _separationMaxSteeringForce = value;
    }

    public void HandleBoidsSeparationScaleForceChange(float value)
    {
        _separationScaleForce = value;
    }
    
    public void HandleBoidsCohesionPerceptionRadiusChange(float value)
    {
        _cohesionPerceptionRadius = value;
    }

    public void HandleBoidsCohesionPerceptionAngleChange(float value)
    {
        _cohesionPerceptionAngle = value;
    }

    public void HandleBoidsCohesionMinDistanceWeightChange(float value)
    {
        _cohesionMinDistanceWeight = value;
    }

    public void HandleBoidsCohesionMaxDistanceWeightChange(float value)
    {
        _cohesionMaxDistanceWeight = value;
    }

    public void HandleBoidsCohesionDesiredMagnitudeChange(float value)
    {
        _cohesionDesiredMagnitude = value;
    }

    public void HandleBoidsCohesionMaxSteeringForceChange(float value)
    {
        _cohesionMaxSteeringForce = value;
    }

    public void HandleBoidsCohesionScaleForceChange(float value)
    {
        _cohesionScaleForce = value;
    }
    
    public void HandleBoidsEscapePerceptionRadiusChange(float value)
    {
        _escapePerceptionRadius = value;
    }

    public void HandleBoidsEscapeSpeedFactorChange(float value)
    {
        _escapeSpeedFactor = value;
    }

    public void HandleBoidsEscapeMaxSteeringForceChange(float value)
    {
        _escapeMaxSteeringForce = value;
    }
    
    public void HandleBoidsAttractionInteractionRadiusChange(float value)
    {
        _attractionInteractionRadius = value;
    }

    public void HandleBoidsAttractionSpeedFactorChange(float value)
    {
        _attractionSpeedFactor = value;
    }

    public void HandleBoidsAttractionMinSteeringForceChange(float value)
    {
        _attractionMinSteeringForce = value;
    }

    public void HandleBoidsAttractionMaxSteeringForceChange(float value)
    {
        _attractionMaxSteeringForce = value;
    }

    public void HandleBoidsAttractionOrbitRadiusChange(float value)
    {
        _attractionOrbitRadius = value;
    }
    
    public void HandleBoidsRepulsionInteractionRadiusChange(float value)
    {
        _repulsionInteractionRadius = value;
    }

    public void HandleBoidsRepulsionSpeedFactorChange(float value)
    {
        _repulsionSpeedFactor = value;
    }

    public void HandleBoidsRepulsionMaxSteeringForceChange(float value)
    {
        _repulsionMaxSteeringForce = value;
    }
    
    public void HandlePredatorsSpawnChange(float value)
    {
        _predatorsSpawnAtStart = (int)value;
    }

    public void HandlePredatorsMinSpeedChange(float value)
    {
        _predatorsMinSpeed = value;
    }

    public void HandlePredatorsMaxSpeedChange(float value)
    {
        _predatorsMaxSpeed = value;
    }

    public void HandlePredatorsWanderRadiusChange(float value)
    {
        _predatorsWanderRadius = value;
    }

    public void HandlePredatorsWanderDistanceChange(float value)
    {
        _predatorsWanderDistance = value;
    }
    
    public void HandlePredatorsMaxSteeringForceChange(float value)
    {
        _predatorsMaxSteeringForce = value;
    }
    
    public void HandlePredatorsVariationRangeChange(float value)
    {
        _predatorsVariationRange = value;
    }
    
    void OnDisable()
    {
        _alignmentJob.Complete();
        _separationJob.Complete();
        _cohesionJob.Complete();
        _escapeJob.Complete();
        _wanderJob.Complete();
        _boidsTransformJob.Complete();
        _boidsTextureColorJob.Complete();
        _predatorsTransformJob.Complete();

        if (_alignmentJob.IsCompleted && _separationJob.IsCompleted && _cohesionJob.IsCompleted
            && _attractionJob.IsCompleted && _escapeJob.IsCompleted && _wanderJob.IsCompleted &&
            _boidsTransformJob.IsCompleted && _boidsTextureColorJob.IsCompleted && _predatorsTransformJob.IsCompleted)
        {
            _boidsJobHandles.Dispose();
            _predatorsJobHandles.Dispose();

            Clean(false);
        }
    }
}
