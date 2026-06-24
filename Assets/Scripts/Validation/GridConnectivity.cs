using System;
using System.Collections.Generic;
using ToDonegeon.Generation;

namespace ToDonegeon.Validation
{
    // How many walkable cells exist, how many the fill reached,
    // and which walkable cells were stranded
    public sealed class GridConnectivity
    {
        public int WalkableCount { get; }
        public int ReachableCount { get; }
        public IReadOnlyList<Coord> UnreachableCells { get; }

        public GridConnectivity(int walkableCount, int reachableCount, IReadOnlyList<Coord> unreachableCells)
        {
            WalkableCount = walkableCount;
            ReachableCount = reachableCount;
            UnreachableCells = unreachableCells ?? throw new ArgumentNullException(nameof(unreachableCells));
        }

        public bool IsFullyConnected => WalkableCount == ReachableCount;

        public int UnreachableCount => UnreachableCells.Count;
    }
}
