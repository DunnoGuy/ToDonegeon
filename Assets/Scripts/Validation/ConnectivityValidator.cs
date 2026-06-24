using System;
using System.Collections.Generic;
using ToDonegeon.Generation;

namespace ToDonegeon.Validation
{
    public sealed class ConnectivityValidator
    {
        // Floods walkable cells from the start and reports reachability.
        public GridConnectivity ValidateGrid(Grid grid, Coord start)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));

            HashSet<Coord> reached = FloodFill(grid, start);

            var unreachable = new List<Coord>();
            int walkable = 0;
            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    if (!grid[x, y].IsWalkable()) continue;

                    walkable++;
                    var cell = new Coord(x, y);
                    if (!reached.Contains(cell)) unreachable.Add(cell);
                }
            }

            return new GridConnectivity(walkable, reached.Count, unreachable);
        }

        // True when the target can be reached from the start across walkable cells.
        public bool IsReachable(Grid grid, Coord start, Coord target)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));
            return grid.IsWalkable(target) && FloodFill(grid, start).Contains(target);
        }

        private static HashSet<Coord> FloodFill(Grid grid, Coord start)
        {
            var visited = new HashSet<Coord>();
            if (!grid.IsWalkable(start)) return visited;

            var queue = new Queue<Coord>();
            visited.Add(start);
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                Coord cell = queue.Dequeue();
                foreach (Coord step in Coord.Orthogonal)
                {
                    Coord next = cell + step;
                    if (grid.IsWalkable(next) && visited.Add(next))
                        queue.Enqueue(next);
                }
            }

            return visited;
        }
    }
}
