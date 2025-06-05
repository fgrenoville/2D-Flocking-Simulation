// Copyright (c) [2024] [Federico Grenoville]

using System;
using System.IO;
using Config.Flocking;
using UnityEngine;
using UnityEngine.Events;

namespace Config
{
    /// <summary>
    /// ConfigLoader is responsible for loading and parsing the JSON-based simulation settings,
    /// broadcasting them through UnityEvents to initialize runtime UI components or logic.
    /// If the configuration file is missing or invalid, it falls back to default settings.
    /// </summary>
    public class ConfigLoader : MonoBehaviour
    {
        public UnityEvent<bool> OnUsePostProcessingEffectsInit;
        
        public UnityEvent<float> OnBoidsSpawnAtStartInit;
        public UnityEvent<float> OnBoidsMinSpeedInit;
        public UnityEvent<float> OnBoidsMaxSpeedInit;
        public UnityEvent<float> OnBoidsMovementAccuracyInit;
        public UnityEvent<float> OnBoidsDragFactorInit;
        
        public UnityEvent<float> OnBoidsAlignmentPerceptionRadiusInit;
        public UnityEvent<float> OnBoidsAlignmentPerceptionAngleInit;
        public UnityEvent<float> OnBoidsAlignmentMinDistanceWeightInit;
        public UnityEvent<float> OnBoidsAlignmentMaxDistanceWeightInit;
        public UnityEvent<float> OnBoidsAlignmentDesiredMagnitudeInit;
        public UnityEvent<float> OnBoidsAlignmentMaxSteeringForceInit;
        public UnityEvent<float> OnBoidsAlignmentScaleForceInit;
        
        public UnityEvent<float> OnBoidsSeparationPerceptionRadiusInit;
        public UnityEvent<float> OnBoidsSeparationPerceptionAngleInit;
        public UnityEvent<float> OnBoidsSeparationMinDistanceWeightInit;
        public UnityEvent<float> OnBoidsSeparationMaxDistanceWeightInit;
        public UnityEvent<float> OnBoidsSeparationDesiredMagnitudeInit;
        public UnityEvent<float> OnBoidsSeparationMaxSteeringForceInit;
        public UnityEvent<float> OnBoidsSeparationScaleForceInit;

        public UnityEvent<float> OnBoidsCohesionPerceptionRadiusInit;
        public UnityEvent<float> OnBoidsCohesionPerceptionAngleInit;
        public UnityEvent<float> OnBoidsCohesionMinDistanceWeightInit;
        public UnityEvent<float> OnBoidsCohesionMaxDistanceWeightInit;
        public UnityEvent<float> OnBoidsCohesionDesiredMagnitudeInit;
        public UnityEvent<float> OnBoidsCohesionMaxSteeringForceInit;
        public UnityEvent<float> OnBoidsCohesionScaleForceInit;

        public UnityEvent<float> OnBoidsEscapePerceptionRadiusInit;
        public UnityEvent<float> OnBoidsEscapeSpeedFactorInit;
        public UnityEvent<float> OnBoidsEscapeMaxSteeringForceInit;
        
        public UnityEvent<float> OnBoidsAttractionInteractionRadiusInit;
        public UnityEvent<float> OnBoidsAttractionSpeedFactorInit;
        public UnityEvent<float> OnBoidsAttractionMinSteeringForceInit;        
        public UnityEvent<float> OnBoidsAttractionMaxSteeringForceInit;
        public UnityEvent<float> OnBoidsAttractionOrbitRadiusInit;

        public UnityEvent<float> OnBoidsRepulsionInteractionRadiusInit;
        public UnityEvent<float> OnBoidsRepulsionSpeedFactorInit;
        public UnityEvent<float> OnBoidsRepulsionMaxSteeringForceInit;
        
        public UnityEvent<float> OnPredatorsSpawnAtStartInit;
        public UnityEvent<float> OnPredatorsMinSpeedInit;
        public UnityEvent<float> OnPredatorsMaxSpeedInit;
        public UnityEvent<float> OnPredatorsWanderRadiusInit;
        public UnityEvent<float> OnPredatorsWanderDistanceInit;
        public UnityEvent<float> OnPredatorsMaxSteeringForceInit;
        public UnityEvent<float> OnPredatorsVariationRangeInit;
        
        public FlockingSettings Settings { get; private set; }
        public bool SettingsSuccessfullyRead { get; private set; }

