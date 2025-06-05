// Copyright (c) [2024] [Federico Grenoville]

namespace Config.Flocking.Boids
{
    [System.Serializable]
    public partial class AttractionSettings
    {
        public RangeFloat InteractionRadius;
        public RangeFloat SpeedFactor;
        public RangeFloat MinSteeringForce;        
        public RangeFloat MaxSteeringForce;
        public RangeFloat OrbitRadius;
    }
}