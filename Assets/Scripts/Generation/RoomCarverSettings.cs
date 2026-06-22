using System;

namespace ToDonegeon.Generation
{
    public sealed class RoomCarverSettings
    {
        public const int DefaultEdgePadding = 1;
        public const int DefaultMinRoomSize = 4;
        public const float DefaultMinFillRatio = 0.6f;

        // wall cells left between a room and its leaf's edges, so neighbouring rooms stay
        // separated by wall and are later joined by corridors rather than merging.
        public int EdgePadding { get; }

        public int MinRoomSize { get; }

        // keeps rooms from shrinking to specks while still allowing size variety
        public float MinFillRatio { get; }

        public RoomCarverSettings(
            int edgePadding = DefaultEdgePadding,
            int minRoomSize = DefaultMinRoomSize,
            float minFillRatio = DefaultMinFillRatio)
        {
            if (edgePadding < 0)
                throw new ArgumentOutOfRangeException(nameof(edgePadding), "Must be non-negative.");
            if (minRoomSize < 1)
                throw new ArgumentOutOfRangeException(nameof(minRoomSize), "Must be at least 1.");
            if (minFillRatio <= 0f || minFillRatio > 1f)
                throw new ArgumentOutOfRangeException(nameof(minFillRatio), "Must lie in the interval (0, 1].");

            EdgePadding = edgePadding;
            MinRoomSize = minRoomSize;
            MinFillRatio = minFillRatio;
        }
    }
}
