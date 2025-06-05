// Copyright (c) [2024] [Federico Grenoville]

using Config.Flocking.Predators;

namespace Config.Flocking
{
    [System.Serializable]
    public partial class PredatorsSettings
    {
        public RangeInt SpawnAtStart;
        public BehaviorSettings Behavior;
    }
}