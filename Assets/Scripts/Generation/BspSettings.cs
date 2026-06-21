using System;

namespace ToDonegeon.Generation
{
    public sealed class BspSettings
    {
        public const int DefaultMinLeafSize = 8;
        public const int DefaultMaxLeafSize = 22;
        public const int DefaultMaxDepth = 6;
        public const float DefaultMinSplitRatio = 0.40f;
        public const float DefaultMaxSplitRatio = 0.60f;
        public const float DefaultMaxAspectRatio = 1.5f;

        public int MinLeafSize { get; }
        public int MaxLeafSize { get; }
        public int MaxDepth { get; }

        public float MinSplitRatio { get; }
        public float MaxSplitRatio { get; }

        // when a region is more than this many times longer on one axis than the other,
        // it is cut across the long axis to keep leaf shapes within a bounded aspect ratio.
        public float MaxAspectRatio { get; }

        public int MinSplittableSize => MinLeafSize * 2;

        public BspSettings(
            int minLeafSize = DefaultMinLeafSize,
            int maxLeafSize = DefaultMaxLeafSize,
            int maxDepth = DefaultMaxDepth,
            float minSplitRatio = DefaultMinSplitRatio,
            float maxSplitRatio = DefaultMaxSplitRatio,
            float maxAspectRatio = DefaultMaxAspectRatio)
        {
            if (minLeafSize < 1)
                throw new ArgumentOutOfRangeException(nameof(minLeafSize), "Must be at least 1.");
            if (maxLeafSize < minLeafSize * 2)
                throw new ArgumentOutOfRangeException(nameof(maxLeafSize),
                    "Must be at least twice MinLeafSize so an oversized region can always be split.");
            if (maxDepth < 0)
                throw new ArgumentOutOfRangeException(nameof(maxDepth), "Must be non-negative.");
            if (minSplitRatio <= 0f || minSplitRatio >= 1f)
                throw new ArgumentOutOfRangeException(nameof(minSplitRatio), "Must lie in the open interval (0, 1).");
            if (maxSplitRatio <= 0f || maxSplitRatio >= 1f)
                throw new ArgumentOutOfRangeException(nameof(maxSplitRatio), "Must lie in the open interval (0, 1).");
            if (minSplitRatio > maxSplitRatio)
                throw new ArgumentException("MinSplitRatio must not exceed MaxSplitRatio.", nameof(minSplitRatio));
            if (maxAspectRatio < 1f)
                throw new ArgumentOutOfRangeException(nameof(maxAspectRatio), "Must be at least 1.");

            MinLeafSize = minLeafSize;
            MaxLeafSize = maxLeafSize;
            MaxDepth = maxDepth;
            MinSplitRatio = minSplitRatio;
            MaxSplitRatio = maxSplitRatio;
            MaxAspectRatio = maxAspectRatio;
        }
    }
}
