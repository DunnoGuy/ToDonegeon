using ToDonegeon.Generation;

namespace ToDonegeon.Gameplay
{
    // Movement rules for the player and enemy
    public static class GridMovement
    {
        public static bool CanEnter(Grid grid, Coord cell) => grid != null && grid.IsWalkable(cell);

        public static bool TryStep(Grid grid, Coord from, Coord step, out Coord to)
        {
            to = from;
            if (grid == null) return false;
            if (step.X < -1 || step.X > 1 || step.Y < -1 || step.Y > 1) return false; // not a single step
            if (step.X == 0 && step.Y == 0) return false;                              // no movement

            Coord target = from + step;
            if (!CanEnter(grid, target)) return false;

            bool diagonal = step.X != 0 && step.Y != 0;
            if (diagonal &&
                (!CanEnter(grid, new Coord(from.X + step.X, from.Y)) ||
                 !CanEnter(grid, new Coord(from.X, from.Y + step.Y))))
            {
                return false; // would cut a wall corner
            }

            to = target;
            return true;
        }
    }
}
