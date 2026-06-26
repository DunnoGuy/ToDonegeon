using System.Collections.Generic;
using ToDonegeon.Generation;

namespace ToDonegeon.Gameplay
{
    // A* pathfinding
    public static class GridPathfinder
    {
        private const int CardinalCost = 10;
        private const int DiagonalCost = 14;

        private static readonly Coord[] Steps =
        {
            Coord.Up, Coord.Down, Coord.Left, Coord.Right,
            Coord.Up + Coord.Left, Coord.Up + Coord.Right,
            Coord.Down + Coord.Left, Coord.Down + Coord.Right
        };

        public static IReadOnlyList<Coord> FindPath(Grid grid, Coord start, Coord goal)
        {
            var path = new List<Coord>();
            if (grid == null || start == goal) return path;
            if (!GridMovement.CanEnter(grid, start) || !GridMovement.CanEnter(grid, goal)) return path;

            var gScore = new Dictionary<Coord, int> { [start] = 0 };
            var cameFrom = new Dictionary<Coord, Coord>();
            var closed = new HashSet<Coord>();
            var open = new SortedSet<Node>(NodeComparer.Instance);

            int sequence = 0;
            open.Add(new Node(start, Heuristic(start, goal), sequence++));

            while (open.Count > 0)
            {
                Node current = open.Min;
                open.Remove(current);
                Coord cell = current.Cell;

                if (cell == goal) return Reconstruct(cameFrom, goal);
                if (!closed.Add(cell)) continue; // a stale, superseded entry

                int baseG = gScore[cell];
                foreach (Coord step in Steps)
                {
                    if (!GridMovement.TryStep(grid, cell, step, out Coord next)) continue;
                    if (closed.Contains(next)) continue;

                    int tentative = baseG + (step.X != 0 && step.Y != 0 ? DiagonalCost : CardinalCost);
                    if (gScore.TryGetValue(next, out int known) && tentative >= known) continue;

                    gScore[next] = tentative;
                    cameFrom[next] = cell;
                    open.Add(new Node(next, tentative + Heuristic(next, goal), sequence++));
                }
            }

            return path; // unreachable
        }

        private static List<Coord> Reconstruct(Dictionary<Coord, Coord> cameFrom, Coord goal)
        {
            var path = new List<Coord>();
            Coord cell = goal;
            while (cameFrom.TryGetValue(cell, out Coord parent))
            {
                path.Add(cell);
                cell = parent;
            }
            path.Reverse(); // start-exclusive, goal-inclusive, in walking order
            return path;
        }

        private static int Heuristic(Coord a, Coord b)
        {
            int dx = System.Math.Abs(a.X - b.X);
            int dy = System.Math.Abs(a.Y - b.Y);
            int min = System.Math.Min(dx, dy);
            int max = System.Math.Max(dx, dy);
            return min * DiagonalCost + (max - min) * CardinalCost;
        }

        private readonly struct Node
        {
            public Coord Cell { get; }
            public int F { get; }
            public int Sequence { get; }

            public Node(Coord cell, int f, int sequence)
            {
                Cell = cell;
                F = f;
                Sequence = sequence;
            }
        }

        private sealed class NodeComparer : IComparer<Node>
        {
            public static readonly NodeComparer Instance = new NodeComparer();

            public int Compare(Node a, Node b)
            {
                int byF = a.F.CompareTo(b.F);
                return byF != 0 ? byF : a.Sequence.CompareTo(b.Sequence);
            }
        }
    }
}
