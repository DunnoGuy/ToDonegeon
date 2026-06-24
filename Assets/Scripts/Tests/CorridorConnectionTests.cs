using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using ToDonegeon.Generation;

namespace ToDonegeon.Tests
{
    public class CorridorConnectionTests
    {
        private const int Width = 64;
        private const int Height = 48;

        // full partition -> carve -> connect off one shared rng stream.
        private static (Grid grid, IReadOnlyList<Room> rooms, IReadOnlyList<RoomLink> links) Generate(int seed)
        {
            var rng = new Random(seed);
            BspTree tree = new BspPartitioner(new BspSettings()).Partition(new GridRect(0, 0, Width, Height), rng);
            var grid = new Grid(Width, Height);
            IReadOnlyList<Room> rooms = new RoomCarver(new RoomCarverSettings()).Carve(grid, tree, rng);
            IReadOnlyList<RoomLink> links = new CorridorConnector().Connect(grid, tree, rooms, rng);
            return (grid, rooms, links);
        }

        private static string TileSignature(Grid grid)
        {
            var sb = new StringBuilder(Width * Height);
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    sb.Append((char)('0' + (int)grid[x, y]));
            return sb.ToString();
        }

        // flood fill over walkable cells; returns how many separate walkable cells are reachable.
        private static int ReachableWalkableCount(Grid grid, Coord start)
        {
            var visited = new HashSet<Coord> { start };
            var queue = new Queue<Coord>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                Coord cell = queue.Dequeue();
                foreach (Coord step in Coord.Orthogonal)
                {
                    Coord next = cell + step;
                    if (!visited.Contains(next) && grid.IsWalkable(next))
                    {
                        visited.Add(next);
                        queue.Enqueue(next);
                    }
                }
            }

            return visited.Count;
        }

        [Test]
        public void Same_Seed_Produces_Identical_Dungeon()
        {
            Assert.That(TileSignature(Generate(123).grid), Is.EqualTo(TileSignature(Generate(123).grid)));
        }

        [Test]
        public void Different_Seeds_Produce_Varied_Dungeons()
        {
            var signatures = new HashSet<string>();
            for (int seed = 0; seed < 16; seed++)
                signatures.Add(TileSignature(Generate(seed).grid));

            Assert.That(signatures.Count, Is.GreaterThan(1));
        }

        [Test]
        public void Every_Walkable_Cell_Is_Reachable_From_A_Room()
        {
            var (grid, rooms, _) = Generate(7);
            int totalWalkable = grid.Count(TileType.Floor) + grid.Count(TileType.Corridor);

            int reachable = ReachableWalkableCount(grid, rooms[0].Center);

            Assert.That(reachable, Is.EqualTo(totalWalkable), "corridors should connect the whole dungeon");
        }

        [Test]
        public void Connecting_Adds_Corridors_Without_Removing_Floor()
        {
            var rng = new Random(50);
            BspTree tree = new BspPartitioner(new BspSettings()).Partition(new GridRect(0, 0, Width, Height), rng);
            var grid = new Grid(Width, Height);
            IReadOnlyList<Room> rooms = new RoomCarver(new RoomCarverSettings()).Carve(grid, tree, rng);

            int floorBefore = grid.Count(TileType.Floor);
            new CorridorConnector().Connect(grid, tree, rooms, rng);

            Assert.That(grid.Count(TileType.Floor), Is.EqualTo(floorBefore), "rooms must stay floor");
            Assert.That(grid.Count(TileType.Corridor), Is.GreaterThan(0), "corridors should be carved");
        }

        [Test]
        public void Spanning_Backbone_Has_One_Link_Fewer_Than_Rooms()
        {
            var (_, rooms, links) = Generate(9);
            Assert.That(links.Count, Is.EqualTo(rooms.Count - 1));
        }
    }
}
