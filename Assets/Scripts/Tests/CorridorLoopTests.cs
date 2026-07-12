using System;
using System.Collections.Generic;
using NUnit.Framework;
using ToDonegeon.Generation;
using ToDonegeon.Validation;

namespace ToDonegeon.Tests
{
    public class CorridorLoopTests
    {
        private const int Width = 64;
        private const int Height = 48;

        private static readonly CorridorSettings NoLoops = new CorridorSettings();
        private static readonly CorridorSettings ManyLoops = new CorridorSettings(extraLoopChance: 1f, maxExtraLoops: 4);

        private static (Grid grid, IReadOnlyList<Room> rooms, IReadOnlyList<RoomLink> links) Build(
            int seed, CorridorSettings corridor)
        {
            var rng = new Random(seed);
            BspTree tree = new BspPartitioner(new BspSettings()).Partition(new GridRect(0, 0, Width, Height), rng);
            var grid = new Grid(Width, Height);
            IReadOnlyList<Room> rooms = new RoomCarver(new RoomCarverSettings()).Carve(grid, tree, rng);
            IReadOnlyList<RoomLink> links = new CorridorConnector(corridor).Connect(grid, tree, rooms, rng);
            return (grid, rooms, links);
        }

        private static int Diameter(RoomGraph graph)
        {
            int diameter = 0;
            foreach (Room source in graph.Rooms)
            {
                var distance = new Dictionary<Room, int> { { source, 0 } };
                var queue = new Queue<Room>();
                queue.Enqueue(source);
                while (queue.Count > 0)
                {
                    Room room = queue.Dequeue();
                    int next = distance[room] + 1;
                    foreach (Room neighbour in graph.Neighbours(room))
                    {
                        if (distance.ContainsKey(neighbour)) continue;
                        distance[neighbour] = next;
                        if (next > diameter) diameter = next;
                        queue.Enqueue(neighbour);
                    }
                }
            }
            return diameter;
        }

        [Test]
        public void Loops_Keep_The_Dungeon_Fully_Connected()
        {
            for (int seed = 0; seed < 40; seed++)
            {
                var (grid, rooms, _) = Build(seed, ManyLoops);
                GridConnectivity connectivity = new ConnectivityValidator().ValidateGrid(grid, rooms[0].Center);
                Assert.That(connectivity.IsFullyConnected, Is.True, $"seed {seed}");
            }
        }

        [Test]
        public void Loops_Add_Extra_Corridors()
        {
            for (int seed = 0; seed < 20; seed++)
            {
                int withLoops = Build(seed, ManyLoops).links.Count;
                int backbone = Build(seed, NoLoops).links.Count;
                Assert.That(withLoops, Is.GreaterThan(backbone), $"seed {seed}");
            }
        }

        [Test]
        public void Loops_Do_Not_Strand_Any_Room()
        {
            for (int seed = 0; seed < 20; seed++)
            {
                var (_, rooms, links) = Build(seed, ManyLoops);
                Assert.That(new RoomGraph(rooms, links).IsConnected, Is.True, $"seed {seed}");
            }
        }

        [Test]
        public void Loops_Shorten_The_Longest_Route()
        {
            int withTotal = 0;
            int withoutTotal = 0;
            for (int seed = 0; seed < 40; seed++)
            {
                var withLoops = Build(seed, ManyLoops);
                var withoutLoops = Build(seed, NoLoops);
                int withDiameter = Diameter(new RoomGraph(withLoops.rooms, withLoops.links));
                int withoutDiameter = Diameter(new RoomGraph(withoutLoops.rooms, withoutLoops.links));

                Assert.That(withDiameter, Is.LessThanOrEqualTo(withoutDiameter), $"seed {seed}");
                withTotal += withDiameter;
                withoutTotal += withoutDiameter;
            }

            Assert.That(withTotal, Is.LessThan(withoutTotal), "loops should lower the graph diameter overall");
        }
    }

    public class CorridorSettingsTests
    {
        [Test]
        public void Rejects_Loop_Chance_Outside_Its_Range()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CorridorSettings(extraLoopChance: -0.1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CorridorSettings(extraLoopChance: 1.1f));
        }

        [Test]
        public void Rejects_Negative_Max_Loops()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CorridorSettings(maxExtraLoops: -1));
        }
    }
}
