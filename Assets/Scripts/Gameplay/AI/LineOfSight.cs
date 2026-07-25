using System;
using ToDonegeon.Generation;

namespace ToDonegeon.Gameplay.AI
{
    public static class LineOfSight
    {
        public static bool IsVisible(Grid grid, Coord from, Coord to, int range)
        {
            if (grid == null) return false;
            if (ChebyshevDistance(from, to) > range) return false;
            return HasClearLine(grid, from, to);
        }

        // A Bresenham walk over the cells strictly between the endpoints.
        private static bool HasClearLine(Grid grid, Coord from, Coord to)
        {
            if (from.X > to.X || (from.X == to.X && from.Y > to.Y))
            {
                Coord swap = from;
                from = to;
                to = swap;
            }

            int x = from.X;
            int y = from.Y;
            int dx = Math.Abs(to.X - x);
            int dy = Math.Abs(to.Y - y);
            int stepX = x < to.X ? 1 : -1;
            int stepY = y < to.Y ? 1 : -1;
            int error = dx - dy;

            while (true)
            {
                int doubled = error * 2;
                if (doubled > -dy) { error -= dy; x += stepX; }
                if (doubled < dx) { error += dy; y += stepY; }

                if (x == to.X && y == to.Y) return true;
                if (!grid.IsWalkable(new Coord(x, y))) return false;
            }
        }

        private static int ChebyshevDistance(Coord a, Coord b)
            => Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
    }
}