        private const string defaultSettings =
            "{\"General\":{\"UsePostProcessingEffects\":true},\"Boids\":{\"SpawnAtStart\":{\"value\":1200,\"min\":10,\"max\":2000},\"Behavior\":{\"MinSpeed\":{\"value\":0,\"min\":0,\"max\":20},\"MaxSpeed\":{\"value\":5,\"min\":1,\"max\":30},\"MovementAccuracy\":{\"value\":32,\"min\":10,\"max\":500},\"DragFactor\":{\"value\":0.0001,\"min\":0.0001,\"max\":0.01}},\"Alignment\":{\"PerceptionRadius\":{\"value\":2.1,\"min\":0,\"max\":20},\"PerceptionAngle\":{\"value\":65,\"min\":0,\"max\":360},\"MinDistanceWeight\":{\"value\":1,\"min\":0.5,\"max\":20},\"MaxDistanceWeight\":{\"value\":10,\"min\":0.5,\"max\":20},\"DesiredMagnitude\":{\"value\":4,\"min\":1,\"max\":20},\"MaxSteeringForce\":{\"value\":0.025,\"min\":0,\"max\":0.5},\"ScaleForce\":{\"value\":1,\"min\":0,\"max\":5}},\"Separation\":{\"PerceptionRadius\":{\"value\":0.45,\"min\":0,\"max\":20},\"PerceptionAngle\":{\"value\":120,\"min\":0,\"max\":360},\"MinDistanceWeight\":{\"value\":16,\"min\":0.5,\"max\":20},\"MaxDistanceWeight\":{\"value\":1,\"min\":0.5,\"max\":20},\"DesiredMagnitude\":{\"value\":5,\"min\":1,\"max\":20},\"MaxSteeringForce\":{\"value\":0.009,\"min\":0,\"max\":0.5},\"ScaleForce\":{\"value\":1,\"min\":0,\"max\":5}},\"Cohesion\":{\"PerceptionRadius\":{\"value\":0.8,\"min\":0,\"max\":20},\"PerceptionAngle\":{\"value\":240,\"min\":0,\"max\":360},\"MinDistanceWeight\":{\"value\":1,\"min\":0.5,\"max\":20},\"MaxDistanceWeight\":{\"value\":8,\"min\":0.5,\"max\":20},\"DesiredMagnitude\":{\"value\":2,\"min\":1,\"max\":20},\"MaxSteeringForce\":{\"value\":0.002,\"min\":0,\"max\":0.5},\"ScaleForce\":{\"value\":1,\"min\":0,\"max\":5}},\"Escape\":{\"PerceptionRadius\":{\"value\":4.3,\"min\":0,\"max\":20},\"SpeedFactor\":{\"value\":5.2,\"min\":0.1,\"max\":20},\"MaxSteeringForce\":{\"value\":0.18,\"min\":0,\"max\":2}},\"Attraction\":{\"InteractionRadius\":{\"value\":11,\"min\":0,\"max\":20},\"SpeedFactor\":{\"value\":15.5,\"min\":0.1,\"max\":20},\"MaxSteeringForce\":{\"value\":0.14,\"min\":0,\"max\":2}},\"Repulsion\":{\"InteractionRadius\":{\"value\":10.5,\"min\":0,\"max\":20},\"SpeedFactor\":{\"value\":160,\"min\":0,\"max\":200},\"MaxSteeringForce\":{\"value\":11.6,\"min\":0.1,\"max\":30}}},\"Predators\":{\"SpawnAtStart\":{\"value\":2,\"min\":1,\"max\":50},\"Behavior\":{\"MinSpeed\":{\"value\":1.1,\"min\":0,\"max\":20},\"MaxSpeed\":{\"value\":4.9,\"min\":1,\"max\":30},\"WanderRadius\":{\"value\":7,\"min\":0,\"max\":30},\"WanderDistance\":{\"value\":13,\"min\":0,\"max\":60},\"MaxSteeringForce\":{\"value\":0.27,\"min\":0,\"max\":2},\"VariationRange\":{\"value\":10,\"min\":0,\"max\":20}}}}";
        
