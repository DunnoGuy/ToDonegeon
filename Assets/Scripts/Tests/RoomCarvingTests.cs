using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ToDonegeon.Generation;

namespace ToDonegeon.Tests
{
    public class RoomCarvingTests
    {
        private const int Width = 64;
        private const int Height = 48;

        // runs partition then carving off one shared rng stream.
        private static (Grid grid, BspTree tree, IReadOnlyList<Room> rooms) Generate(
            int seed, BspSettings bsp = null, RoomCarverSettings carve = null)
        {
            var rng = new Random(seed);
            BspTree tree = new BspPartitioner(bsp ?? new BspSettings())
                .Partition(new GridRect(0, 0, Width, Height), rng);
            var grid = new Grid(Width, Height);
            IReadOnlyList<Room> rooms = new RoomCarver(carve ?? new RoomCarverSettings()).Carve(grid, tree, rng);
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

        [Test]
        public void Same_Seed_Produces_Identical_Rooms()
        {
            Assert.That(FloorSignature(Generate(123).grid), Is.EqualTo(FloorSignature(Generate(123).grid)));
        }

        [Test]
        public void Different_Seeds_Produce_Varied_Rooms()
        {
            var signatures = new HashSet<string>();
            for (int seed = 0; seed < 16; seed++)
                signatures.Add(FloorSignature(Generate(seed).grid));

            Assert.That(signatures.Count, Is.GreaterThan(1));
        }

        [Test]
        public void One_Room_Per_Leaf_For_Normal_Settings()
        {
            var (_, tree, rooms) = Generate(8);
            Assert.That(rooms.Count, Is.EqualTo(tree.Leaves.Count));
        }

        [Test]
        public void Rooms_Stay_Inside_Their_Leaf_With_Padding()
        {
            var carve = new RoomCarverSettings();
            var (_, _, rooms) = Generate(5, carve: carve);

            foreach (Room room in rooms)
            {
                GridRect leaf = room.Leaf.Region;
                GridRect bounds = room.Bounds;
                Assert.That(bounds.X, Is.GreaterThanOrEqualTo(leaf.X + carve.EdgePadding));
                Assert.That(bounds.Y, Is.GreaterThanOrEqualTo(leaf.Y + carve.EdgePadding));
                Assert.That(bounds.MaxX, Is.LessThanOrEqualTo(leaf.MaxX - carve.EdgePadding));
                Assert.That(bounds.MaxY, Is.LessThanOrEqualTo(leaf.MaxY - carve.EdgePadding));
            }
        }

        [Test]
        public void Rooms_Respect_Minimum_Size()
        {
            var carve = new RoomCarverSettings();
            var (_, _, rooms) = Generate(11, carve: carve);

            foreach (Room room in rooms)
            {
                Assert.That(room.Bounds.Width, Is.GreaterThanOrEqualTo(carve.MinRoomSize));
                Assert.That(room.Bounds.Height, Is.GreaterThanOrEqualTo(carve.MinRoomSize));
            }
        }

        [Test]
        public void Carved_Floor_Count_Matches_Total_Room_Area()
        {
            var (grid, _, rooms) = Generate(17);
            Assert.That(grid.Count(TileType.Floor), Is.EqualTo(rooms.Sum(r => r.Bounds.Area)));
        }

        [Test]
        public void Rooms_Do_Not_Overlap()
        {
            var rooms = Generate(21).rooms;
            for (int i = 0; i < rooms.Count; i++)
                for (int j = i + 1; j < rooms.Count; j++)
                    Assert.That(rooms[i].Bounds.Overlaps(rooms[j].Bounds), Is.False);
        }

        [Test]
        public void Every_Room_Cell_Is_Floor()
        {
            var (grid, _, rooms) = Generate(33);
            foreach (Room room in rooms)
                for (int y = room.Bounds.Y; y < room.Bounds.MaxY; y++)
                    for (int x = room.Bounds.X; x < room.Bounds.MaxX; x++)
                        Assert.That(grid[x, y], Is.EqualTo(TileType.Floor));
        }
    }

    public class RoomCarverSettingsTests
    {
        [Test]
        public void Rejects_Negative_Padding()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new RoomCarverSettings(edgePadding: -1));
        }

        [Test]
        public void Rejects_Fill_Ratio_Outside_Its_Range()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new RoomCarverSettings(minFillRatio: 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RoomCarverSettings(minFillRatio: 1.5f));
        }

        [Test]
        public void Rejects_Zero_Min_Room_Size()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new RoomCarverSettings(minRoomSize: 0));
        }
    }
}
