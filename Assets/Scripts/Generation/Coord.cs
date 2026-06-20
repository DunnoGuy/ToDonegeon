using System;

namespace ToDonegeon.Generation
{
    // An integer grid coordinate: column X, row Y.
    // (0,0) is the bottom-left cell
    public readonly struct Coord : IEquatable<Coord>
    {
        public int X { get; }
        public int Y { get; }

        public Coord(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static readonly Coord Zero = new Coord(0, 0);

        public static readonly Coord Up = new Coord(0, 1);
        public static readonly Coord Down = new Coord(0, -1);
        public static readonly Coord Left = new Coord(-1, 0);
        public static readonly Coord Right = new Coord(1, 0);

        // the four orthogonal neighbour offsets (von Neumann neighbourhood): corridors are
        // 4-connected and the flood-fill validator expands across these offsets only.
        public static readonly Coord[] Orthogonal = { Up, Down, Left, Right };

        public static Coord operator +(Coord a, Coord b) => new Coord(a.X + b.X, a.Y + b.Y);
        public static Coord operator -(Coord a, Coord b) => new Coord(a.X - b.X, a.Y - b.Y);

        public static bool operator ==(Coord a, Coord b) => a.Equals(b);
        public static bool operator !=(Coord a, Coord b) => !a.Equals(b);

        // Manhattan (L1) distance - a metric for 4-connected corridor carving.
        public static int ManhattanDistance(Coord a, Coord b)
            => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

        public bool Equals(Coord other) => X == other.X && Y == other.Y;

        public override bool Equals(object obj) => obj is Coord other && Equals(other);

        private const int HashMultiplier = 397;

        public override int GetHashCode() => unchecked((X * HashMultiplier) ^ Y);

        public override string ToString() => $"({X}, {Y})";
    }
}
