// Copyright (c) [2024] [Federico Grenoville]

namespace Config.Flocking.Boids
{
    [System.Serializable]
    public partial class BehaviorSettings
    {
        public RangeFloat MinSpeed;
        public RangeFloat MaxSpeed;
        public RangeInt MovementAccuracy;
        public RangeFloat DragFactor;
    }
}
