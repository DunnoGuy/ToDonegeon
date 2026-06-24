using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ToDonegeon.Generation;
using ToDonegeon.Validation;

namespace ToDonegeon.Tests
{
    public class ConnectivityValidatorTests
    {
        private const int Width = 64;
        private const int Height = 48;

        private static (Grid grid, IReadOnlyList<Room> rooms) GenerateDungeon(int seed)
        {
            var rng = new Random(seed);
            BspTree tree = new BspPartitioner(new BspSettings()).Partition(new GridRect(0, 0, Width, Height), rng);
            var grid = new Grid(Width, Height);
            IReadOnlyList<Room> rooms = new RoomCarver(new RoomCarverSettings()).Carve(grid, tree, rng);
            new CorridorConnector().Connect(grid, tree, rooms, rng);
            return (grid, rooms);
        }

        [Test]
        public void Generated_Dungeon_Has_No_Orphaned_Cells()
        {
            var (grid, rooms) = GenerateDungeon(7);

            GridConnectivity result = new ConnectivityValidator().ValidateGrid(grid, rooms[0].Center);

            Assert.That(result.IsFullyConnected, Is.True);
            Assert.That(result.UnreachableCells, Is.Empty);
            Assert.That(result.ReachableCount, Is.EqualTo(result.WalkableCount));
        }

        [Test]
        public void Detects_An_Isolated_Floor_Pocket()
        {
            var grid = new Grid(10, 10);
            grid.Fill(new GridRect(0, 0, 3, 3), TileType.Floor);  // reachable region: 9 cells
            grid.Fill(new GridRect(6, 6, 2, 2), TileType.Floor);  // stranded pocket: 4 cells

            GridConnectivity result = new ConnectivityValidator().ValidateGrid(grid, new Coord(1, 1));

            Assert.That(result.IsFullyConnected, Is.False);
            Assert.That(result.WalkableCount, Is.EqualTo(13));
            Assert.That(result.ReachableCount, Is.EqualTo(9));
            Assert.That(result.UnreachableCount, Is.EqualTo(4));
        }

        [Test]
        public void IsReachable_Tracks_Whether_A_Path_Exists()
        {
            var grid = new Grid(10, 10);
            grid.Fill(new GridRect(0, 0, 3, 3), TileType.Floor);
            grid.Fill(new GridRect(6, 6, 2, 2), TileType.Floor);

            var validator = new ConnectivityValidator();
            Assert.That(validator.IsReachable(grid, new Coord(0, 0), new Coord(2, 2)), Is.True);
            Assert.That(validator.IsReachable(grid, new Coord(0, 0), new Coord(6, 6)), Is.False);
        }

        [Test]
        public void Start_On_A_Wall_Reaches_Nothing()
        {
            var grid = new Grid(5, 5);
            grid.Fill(new GridRect(1, 1, 3, 3), TileType.Floor);

            GridConnectivity result = new ConnectivityValidator().ValidateGrid(grid, new Coord(0, 0)); // a wall

            Assert.That(result.ReachableCount, Is.EqualTo(0));
            Assert.That(result.IsFullyConnected, Is.False);
        }
    }

    public class RoomGraphTests
    {
        private const int Width = 64;
        private const int Height = 48;

        private static (IReadOnlyList<Room> rooms, IReadOnlyList<RoomLink> links) GenerateGraphInputs(int seed)
        {
            var rng = new Random(seed);
            BspTree tree = new BspPartitioner(new BspSettings()).Partition(new GridRect(0, 0, Width, Height), rng);
            var grid = new Grid(Width, Height);
            IReadOnlyList<Room> rooms = new RoomCarver(new RoomCarverSettings()).Carve(grid, tree, rng);
            IReadOnlyList<RoomLink> links = new CorridorConnector().Connect(grid, tree, rooms, rng);
            return (rooms, links);
        }

        [Test]
        public void Graph_Of_A_Generated_Dungeon_Is_Connected()
        {
            var (rooms, links) = GenerateGraphInputs(7);

            var graph = new RoomGraph(rooms, links);

            Assert.That(graph.RoomCount, Is.EqualTo(rooms.Count));
            Assert.That(graph.IsConnected, Is.True);
        }

        [Test]
        public void Entrance_And_Exit_Are_Mutually_Reachable()
        {
            var (rooms, links) = GenerateGraphInputs(7);
            var graph = new RoomGraph(rooms, links);

            Room entrance = rooms[0];
            Room exit = rooms[rooms.Count - 1];

            Assert.That(graph.AreConnected(entrance, exit), Is.True);
            Assert.That(graph.AreConnected(exit, entrance), Is.True);
        }

        [Test]
        public void Dropping_A_Backbone_Edge_Disconnects_The_Graph()
        {
            var (rooms, links) = GenerateGraphInputs(7);
            var fewerLinks = links.Take(links.Count - 1).ToList(); // spanning tree minus one edge

            var graph = new RoomGraph(rooms, fewerLinks);

            Assert.That(graph.IsConnected, Is.False);
        }

        [Test]
        public void A_Single_Room_Is_Trivially_Connected()
        {
            var leaf = new BspNode(new GridRect(0, 0, 6, 6));
            var room = new Room(new GridRect(1, 1, 4, 4), leaf);

            var graph = new RoomGraph(new[] { room }, Array.Empty<RoomLink>());

            Assert.That(graph.RoomCount, Is.EqualTo(1));
            Assert.That(graph.IsConnected, Is.True);
            Assert.That(graph.AreConnected(room, room), Is.True);
        }

        [Test]
        public void Neighbours_Reflect_The_Links()
        {
            var leafA = new BspNode(new GridRect(0, 0, 6, 6));
            var leafB = new BspNode(new GridRect(6, 0, 6, 6));
            var a = new Room(new GridRect(1, 1, 3, 3), leafA);
            var b = new Room(new GridRect(7, 1, 3, 3), leafB);

            var graph = new RoomGraph(new[] { a, b }, new[] { new RoomLink(a, b) });

            Assert.That(graph.Neighbours(a), Does.Contain(b));
            Assert.That(graph.Neighbours(b), Does.Contain(a));
        }
    }
}
