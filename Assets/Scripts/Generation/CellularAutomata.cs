using System;
using System.Collections.Generic;

namespace ToDonegeon.Generation
{
    // Grows an organic cave into a rectangular region. A random seed followed by repeated
    // smoothing under a wall-majority rule. Cells outside the region count as wall while
    // smoothing, so the cave stays within the region and rounds off at its edges.
    public sealed class CellularAutomata
    {
        private readonly CaveSettings _settings;

        public CellularAutomata(CaveSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        // writes cave floor into the region and returns its connectivity summary
        public CaveResult Fill(Grid grid, GridRect region, Random rng)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));
            if (rng == null) throw new ArgumentNullException(nameof(rng));
            if (region.Width < 1 || region.Height < 1)
                throw new ArgumentException("Region must be at least one cell.", nameof(region));

            int w = region.Width;
            int h = region.Height;

            bool[,] floor = _settings.NoiseSeeded ? SeedFromNoise(region, rng) : SeedRandom(w, h, rng);
            for (int i = 0; i < _settings.SmoothingIterations; i++)
                floor = Smooth(floor, w, h);

            List<List<Coord>> components = FindComponents(floor, w, h);
            List<Coord> largest = Largest(components);

            if (largest == null)
            {
                // smoothing wiped every cell. Keep a one-cell chamber so it stays connectable.
                var centre = new Coord(w / 2, h / 2);
                floor[centre.X, centre.Y] = true;
                largest = new List<Coord> { centre };
                components = new List<List<Coord>> { largest };
            }

            if (_settings.KeepOnlyLargestComponent)
                KeepOnly(floor, w, h, largest);

            WriteFloor(grid, region, floor, w, h, out int floorCount, out GridRect floorBounds);

            Coord anchorLocal = NearestToCentre(largest, w, h);
            var anchor = new Coord(region.X + anchorLocal.X, region.Y + anchorLocal.Y);
            int componentCount = _settings.KeepOnlyLargestComponent ? 1 : components.Count;

            return new CaveResult(anchor, floorBounds, floorCount, largest.Count, componentCount);
        }

        private bool[,] SeedRandom(int w, int h, Random rng)
        {
            var floor = new bool[w, h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    floor[x, y] = rng.NextDouble() >= _settings.InitialWallProbability;
            return floor;
        }

        // seeds openness from a perlin field over the region's world position, so nearby
        // cells tend to agree and the caves come out in coherent patches
        private bool[,] SeedFromNoise(GridRect region, Random rng)
        {
            var noise = new PerlinNoise(rng.Next());
            var floor = new bool[region.Width, region.Height];
            for (int y = 0; y < region.Height; y++)
                for (int x = 0; x < region.Width; x++)
                {
                    float value = noise.Noise((region.X + x) * _settings.NoiseFrequency,
                        (region.Y + y) * _settings.NoiseFrequency);
                    floor[x, y] = value >= _settings.InitialWallProbability;
                }
            return floor;
        }

        private bool[,] Smooth(bool[,] floor, int w, int h)
        {
            var next = new bool[w, h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    next[x, y] = WallNeighbours(floor, x, y, w, h) < _settings.WallThreshold;
            return next;
        }

        // counts wall cells in the 8-cell neighbourhood
        private static int WallNeighbours(bool[,] floor, int x, int y, int w, int h)
        {
            int walls = 0;
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = x + dx;
                    int ny = y + dy;
                    if (nx < 0 || nx >= w || ny < 0 || ny >= h || !floor[nx, ny]) walls++;
                }
            }
            return walls;
        }

        private static List<List<Coord>> FindComponents(bool[,] floor, int w, int h)
        {
            var components = new List<List<Coord>>();
            var visited = new bool[w, h];
            var stack = new Stack<Coord>();

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (!floor[x, y] || visited[x, y]) continue;

                    var cells = new List<Coord>();
                    visited[x, y] = true;
                    stack.Push(new Coord(x, y));
                    while (stack.Count > 0)
                    {
                        Coord cell = stack.Pop();
                        cells.Add(cell);
                        foreach (Coord step in Coord.Orthogonal)
                        {
                            int nx = cell.X + step.X;
                            int ny = cell.Y + step.Y;
                            if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;
                            if (!floor[nx, ny] || visited[nx, ny]) continue;
                            visited[nx, ny] = true;
                            stack.Push(new Coord(nx, ny));
                        }
                    }
                    components.Add(cells);
                }
            }
            return components;
        }

        // The first component with the most cells in scan order
        private static List<Coord> Largest(List<List<Coord>> components)
        {
            List<Coord> best = null;
            foreach (List<Coord> component in components)
                if (best == null || component.Count > best.Count) best = component;
            return best;
        }

        private static void KeepOnly(bool[,] floor, int w, int h, List<Coord> largest)
        {
            var keep = new bool[w, h];
            foreach (Coord cell in largest) keep[cell.X, cell.Y] = true;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    floor[x, y] = keep[x, y];
        }

        private static void WriteFloor(
            Grid grid, GridRect region, bool[,] floor, int w, int h,
            out int floorCount, out GridRect floorBounds)
        {
            floorCount = 0;
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (!floor[x, y]) continue;

                    int gx = region.X + x;
                    int gy = region.Y + y;
                    grid[gx, gy] = TileType.Floor;
                    floorCount++;
                    if (gx < minX) minX = gx;
                    if (gy < minY) minY = gy;
                    if (gx > maxX) maxX = gx;
                    if (gy > maxY) maxY = gy;
                }
            }

            floorBounds = floorCount > 0
                ? new GridRect(minX, minY, maxX - minX + 1, maxY - minY + 1)
                : new GridRect(region.X, region.Y, 1, 1);
        }

        // The largest component's cell closest to the region centre
        private static Coord NearestToCentre(List<Coord> cells, int w, int h)
        {
            var centre = new Coord(w / 2, h / 2);
            Coord best = cells[0];
            int bestDistance = Coord.ManhattanDistance(best, centre);
            for (int i = 1; i < cells.Count; i++)
            {
                int distance = Coord.ManhattanDistance(cells[i], centre);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = cells[i];
                }
            }
            return best;
        }
    }
}
