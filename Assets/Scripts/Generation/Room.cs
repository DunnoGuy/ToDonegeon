using System;

namespace ToDonegeon.Generation
{
    public sealed class Room
    {
        public GridRect Bounds { get; }

        public BspNode Leaf { get; }

        public Coord Center => Bounds.Center;

        public Room(GridRect bounds, BspNode leaf)
        {
            Bounds = bounds;
            Leaf = leaf ?? throw new ArgumentNullException(nameof(leaf));
        }
    }
}
