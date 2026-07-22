using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ToDonegeon.Generation;
using ToDonegeon.Validation;

namespace ToDonegeon.Tests
{
    public class ZonedCarvingTests
    {
        private const int Width = 64;
        private const int Height = 48;

        private static readonly ChamberShape[] Mix =
            { ChamberShape.Rectangle, ChamberShape.Cave, ChamberShape.RandomWalk };

        private static (Grid grid, BspTree tree, IReadOnlyList<Room> rooms) Carve(int seed, RoomCarverSettings settings)
        {
            var rng = new Random(seed);
            BspTree tree = new BspPartitioner(new BspSettings()).Partition(new GridRect(0, 0, Width, Height), rng);
            var grid = new Grid(Width, Height);
            IReadOnlyList<Room> rooms = new RoomCarver(settings).Carve(grid, tree, rng);
            return (grid, tree, rooms);
        }

        private static RoomCarverSettings Zoned(int zoneDepth) =>
            new RoomCarverSettings(shapeMix: Mix, shapeZoneDepth: zoneDepth);

        private static string FloorSignature(Grid grid)
        {
            var sb = new StringBuilder(Width * Height);
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    sb.Append(grid[x, y] == TileType.Floor ? '1' : '0');
            return sb.ToString();
        }

        [Test]
        public void Zoned_Dungeons_Stay_Connected()
        {
            for (int seed = 0; seed < 40; seed++)
            {
                var (grid, tree, rooms) = Carve(seed, Zoned(2));
                new CorridorConnector().Connect(grid, tree, rooms, new Random(seed + 1));

                GridConnectivity connectivity = new ConnectivityValidator().ValidateGrid(grid, rooms[0].Center);
                Assert.That(connectivity.IsFullyConnected, Is.True, $"seed {seed}");
            }
        }

        [Test]
        public void Zoned_Dungeons_Are_Deterministic()
        {
            Assert.That(FloorSignature(Carve(123, Zoned(2)).grid),
                Is.EqualTo(FloorSignature(Carve(123, Zoned(2)).grid)));
        }

        [Test]
        public void Depth_Zero_Uses_One_Shape_For_The_Whole_Map()
        {
            for (int seed = 0; seed < 40; seed++)
            {
                IReadOnlyList<Room> rooms = Carve(seed, Zoned(0)).rooms;
                int distinct = rooms.Select(r => r.Shape).Distinct().Count();
                Assert.That(distinct, Is.EqualTo(1), $"seed {seed}");
            }
        }

        [Test]
        public void Deeper_Zones_Mix_Shapes()
        {
            bool anyMixed = false;
            for (int seed = 0; seed < 20 && !anyMixed; seed++)
                if (Carve(seed, Zoned(2)).rooms.Select(r => r.Shape).Distinct().Count() > 1)
                    anyMixed = true;

            Assert.That(anyMixed, Is.True);
        }

        [Test]
        public void Zones_Are_Contiguous()
        {
            const int zoneDepth = 2;
            for (int seed = 0; seed < 20; seed++)
            {
                var (_, tree, rooms) = Carve(seed, Zoned(zoneDepth));
                var roomByLeaf = new Dictionary<BspNode, Room>();
                foreach (Room room in rooms) roomByLeaf[room.Leaf] = room;
                AssertZonesUniform(tree.Root, 0, zoneDepth, roomByLeaf, seed);
            }
        }

        // every subtree that forms a zone must carry a single shape across all its leaves
        private static void AssertZonesUniform(
            BspNode node, int depth, int zoneDepth, Dictionary<BspNode, Room> roomByLeaf, int seed)
        {
            if (node.IsLeaf || depth >= zoneDepth)
            {
                ChamberShape? zoneShape = null;
                foreach (Room room in LeavesUnder(node, roomByLeaf))
                {
                    if (zoneShape == null) zoneShape = room.Shape;
                    else Assert.That(room.Shape, Is.EqualTo(zoneShape.Value), $"seed {seed}: mixed shape inside one zone");
                }
                return;
            }

            AssertZonesUniform(node.Left, depth + 1, zoneDepth, roomByLeaf, seed);
            AssertZonesUniform(node.Right, depth + 1, zoneDepth, roomByLeaf, seed);
        }

        private static IEnumerable<Room> LeavesUnder(BspNode node, Dictionary<BspNode, Room> roomByLeaf)
        {
            if (node.IsLeaf)
            {
                if (roomByLeaf.TryGetValue(node, out Room room)) yield return room;
                yield break;
            }

            foreach (Room room in LeavesUnder(node.Left, roomByLeaf)) yield return room;
            foreach (Room room in LeavesUnder(node.Right, roomByLeaf)) yield return room;
        }
    }

    public class ZonedCarverSettingsTests
    {
        [Test]
        public void Rejects_An_Empty_Shape_Mix()
        {
            Assert.Throws<ArgumentException>(() => new RoomCarverSettings(shapeMix: new ChamberShape[0]));
        }

        [Test]
        public void Rejects_Negative_Zone_Depth()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new RoomCarverSettings(shapeZoneDepth: -1));
        }
    }
}
