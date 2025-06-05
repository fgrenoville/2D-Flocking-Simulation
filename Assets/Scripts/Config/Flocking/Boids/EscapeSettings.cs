// Copyright (c) [2024] [Federico Grenoville]


namespace Config.Flocking.Boids
{
    [System.Serializable]
    public partial class EscapeSettings
    {
        public RangeFloat PerceptionRadius;
        public RangeFloat SpeedFactor;
        public RangeFloat MaxSteeringForce;
    }
}

