// Copyright (c) [2024] [Federico Grenoville]

namespace Config.Flocking.Boids
{
    [System.Serializable]
    public partial class RepulsionSettings
    {
        public RangeFloat InteractionRadius;
        public RangeFloat SpeedFactor;
        public RangeFloat MaxSteeringForce;
    }
}