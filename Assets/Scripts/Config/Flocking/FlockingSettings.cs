// Copyright (c) [2024] [Federico Grenoville]

namespace Config.Flocking
{
    [System.Serializable]
    public partial class FlockingSettings
    {
        public GeneralSettings General;
        public BoidsSettings Boids;
        public PredatorsSettings Predators;
    }
}
