// Copyright (c) [2024] [Federico Grenoville]

namespace Config.Flocking.Predators
{
    [System.Serializable]
    public partial class BehaviorSettings
    {
        public RangeFloat MinSpeed;
        public RangeFloat MaxSpeed;
        public RangeFloat WanderRadius;
        public RangeFloat WanderDistance;
        public RangeFloat MaxSteeringForce;
        public RangeFloat VariationRange;
    }
}