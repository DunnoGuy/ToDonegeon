using System;

namespace ToDonegeon.Generation
{
    // Turns a difficulty tier into the layout of one floor: deeper floors grow larger, split into
    // more rooms and pack more enemies per room, each clamped so late floors stay quick to build and
    // fair to play.
    public static class DifficultyPlan
    {
        // tier-1 baseline (the default floor)
        private const int BaseWidth = 48;
        private const int BaseHeight = 32;
        private const int BaseDepth = BspSettings.DefaultMaxDepth;
        private const int BaseEnemiesPerRoom = 2;

        // how a floor grows each tier, and the ceilings that keep deep floors in check
        private const int WidthPerTier = 6;
        private const int HeightPerTier = 4;
        private const int TiersPerDepthStep = 2;
        private const int TiersPerDensityStep = 2;
        private const int MaxWidth = 96;
        private const int MaxHeight = 64;
        private const int MaxDepth = 9;
        private const int MaxEnemiesPerRoom = 5;

        public static FloorPlan ForTier(int tier)
        {
            if (tier < 1) throw new ArgumentOutOfRangeException(nameof(tier), "Tier must be at least 1.");

            int steps = tier - 1;
            return new FloorPlan(
                tier,
                Math.Min(BaseWidth + steps * WidthPerTier, MaxWidth),
                Math.Min(BaseHeight + steps * HeightPerTier, MaxHeight),
                Math.Min(BaseDepth + steps / TiersPerDepthStep, MaxDepth),
                Math.Min(BaseEnemiesPerRoom + steps / TiersPerDensityStep, MaxEnemiesPerRoom));
        }
    }
}
