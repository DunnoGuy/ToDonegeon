using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using ToDonegeon.Generation;

namespace ToDonegeon.Tests
{
    public class CellularAutomataTests
    {
        private const int GridSize = 40;
        private static readonly GridRect Region = new GridRect(5, 5, 24, 24);

        private static (Grid grid, CaveResult result) Fill(int seed, CaveSettings settings = null)
        {
            var grid = new Grid(GridSize, GridSize);
            CaveResult result = new CellularAutomata(settings ?? new CaveSettings())
                .Fill(grid, Region, new Random(seed));
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

        // 4-connected flood fill over floor cells, matching the connectivity validator.
        private static int ReachableFloorCount(Grid grid, Coord start)
        {
            var visited = new HashSet<Coord>();
            var stack = new Stack<Coord>();
            visited.Add(start);
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
        public void Same_Seed_Produces_Identical_Caves()
        {
            Assert.That(FloorSignature(Fill(123).grid), Is.EqualTo(FloorSignature(Fill(123).grid)));
        }

        [Test]
        public void Different_Seeds_Produce_Varied_Caves()
        {
            var signatures = new HashSet<string>();
            for (int seed = 0; seed < 16; seed++)
                signatures.Add(FloorSignature(Fill(seed).grid));

            Assert.That(signatures.Count, Is.GreaterThan(1));
        }

        [Test]
        public void Floor_Only_Appears_Inside_The_Region()
        {
            Grid grid = Fill(7).grid;
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
                var (grid, result) = Fill(seed);
                Assert.That(Region.Contains(result.Anchor), Is.True, $"seed {seed}: anchor outside region");
                Assert.That(grid[result.Anchor], Is.EqualTo(TileType.Floor), $"seed {seed}: anchor not floor");
            }
        }

        [Test]
        public void Produces_A_NonTrivial_Cave()
        {
            for (int seed = 0; seed < 40; seed++)
            {
                CaveResult result = Fill(seed).result;
                Assert.That(result.FloorCount, Is.GreaterThan(0), $"seed {seed}: empty cave");
                Assert.That(result.FloorCount, Is.LessThan(Region.Area), $"seed {seed}: solid block, not a cave");
            }
        }

        [Test]
        public void Largest_Component_Only_Yields_One_Connected_Blob()
        {
            var settings = new CaveSettings(keepOnlyLargestComponent: true);
            for (int seed = 0; seed < 40; seed++)
            {
                var (grid, result) = Fill(seed, settings);
                Assert.That(result.ComponentCount, Is.EqualTo(1), $"seed {seed}: more than one component kept");
                Assert.That(ReachableFloorCount(grid, result.Anchor), Is.EqualTo(result.FloorCount),
                    $"seed {seed}: floor not fully reachable from the anchor");
            }
        }

        [Test]
        public void Noise_Seeding_Changes_The_Result()
        {
            var uniform = new CaveSettings(noiseSeeded: false);
            var perlin = new CaveSettings(noiseSeeded: true);
            Assert.That(FloorSignature(Fill(42, perlin).grid),
                Is.Not.EqualTo(FloorSignature(Fill(42, uniform).grid)));
        }

        [Test]
        public void Noise_Seeded_Caves_Are_Still_Valid()
        {
            var settings = new CaveSettings(noiseSeeded: true);
            for (int seed = 0; seed < 40; seed++)
            {
                var (grid, result) = Fill(seed, settings);
                Assert.That(grid[result.Anchor], Is.EqualTo(TileType.Floor), $"seed {seed}: anchor not floor");
                Assert.That(result.ComponentCount, Is.EqualTo(1), $"seed {seed}: more than one component");
                Assert.That(ReachableFloorCount(grid, result.Anchor), Is.EqualTo(result.FloorCount),
                    $"seed {seed}: floor not fully reachable from the anchor");
                Assert.That(result.FloorCount, Is.GreaterThan(0), $"seed {seed}: empty cave");
            }
        }
    }

    public class CaveSettingsTests
    {
        [Test]
        public void Rejects_Wall_Probability_Outside_Its_Range()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CaveSettings(initialWallProbability: -0.1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CaveSettings(initialWallProbability: 1.1f));
        }

        [Test]
        public void Rejects_Negative_Iterations()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CaveSettings(smoothingIterations: -1));
        }

        [Test]
        public void Rejects_Wall_Threshold_Outside_The_Neighbourhood()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CaveSettings(wallThreshold: 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CaveSettings(wallThreshold: 9));
        }

        [Test]
        public void Rejects_Nonpositive_Noise_Frequency()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CaveSettings(noiseFrequency: 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CaveSettings(noiseFrequency: -0.1f));
        }
    }
}
