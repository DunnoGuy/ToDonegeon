using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using ToDonegeon.Generation;

namespace ToDonegeon.Tests
{
    public class RandomWalkTests
    {
        private const int GridSize = 40;
        private static readonly GridRect Region = new GridRect(5, 5, 24, 24);

        private static (Grid grid, WalkResult result) Carve(int seed, RandomWalkSettings settings = null)
        {
            var grid = new Grid(GridSize, GridSize);
            WalkResult result = new RandomWalk(settings ?? new RandomWalkSettings())
                .Carve(grid, Region, new Random(seed));
            return (grid, result);
        }

        private static string FloorSignature(Grid grid)
        {
            var sb = new StringBuilder(Region.Area);
            for (int y = Region.Y; y < Region.MaxY; y++)
                for (int x = Region.X; x < Region.MaxX; x++)
                    sb.Append(grid[x, y] == TileType.Floor ? '1' : '0');
            return sb.ToString();
        }

        private static int ReachableFloorCount(Grid grid, Coord start)
        {
            var visited = new HashSet<Coord> { start };
            var stack = new Stack<Coord>();
            stack.Push(start);
            while (stack.Count > 0)
            {
                Coord cell = stack.Pop();
                foreach (Coord step in Coord.Orthogonal)
                {
                    Coord next = cell + step;
                    if (!grid.InBounds(next) || visited.Contains(next)) continue;
                    if (grid[next] != TileType.Floor) continue;
                    visited.Add(next);
                    stack.Push(next);
                }
            }
            return visited.Count;
        }

        [Test]
        public void Same_Seed_Produces_Identical_Walks()
        {
            Assert.That(FloorSignature(Carve(123).grid), Is.EqualTo(FloorSignature(Carve(123).grid)));
        }

        [Test]
        public void Different_Seeds_Produce_Varied_Walks()
        {
            var signatures = new HashSet<string>();
            for (int seed = 0; seed < 16; seed++)
                signatures.Add(FloorSignature(Carve(seed).grid));

            Assert.That(signatures.Count, Is.GreaterThan(1));
        }

        [Test]
        public void Floor_Only_Appears_Inside_The_Region()
        {
            Grid grid = Carve(7).grid;
            for (int y = 0; y < GridSize; y++)
                for (int x = 0; x < GridSize; x++)
                    if (!Region.Contains(new Coord(x, y)))
                        Assert.That(grid[x, y], Is.EqualTo(TileType.Wall), $"cell ({x},{y}) outside region");
        }

        [Test]
        public void Anchor_Is_A_Floor_Cell_Inside_The_Region()
        {
            for (int seed = 0; seed < 40; seed++)
            {
                var (grid, result) = Carve(seed);
                Assert.That(Region.Contains(result.Anchor), Is.True, $"seed {seed}");
                Assert.That(grid[result.Anchor], Is.EqualTo(TileType.Floor), $"seed {seed}");
            }
        }

        [Test]
        public void Whole_Trail_Is_Reachable_From_The_Anchor()
        {
            for (int seed = 0; seed < 40; seed++)
            {
                var (grid, result) = Carve(seed);
                Assert.That(ReachableFloorCount(grid, result.Anchor), Is.EqualTo(result.FloorCount),
                    $"seed {seed}: trail is not one connected blob");
            }
        }

        [Test]
        public void Carves_More_Than_Its_Start_Cell()
        {
            for (int seed = 0; seed < 40; seed++)
            {
                WalkResult result = Carve(seed).result;
                Assert.That(result.FloorCount, Is.GreaterThan(1), $"seed {seed}");
                Assert.That(result.FloorCount, Is.LessThanOrEqualTo(Region.Area), $"seed {seed}");
            }
        }
    }

    public class RandomWalkSettingsTests
    {
        [Test]
        public void Rejects_Fill_Fraction_Outside_Its_Range()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new RandomWalkSettings(fillFraction: 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RandomWalkSettings(fillFraction: 1.5f));
        }

        [Test]
        public void Rejects_Step_Multiplier_Below_One()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new RandomWalkSettings(maxStepsMultiplier: 0));
        }
    }
}
