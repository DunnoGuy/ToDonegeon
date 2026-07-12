using System;

namespace ToDonegeon.Generation
{
    // tuning for the extra loop corridors added on top of the spanning backbone
    public sealed class CorridorSettings
    {
        public const float DefaultExtraLoopChance = 0f;
        public const int DefaultMaxExtraLoops = 0;

        // per room, the chance of trying to add one shortcut to a nearby room
        public float ExtraLoopChance { get; }

        // upper bound on how many extra corridors are added
        public int MaxExtraLoops { get; }

        public CorridorSettings(
            float extraLoopChance = DefaultExtraLoopChance,
            int maxExtraLoops = DefaultMaxExtraLoops)
        {
            if (extraLoopChance < 0f || extraLoopChance > 1f)
                throw new ArgumentOutOfRangeException(nameof(extraLoopChance), "Must lie in [0, 1]");
            if (maxExtraLoops < 0)
                throw new ArgumentOutOfRangeException(nameof(maxExtraLoops), "Must be non-negative.");

            ExtraLoopChance = extraLoopChance;
            MaxExtraLoops = maxExtraLoops;
        }
    }
}
