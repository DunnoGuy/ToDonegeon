using System;

namespace ToDonegeon.Generation
{
    // Tuning for the cellular-automata cave shaper
    public sealed class CaveSettings
    {
        // chance a cell starts as wall
        public const float DefaultInitialWallProbability = 0.50f;
        public const int DefaultSmoothingIterations = 5;

        // In the 8-cell neighbourhood, this many wall neighbours turn a cell to wall.
        // Cells outside the region count as wall, so the cave rounds off inside its region.
        public const int DefaultWallThreshold = 5;
        public const bool DefaultKeepOnlyLargestComponent = true;

        public float InitialWallProbability { get; }
        public int SmoothingIterations { get; }
        public int WallThreshold { get; }

        // The flood-fill repair step. Only keeps the largest connected floor blob and fill
        // stray pockets back to wall, so the chamber is reachable as a whole.
        public bool KeepOnlyLargestComponent { get; }

        private const int NeighbourCount = 8;

        public CaveSettings(
            float initialWallProbability = DefaultInitialWallProbability,
            int smoothingIterations = DefaultSmoothingIterations,
            int wallThreshold = DefaultWallThreshold,
            bool keepOnlyLargestComponent = DefaultKeepOnlyLargestComponent)
        {
            if (initialWallProbability < 0f || initialWallProbability > 1f)
                throw new ArgumentOutOfRangeException(nameof(initialWallProbability), "Must lie in [0, 1].");
            if (smoothingIterations < 0)
                throw new ArgumentOutOfRangeException(nameof(smoothingIterations), "Must be non-negative.");
            if (wallThreshold < 1 || wallThreshold > NeighbourCount)
                throw new ArgumentOutOfRangeException(nameof(wallThreshold), "Must lie in [1, 8].");

            InitialWallProbability = initialWallProbability;
            SmoothingIterations = smoothingIterations;
            WallThreshold = wallThreshold;
            KeepOnlyLargestComponent = keepOnlyLargestComponent;
        }
    }
}
