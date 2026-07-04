using System;
using System.Collections.Generic;

namespace ToDonegeon.Generation
{
    // Carves a winding passage by stepping a point around the region at random and turning
    // each cell it visits to floor.
    public sealed class RandomWalk
    {
        private readonly RandomWalkSettings _settings;

        public RandomWalk(RandomWalkSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        // carves floor into the region and returns where the walk began
        public WalkResult Carve(Grid grid, GridRect region, Random rng)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));
            if (rng == null) throw new ArgumentNullException(nameof(rng));
            if (region.Width < 1 || region.Height < 1)
                throw new ArgumentException("Region must be at least one cell.", nameof(region));

            int target = Math.Max(1, (int)Math.Round(region.Area * _settings.FillFraction));
            target = Math.Min(target, region.Area);
            int maxSteps = region.Area * _settings.MaxStepsMultiplier;

            Coord start = region.Center;
            var carved = new HashSet<Coord> { start };
            grid[start] = TileType.Floor;

            Coord pos = start;
            int steps = 0;
            while (carved.Count < target && steps < maxSteps)
            {
                steps++;
                Coord next = pos + Coord.Orthogonal[rng.Next(Coord.Orthogonal.Length)];
                if (!region.Contains(next)) continue;

                pos = next;
                if (carved.Add(pos)) grid[pos] = TileType.Floor;
            }

            return new WalkResult(start, BoundsOf(carved), carved.Count);
        }

        private static GridRect BoundsOf(HashSet<Coord> cells)
        {
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            foreach (Coord cell in cells)
            {
                if (cell.X < minX) minX = cell.X;
                if (cell.Y < minY) minY = cell.Y;
                if (cell.X > maxX) maxX = cell.X;
                if (cell.Y > maxY) maxY = cell.Y;
            }
            return new GridRect(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }
    }
}
