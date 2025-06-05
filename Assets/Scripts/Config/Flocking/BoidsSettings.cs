// Copyright (c) [2024] [Federico Grenoville]

using Config.Flocking.Boids;

namespace Config.Flocking
{
    [System.Serializable]
    public partial class BoidsSettings
    {
        public RangeInt SpawnAtStart;
        public BehaviorSettings Behavior;
        public AlignmentSettings Alignment;
        public SeparationSettings Separation;
        public CohesionSettings Cohesion;
        public EscapeSettings Escape;
        public AttractionSettings Attraction;
        public RepulsionSettings Repulsion;
    }
}