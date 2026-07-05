using System;

namespace ToDonegeon.Generation
{
    public sealed class Room
    {
        private readonly Coord? _centre;

        public GridRect Bounds { get; }

        public BspNode Leaf { get; }

        // an explicit floor cell for organic chambers, whose middle can land on wall,
        // otherwise the geometric centre of the bounds
        public Coord Center => _centre ?? Bounds.Center;

        public Room(GridRect bounds, BspNode leaf, Coord? centre = null)
        {
            Bounds = bounds;
            Leaf = leaf ?? throw new ArgumentNullException(nameof(leaf));
            _centre = centre;
        }
    }
}
