using System;
using System.Collections.Generic;

namespace ToDonegeon.Generation
{
    public sealed class RoomCarverSettings
    {
        public const int DefaultEdgePadding = 1;
        public const int DefaultMinRoomSize = 4;
        public const float DefaultMinFillRatio = 0.6f;
        public const ChamberShape DefaultShape = ChamberShape.Rectangle;
        public const int DefaultShapeZoneDepth = 2;

        // wall cells left between a room and its leaf's edges, so neighbouring rooms stay
        // separated by wall and are later joined by corridors rather than merging.
        public int EdgePadding { get; }

        public int MinRoomSize { get; }

        // keeps rooms from shrinking to specks while still allowing size variety
        public float MinFillRatio { get; }

        public ChamberShape Shape { get; }

        // used when Shape is Cave
        public CaveSettings CaveSettings { get; }

        // used when Shape is RandomWalk
        public RandomWalkSettings WalkSettings { get; }

        // When set, each BSP subtree down to ShapeZoneDepth draws a shape from this list and all
        // its leaves share it.
        public IReadOnlyList<ChamberShape> ShapeMix { get; }

        // how deep to descend the tree before fixing a zone's shape
        public int ShapeZoneDepth { get; }

        public RoomCarverSettings(
            int edgePadding = DefaultEdgePadding,
            int minRoomSize = DefaultMinRoomSize,
            float minFillRatio = DefaultMinFillRatio,
            ChamberShape shape = DefaultShape,
            CaveSettings caveSettings = null,
            RandomWalkSettings walkSettings = null,
            IReadOnlyList<ChamberShape> shapeMix = null,
            int shapeZoneDepth = DefaultShapeZoneDepth)
        {
            if (edgePadding < 0)
                throw new ArgumentOutOfRangeException(nameof(edgePadding), "Must be non-negative.");
            if (minRoomSize < 1)
                throw new ArgumentOutOfRangeException(nameof(minRoomSize), "Must be at least 1.");
            if (minFillRatio <= 0f || minFillRatio > 1f)
                throw new ArgumentOutOfRangeException(nameof(minFillRatio), "Must lie in the interval (0, 1].");
            if (shapeMix != null && shapeMix.Count == 0)
                throw new ArgumentException("Shape mix must not be empty when supplied.", nameof(shapeMix));
            if (shapeZoneDepth < 0)
                throw new ArgumentOutOfRangeException(nameof(shapeZoneDepth), "Must be non-negative.");

            EdgePadding = edgePadding;
            MinRoomSize = minRoomSize;
            MinFillRatio = minFillRatio;
            Shape = shape;
            CaveSettings = caveSettings ?? new CaveSettings();
            WalkSettings = walkSettings ?? new RandomWalkSettings();
            ShapeMix = shapeMix;
            ShapeZoneDepth = shapeZoneDepth;
        }
    }
}
