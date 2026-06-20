using System;

namespace ToDonegeon.Generation
{
    // An axis-aligned rectangle on the integer grid. Used for the regions produced by the
    // BSP partition and for the rooms carved inside them, so it is the shared currency
    // between the partition and carving stages.
    public readonly struct GridRect : IEquatable<GridRect>
    {
        // X of the minimum (left) edge.
        public int X { get; }

        // Y of the minimum (bottom) edge.
        public int Y { get; }

        public int Width { get; }
        public int Height { get; }

        public GridRect(int x, int y, int width, int height)
        {
            if (width < 0) throw new ArgumentOutOfRangeException(nameof(width), "Width must be non-negative.");
            if (height < 0) throw new ArgumentOutOfRangeException(nameof(height), "Height must be non-negative.");

            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public int MaxX => X + Width;
        public int MaxY => Y + Height;
        public int Area => Width * Height;

        // Integer centre cell. On even spans it is biased toward the minimum corner
        // (floor division), which keeps the centre inside the rectangle for any size >= 1.
        public Coord Center => new Coord(X + Width / 2, Y + Height / 2);

        // True when the coordinate lies inside the half-open rectangle.
        public bool Contains(Coord c)
            => c.X >= X && c.X < MaxX && c.Y >= Y && c.Y < MaxY;

        // True when the two rectangles share at least one cell.
        public bool Overlaps(GridRect other)
            => X < other.MaxX && MaxX > other.X && Y < other.MaxY && MaxY > other.Y;

        public bool Equals(GridRect other)
            => X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;

        public override bool Equals(object obj) => obj is GridRect other && Equals(other);

        private const int HashSeed = 17;
        private const int HashStep = 31;

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = HashSeed;
                hash = hash * HashStep + X;
                hash = hash * HashStep + Y;
                hash = hash * HashStep + Width;
                hash = hash * HashStep + Height;
                return hash;
            }
        }

        public override string ToString() => $"GridRect(x:{X}, y:{Y}, w:{Width}, h:{Height})";
    }
}
