using System.Collections.Generic;
using NUnit.Framework;
using ToDonegeon.Generation;
using ToDonegeon.Gameplay;

namespace ToDonegeon.Tests
{
    public class GridPathfinderTests
    {
        // '#' wall, '.' floor
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

        private static void AssertWalkable(Grid grid, Coord start, IReadOnlyList<Coord> path)
        {
            Coord prev = start;
            foreach (Coord cell in path)
            {
                Assert.That(GridMovement.TryStep(grid, prev, cell - prev, out Coord to), Is.True,
                    $"step {prev} -> {cell} must be legal");
                Assert.That(to, Is.EqualTo(cell));
                prev = cell;
            }
        }

        [Test]
        public void Finds_A_Straight_Path()
        {
            Grid grid = GridFrom(".....");
            IReadOnlyList<Coord> path = GridPathfinder.FindPath(grid, new Coord(0, 0), new Coord(4, 0));

            Assert.That(path.Count, Is.EqualTo(4));
            Assert.That(path[path.Count - 1], Is.EqualTo(new Coord(4, 0)));
            AssertWalkable(grid, new Coord(0, 0), path);
        }

        [Test]
        public void Uses_Diagonals_To_Reach_A_Corner()
        {
            Grid grid = GridFrom("...", "...", "...");
            IReadOnlyList<Coord> path = GridPathfinder.FindPath(grid, new Coord(0, 0), new Coord(2, 2));

            Assert.That(path.Count, Is.EqualTo(2), "two diagonal steps");
            Assert.That(path[path.Count - 1], Is.EqualTo(new Coord(2, 2)));
            AssertWalkable(grid, new Coord(0, 0), path);
        }

        [Test]
        public void Routes_Around_A_Wall()
        {
            Grid grid = GridFrom(
                "..#..",
                "..#..",
                "..#..",
                "..#..",
                ".....");
            var start = new Coord(0, 3);
            var goal = new Coord(4, 3);

            IReadOnlyList<Coord> path = GridPathfinder.FindPath(grid, start, goal);

            Assert.That(path.Count, Is.GreaterThan(0));
            Assert.That(path[path.Count - 1], Is.EqualTo(goal));
            AssertWalkable(grid, start, path);
            foreach (Coord cell in path)
                Assert.That(grid[cell], Is.Not.EqualTo(TileType.Wall));
        }

        [Test]
        public void Returns_Empty_When_Goal_Is_Walled_Off()
        {
            Grid grid = GridFrom(
                ".....",
                ".###.",
                ".#.#.",
                ".###.",
                ".....");
            IReadOnlyList<Coord> path = GridPathfinder.FindPath(grid, new Coord(0, 0), new Coord(2, 2));

            Assert.That(path, Is.Empty);
        }

        [Test]
        public void Returns_Empty_When_Goal_Is_A_Wall()
        {
            Grid grid = GridFrom(".#.");
            Assert.That(GridPathfinder.FindPath(grid, new Coord(0, 0), new Coord(1, 0)), Is.Empty);
        }

        [Test]
        public void Returns_Empty_When_Start_Equals_Goal()
        {
            Grid grid = GridFrom("...");
            Assert.That(GridPathfinder.FindPath(grid, new Coord(1, 0), new Coord(1, 0)), Is.Empty);
        }
    }
}
