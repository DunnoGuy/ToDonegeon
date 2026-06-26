using NUnit.Framework;
using ToDonegeon.Generation;
using ToDonegeon.Gameplay;

namespace ToDonegeon.Tests
{
    public class GridMovementTests
    {
        // Build a grid ('#' wall, '.' floor). Row 0 is the top of the
        // picture and maps to the highest Y
        private static Grid GridFrom(params string[] rows)
        {
            int height = rows.Length;
            int width = rows[0].Length;
            var grid = new Grid(width, height);
            for (int row = 0; row < height; row++)
                for (int x = 0; x < width; x++)
                    grid[x, height - 1 - row] = rows[row][x] == '#' ? TileType.Wall : TileType.Floor;
            return grid;
        }

        [Test]
        public void Step_Onto_Floor_Succeeds()
        {
            Grid grid = GridFrom("...", "...", "...");

            bool ok = GridMovement.TryStep(grid, new Coord(1, 1), Coord.Up, out Coord to);

            Assert.That(ok, Is.True);
            Assert.That(to, Is.EqualTo(new Coord(1, 2)));
        }

        [Test]
        public void Step_Into_Wall_Fails_And_Keeps_Position()
        {
            Grid grid = GridFrom(".#.", "...", "...");

            bool ok = GridMovement.TryStep(grid, new Coord(1, 1), Coord.Up, out Coord to);

            Assert.That(ok, Is.False);
            Assert.That(to, Is.EqualTo(new Coord(1, 1)));
        }

        [Test]
        public void Step_Out_Of_Bounds_Fails()
        {
            Grid grid = GridFrom("...", "...", "...");

            Assert.That(GridMovement.TryStep(grid, new Coord(0, 0), Coord.Left, out _), Is.False);
        }

        [Test]
        public void Non_Unit_Step_Is_Rejected()
        {
            Grid grid = GridFrom("...", "...", "...");

            Assert.That(GridMovement.TryStep(grid, new Coord(0, 0), new Coord(2, 0), out _), Is.False);
            Assert.That(GridMovement.TryStep(grid, new Coord(0, 0), Coord.Zero, out _), Is.False);
        }

        [Test]
        public void Diagonal_Allowed_When_Both_Orthogonals_Open()
        {
            Grid grid = GridFrom("...", "...", "...");

            bool ok = GridMovement.TryStep(grid, new Coord(1, 1), new Coord(1, 1), out Coord to);

            Assert.That(ok, Is.True);
            Assert.That(to, Is.EqualTo(new Coord(2, 2)));
        }

        [Test]
        public void Diagonal_Through_A_Wall_Corner_Is_Forbidden()
        {
            // the two orthogonal cells (1,2) and (2,1) are walls.
            Grid grid = GridFrom(".#.", "..#", "...");

            bool ok = GridMovement.TryStep(grid, new Coord(1, 1), new Coord(1, 1), out _);

            Assert.That(ok, Is.False);
        }

        [Test]
        public void CanEnter_Reflects_Terrain_And_Bounds()
        {
            Grid grid = GridFrom(".#");

            Assert.That(GridMovement.CanEnter(grid, new Coord(0, 0)), Is.True);
            Assert.That(GridMovement.CanEnter(grid, new Coord(1, 0)), Is.False, "wall");
            Assert.That(GridMovement.CanEnter(grid, new Coord(9, 9)), Is.False, "out of bounds");
        }
    }
}
