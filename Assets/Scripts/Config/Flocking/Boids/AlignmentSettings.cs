// Copyright (c) [2024] [Federico Grenoville]

namespace Config.Flocking.Boids
{
    [System.Serializable]
    public partial class AlignmentSettings
    {
        public RangeFloat PerceptionRadius;
        public RangeFloat PerceptionAngle;
        public RangeFloat MinDistanceWeight;
        public RangeFloat MaxDistanceWeight;
        public RangeFloat DesiredMagnitude;
        public RangeFloat MaxSteeringForce;
        public RangeFloat ScaleForce;
    }
}