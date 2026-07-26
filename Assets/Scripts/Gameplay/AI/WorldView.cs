using System;
using System.Collections.Generic;
using ToDonegeon.Generation;

namespace ToDonegeon.Gameplay.AI
{
    // A snapshot the enemy AI reads
    public sealed class WorldView
    {
        private readonly HashSet<Coord> _occupied;

        public Grid Grid { get; }
        public Coord PlayerCell { get; }

        public WorldView(Grid grid, Coord playerCell, IReadOnlyCollection<Coord> occupied)
        {
            Grid = grid ?? throw new ArgumentNullException(nameof(grid));
            PlayerCell = playerCell;
            _occupied = occupied != null ? new HashSet<Coord>(occupied) : new HashSet<Coord>();
        }

        public bool IsOccupied(Coord cell) => _occupied.Contains(cell);

        // A cell an enemy cannot move into
        public bool IsBlocked(Coord cell) => cell == PlayerCell || _occupied.Contains(cell);
    }
}
