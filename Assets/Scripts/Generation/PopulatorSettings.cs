using System;

namespace ToDonegeon.Generation
{
    public sealed class PopulatorSettings
    {
        public const int DefaultMaxChests = 5;
        public const int DefaultMaxEnemiesPerRoom = 2;
        public const int DefaultDifficultyTier = 1;

        public int MaxChests { get; }

        public int MaxEnemiesPerRoom { get; }

        // Scales enemy counts
        public int DifficultyTier { get; }

        public PopulatorSettings(
            int maxChests = DefaultMaxChests,
            int maxEnemiesPerRoom = DefaultMaxEnemiesPerRoom,
            int difficultyTier = DefaultDifficultyTier)
        {
            if (maxChests < 0)
                throw new ArgumentOutOfRangeException(nameof(maxChests), "Must be non-negative.");
            if (maxEnemiesPerRoom < 0)
                throw new ArgumentOutOfRangeException(nameof(maxEnemiesPerRoom), "Must be non-negative.");
            if (difficultyTier < 1)
                throw new ArgumentOutOfRangeException(nameof(difficultyTier), "Must be at least 1.");

            MaxChests = maxChests;
            MaxEnemiesPerRoom = maxEnemiesPerRoom;
            DifficultyTier = difficultyTier;
        }

        // The effective per-room enemy cap after applying the difficulty tier.
        public int EnemyCapPerRoom => MaxEnemiesPerRoom * DifficultyTier;
    }
}
