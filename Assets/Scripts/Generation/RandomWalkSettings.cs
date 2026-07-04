using System;

namespace ToDonegeon.Generation
{
    // tuning for the random-walk shaper
    public sealed class RandomWalkSettings
    {
        public const float DefaultFillFraction = 0.45f;
        public const int DefaultMaxStepsMultiplier = 20;

        // how much of the region to turn to floor, as a fraction of its area
        public float FillFraction { get; }

        // Caps the walk at area times this
        public int MaxStepsMultiplier { get; }

        public RandomWalkSettings(
            float fillFraction = DefaultFillFraction,
            int maxStepsMultiplier = DefaultMaxStepsMultiplier)
        {
            if (fillFraction <= 0f || fillFraction > 1f)
                throw new ArgumentOutOfRangeException(nameof(fillFraction), "Must lie in (0, 1].");
            if (maxStepsMultiplier < 1)
                throw new ArgumentOutOfRangeException(nameof(maxStepsMultiplier), "Must be at least 1.");

            FillFraction = fillFraction;
            MaxStepsMultiplier = maxStepsMultiplier;
        }
    }
}
