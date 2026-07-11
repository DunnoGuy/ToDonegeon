using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ToDonegeon.Generation;
using ToDonegeon.Validation;

namespace ToDonegeon.Tests
{
    public class ChamberCarvingTests
    {
        private const int Width = 64;
        private const int Height = 48;

        private static (Grid grid, BspTree tree, IReadOnlyList<Room> rooms) Generate(int seed, ChamberShape shape)
        {
            var rng = new Random(seed);
            BspTree tree = new BspPartitioner(new BspSettings()).Partition(new GridRect(0, 0, Width, Height), rng);
            var grid = new Grid(Width, Height);
            IReadOnlyList<Room> rooms = new RoomCarver(new RoomCarverSettings(shape: shape)).Carve(grid, tree, rng);
            return (grid, tree, rooms);
        }

        private static string FloorSignature(Grid grid)
        {
            var sb = new StringBuilder(Width * Height);
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    sb.Append(grid[x, y] == TileType.Floor ? '1' : '0');
            return sb.ToString();
        }

        [TestCase(ChamberShape.Cave)]
        [TestCase(ChamberShape.RandomWalk)]
        public void Chamber_Centres_Sit_On_Floor(ChamberShape shape)
        {
            for (int seed = 0; seed < 40; seed++)
            {
                var (grid, _, rooms) = Generate(seed, shape);
                foreach (Room room in rooms)
                    Assert.That(grid[room.Center], Is.EqualTo(TileType.Floor), $"seed {seed}");
            }
        }

        [TestCase(ChamberShape.Cave)]
        [TestCase(ChamberShape.RandomWalk)]
        public void Chambers_Stay_Inside_Their_Leaf_With_Padding(ChamberShape shape)
        {
            const int pad = RoomCarverSettings.DefaultEdgePadding;
            var (_, _, rooms) = Generate(5, shape);
            foreach (Room room in rooms)
            {
                GridRect leaf = room.Leaf.Region;
                GridRect bounds = room.Bounds;
                Assert.That(bounds.X, Is.GreaterThanOrEqualTo(leaf.X + pad));
                Assert.That(bounds.Y, Is.GreaterThanOrEqualTo(leaf.Y + pad));
                Assert.That(bounds.MaxX, Is.LessThanOrEqualTo(leaf.MaxX - pad));
                Assert.That(bounds.MaxY, Is.LessThanOrEqualTo(leaf.MaxY - pad));
            }
        }

        [TestCase(ChamberShape.Cave)]
        [TestCase(ChamberShape.RandomWalk)]
        public void Same_Seed_Produces_Identical_Layout(ChamberShape shape)
        {
            Assert.That(FloorSignature(Generate(123, shape).grid),
                Is.EqualTo(FloorSignature(Generate(123, shape).grid)));
        }

        [TestCase(ChamberShape.Cave)]
        [TestCase(ChamberShape.RandomWalk)]
        public void Different_Seeds_Produce_Varied_Layout(ChamberShape shape)
        {
            var signatures = new HashSet<string>();
            for (int seed = 0; seed < 16; seed++)
                signatures.Add(FloorSignature(Generate(seed, shape).grid));

            Assert.That(signatures.Count, Is.GreaterThan(1));
        }

        [TestCase(ChamberShape.Cave)]
        [TestCase(ChamberShape.RandomWalk)]
        public void Chambers_Are_Not_Solid_Rectangles(ChamberShape shape)
        {
            for (int seed = 0; seed < 20; seed++)
            {
                var (grid, _, rooms) = Generate(seed, shape);
                int floor = grid.Count(TileType.Floor);
                int boundsArea = rooms.Sum(r => r.Bounds.Area);
                Assert.That(floor, Is.LessThan(boundsArea), $"seed {seed}: chambers fill their whole boxes");
            }
        }

        [TestCase(ChamberShape.Cave)]
        [TestCase(ChamberShape.RandomWalk)]
        public void Organic_Dungeons_Connect_Through_Corridors(ChamberShape shape)
        {
            for (int seed = 0; seed < 40; seed++)
            {
                var (grid, tree, rooms) = Generate(seed, shape);
                new CorridorConnector().Connect(grid, tree, rooms, new Random(seed + 1));

                Coord start = rooms[0].Center;
                GridConnectivity connectivity = new ConnectivityValidator().ValidateGrid(grid, start);
                Assert.That(connectivity.IsFullyConnected, Is.True, $"seed {seed}: {connectivity.UnreachableCount} stranded");
            }
        }
    }
}
