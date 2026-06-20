using System;

namespace ToDonegeon.Generation
{
    // Cells are stored in a single flat TileType[] for cache-friendly iteration.
    public sealed class Grid
    {
        private readonly TileType[] _cells;

        public int Width { get; }
        public int Height { get; }

        public int CellCount => _cells.Length;

        public Grid(int width, int height, TileType fill = TileType.Wall)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");

            Width = width;
            Height = height;
            _cells = new TileType[width * height];

            if (fill != TileType.Wall) Fill(fill);
        }

        public bool InBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

        public bool InBounds(Coord c) => InBounds(c.X, c.Y);

        // reads or writes a single cell.
        public TileType this[int x, int y]
        {
            get
            {
                if (!InBounds(x, y)) throw OutOfRange(x, y);
                return _cells[Index(x, y)];
            }
            set
            {
                if (!InBounds(x, y)) throw OutOfRange(x, y);
                _cells[Index(x, y)] = value;
            }
        }

        public TileType this[Coord c]
        {
            get => this[c.X, c.Y];
            set => this[c.X, c.Y] = value;
        }

        public bool IsWalkable(Coord c) => InBounds(c) && _cells[Index(c.X, c.Y)].IsWalkable();

        public void Fill(TileType type)
        {
            for (int i = 0; i < _cells.Length; i++) _cells[i] = type;
        }

        public void Fill(GridRect rect, TileType type)
        {
            int xMin = Math.Max(0, rect.X);
            int yMin = Math.Max(0, rect.Y);
            int xMax = Math.Min(Width, rect.MaxX);
            int yMax = Math.Min(Height, rect.MaxY);

            for (int y = yMin; y < yMax; y++)
                for (int x = xMin; x < xMax; x++)
                    _cells[Index(x, y)] = type;
        }

        // counts how many cells currently hold the given type.
        public int Count(TileType type)
        {
            int n = 0;
            for (int i = 0; i < _cells.Length; i++)
                if (_cells[i] == type) n++;
            return n;
        }

        private int Index(int x, int y) => y * Width + x;

        private IndexOutOfRangeException OutOfRange(int x, int y)
            => new IndexOutOfRangeException($"Cell ({x}, {y}) is outside the {Width}x{Height} grid.");
    }
}