        void Start()
        {
            try
            {
                TextAsset jsonFile = Resources.Load<TextAsset>("flocking_starter");

                if (jsonFile == null)
                {
                    throw new FileNotFoundException("Configuration file not found in \"Resources\" folder!");
                }
                
                Settings = JsonUtility.FromJson<FlockingSettings>(jsonFile.text);

                if (Settings == null)
                {
                    throw new Exception("Deserialization failed: maybe the file format?!");
                }
                
                SettingsSuccessfullyRead = true;

                OnUsePostProcessingEffectsInit?.Invoke(Settings.General.UsePostProcessingEffects);
                
                OnBoidsSpawnAtStartInit?.Invoke(Settings.Boids.SpawnAtStart.value);
                
                OnBoidsMinSpeedInit?.Invoke(Settings.Boids.Behavior.MinSpeed.value);
                OnBoidsMaxSpeedInit?.Invoke(Settings.Boids.Behavior.MaxSpeed.value);
                OnBoidsMovementAccuracyInit?.Invoke(Settings.Boids.Behavior.MovementAccuracy.value);
                OnBoidsDragFactorInit?.Invoke(Settings.Boids.Behavior.DragFactor.value);
        
                OnBoidsAlignmentPerceptionRadiusInit?.Invoke(Settings.Boids.Alignment.PerceptionRadius.value);
                OnBoidsAlignmentPerceptionAngleInit?.Invoke(Settings.Boids.Alignment.PerceptionAngle.value);
                OnBoidsAlignmentMinDistanceWeightInit?.Invoke(Settings.Boids.Alignment.MinDistanceWeight.value);
                OnBoidsAlignmentMaxDistanceWeightInit?.Invoke(Settings.Boids.Alignment.MaxDistanceWeight.value);
                OnBoidsAlignmentDesiredMagnitudeInit?.Invoke(Settings.Boids.Alignment.DesiredMagnitude.value);
                OnBoidsAlignmentMaxSteeringForceInit?.Invoke(Settings.Boids.Alignment.MaxSteeringForce.value);
                OnBoidsAlignmentScaleForceInit?.Invoke(Settings.Boids.Alignment.ScaleForce.value);
        
                OnBoidsSeparationPerceptionRadiusInit?.Invoke(Settings.Boids.Separation.PerceptionRadius.value);
                OnBoidsSeparationPerceptionAngleInit?.Invoke(Settings.Boids.Separation.PerceptionAngle.value);
                OnBoidsSeparationMinDistanceWeightInit?.Invoke(Settings.Boids.Separation.MinDistanceWeight.value);
                OnBoidsSeparationMaxDistanceWeightInit?.Invoke(Settings.Boids.Separation.MaxDistanceWeight.value);
                OnBoidsSeparationDesiredMagnitudeInit?.Invoke(Settings.Boids.Separation.DesiredMagnitude.value);
                OnBoidsSeparationMaxSteeringForceInit?.Invoke(Settings.Boids.Separation.MaxSteeringForce.value);
                OnBoidsSeparationScaleForceInit?.Invoke(Settings.Boids.Separation.ScaleForce.value);

                OnBoidsCohesionPerceptionRadiusInit?.Invoke(Settings.Boids.Cohesion.PerceptionRadius.value);
                OnBoidsCohesionPerceptionAngleInit?.Invoke(Settings.Boids.Cohesion.PerceptionAngle.value);
                OnBoidsCohesionMinDistanceWeightInit?.Invoke(Settings.Boids.Cohesion.MinDistanceWeight.value);
                OnBoidsCohesionMaxDistanceWeightInit?.Invoke(Settings.Boids.Cohesion.MaxDistanceWeight.value);
                OnBoidsCohesionDesiredMagnitudeInit?.Invoke(Settings.Boids.Cohesion.DesiredMagnitude.value);
                OnBoidsCohesionMaxSteeringForceInit?.Invoke(Settings.Boids.Cohesion.MaxSteeringForce.value);
                OnBoidsCohesionScaleForceInit?.Invoke(Settings.Boids.Cohesion.ScaleForce.value);

                OnBoidsEscapePerceptionRadiusInit?.Invoke(Settings.Boids.Escape.PerceptionRadius.value);
                OnBoidsEscapeSpeedFactorInit?.Invoke(Settings.Boids.Escape.SpeedFactor.value);
                OnBoidsEscapeMaxSteeringForceInit?.Invoke(Settings.Boids.Escape.MaxSteeringForce.value);
        
                OnBoidsAttractionInteractionRadiusInit?.Invoke(Settings.Boids.Attraction.InteractionRadius.value);
                OnBoidsAttractionSpeedFactorInit?.Invoke(Settings.Boids.Attraction.SpeedFactor.value);
                OnBoidsAttractionMinSteeringForceInit?.Invoke(Settings.Boids.Attraction.MinSteeringForce.value);
                OnBoidsAttractionMaxSteeringForceInit?.Invoke(Settings.Boids.Attraction.MaxSteeringForce.value);
                OnBoidsAttractionOrbitRadiusInit?.Invoke(Settings.Boids.Attraction.OrbitRadius.value);

                OnBoidsRepulsionInteractionRadiusInit?.Invoke(Settings.Boids.Repulsion.InteractionRadius.value);
                OnBoidsRepulsionSpeedFactorInit?.Invoke(Settings.Boids.Repulsion.SpeedFactor.value);
                OnBoidsRepulsionMaxSteeringForceInit?.Invoke(Settings.Boids.Repulsion.MaxSteeringForce.value);
        
                OnPredatorsSpawnAtStartInit?.Invoke(Settings.Predators.SpawnAtStart.value);
                OnPredatorsMinSpeedInit?.Invoke(Settings.Predators.Behavior.MinSpeed.value);
                OnPredatorsMaxSpeedInit?.Invoke(Settings.Predators.Behavior.MaxSpeed.value);
                OnPredatorsWanderRadiusInit?.Invoke(Settings.Predators.Behavior.WanderRadius.value);
                OnPredatorsWanderDistanceInit?.Invoke(Settings.Predators.Behavior.WanderDistance.value);
                OnPredatorsMaxSteeringForceInit?.Invoke(Settings.Predators.Behavior.MaxSteeringForce.value);
                OnPredatorsVariationRangeInit?.Invoke(Settings.Predators.Behavior.VariationRange.value);
            }
            catch (FileNotFoundException e)
            {
                Debug.LogError(e.Message);
                SettingsSuccessfullyRead = false;

                LoadDefaultConfig();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                SettingsSuccessfullyRead = false;
                
                LoadDefaultConfig();
            }
        }

        private void LoadDefaultConfig()
        {
            try
            {
                Settings = JsonUtility.FromJson<FlockingSettings>(defaultSettings);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                SettingsSuccessfullyRead = false;
            }
        }
    }
}
