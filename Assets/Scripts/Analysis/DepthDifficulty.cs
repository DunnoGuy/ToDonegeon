namespace ToDonegeon.Analysis
{
    // Aggregate threat of many dungeons generated at one descent depth.
    public readonly struct DepthDifficulty
    {
        public int Depth { get; }
        public int Width { get; }
        public int Height { get; }
        public double MeanThreat { get; }
        public long MinThreat { get; }
        public long MaxThreat { get; }
        public double ThreatStdDev { get; }
        public double MeanEnemies { get; }
        public float ValidRate { get; }

        // spread of threat relative to its mean
        public double ThreatSpread => MeanThreat > 0 ? ThreatStdDev / MeanThreat : 0;

        public DepthDifficulty(int depth, int width, int height, double meanThreat, long minThreat,
            long maxThreat, double threatStdDev, double meanEnemies, float validRate)
        {
            Depth = depth;
            Width = width;
            Height = height;
            MeanThreat = meanThreat;
            MinThreat = minThreat;
            MaxThreat = maxThreat;
            ThreatStdDev = threatStdDev;
            MeanEnemies = meanEnemies;
            ValidRate = validRate;
        }
    }
}
