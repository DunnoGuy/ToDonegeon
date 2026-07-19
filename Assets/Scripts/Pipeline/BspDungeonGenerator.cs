using System;
using System.Collections.Generic;
using ToDonegeon.Generation;
using ToDonegeon.Validation;

namespace ToDonegeon.Pipeline
{
    // Manages the whole generation pipeline for a seed.
    // Picks an entrance and a distant exit, and regenerates on the next seed if a run ever fails validation.
    public sealed class BspDungeonGenerator
    {
        public const int DefaultMaxAttempts = 8;

        private readonly BspPartitioner _partitioner;
        private readonly RoomCarver _carver;
        private readonly CorridorConnector _connector;
        private readonly ConnectivityValidator _validator;
        private readonly DungeonPopulator _populator;
        private readonly int _maxAttempts;

        public BspDungeonGenerator(
            BspSettings bspSettings = null,
            RoomCarverSettings carverSettings = null,
            PopulatorSettings populatorSettings = null,
            CorridorSettings corridorSettings = null,
            int maxAttempts = DefaultMaxAttempts)
        {
            if (maxAttempts < 1)
                throw new ArgumentOutOfRangeException(nameof(maxAttempts), "Must be at least 1.");

            _partitioner = new BspPartitioner(bspSettings ?? new BspSettings());
            _carver = new RoomCarver(carverSettings ?? new RoomCarverSettings());
            _connector = new CorridorConnector(corridorSettings);
            _validator = new ConnectivityValidator();
            _populator = new DungeonPopulator(populatorSettings ?? new PopulatorSettings());
            _maxAttempts = maxAttempts;
        }

        // returns the first valid result
        public Dungeon Generate(int width, int height, int seed)
        {
            Dungeon dungeon = null;
            for (int attempt = 0; attempt < _maxAttempts; attempt++)
            {
                dungeon = GenerateOnce(width, height, seed + attempt, attempt + 1);
                if (dungeon.IsValid) return dungeon;
            }

            return dungeon;
        }

        private Dungeon GenerateOnce(int width, int height, int seed, int attempt)
        {
            var rng = new Random(seed);
            var bounds = new GridRect(0, 0, width, height);

            BspTree tree = _partitioner.Partition(bounds, rng);
            var grid = new Grid(width, height);
            IReadOnlyList<Room> rooms = _carver.Carve(grid, tree, rng);
            IReadOnlyList<RoomLink> links = _connector.Connect(grid, tree, rooms, rng);

            Room entrance = rooms.Count > 0 ? rooms[0] : null;
            Room exit = FarthestRoom(rooms, entrance);
            MarkEndpoints(grid, entrance, exit);

            var graph = new RoomGraph(rooms, links);
            Coord start = entrance?.Center ?? Coord.Zero;
            GridConnectivity connectivity = _validator.ValidateGrid(grid, start);
            bool entranceExitConnected = entrance != null && exit != null && graph.AreConnected(entrance, exit);

            // Populated last, over the validated geometry, so placement can assume traversability
            IReadOnlyList<Entity> entities = _populator.Populate(grid, rooms, links, entrance, exit, rng);

            return new Dungeon(
                seed, grid, tree, rooms, links, graph, entrance, exit, connectivity, entranceExitConnected,
                entities, attempt);
        }

        private static void MarkEndpoints(Grid grid, Room entrance, Room exit)
        {
            if (entrance != null) grid[entrance.Center] = TileType.Entrance;
            if (exit != null && exit != entrance) grid[exit.Center] = TileType.Exit;
        }

        // The room with the farthest center (Manhattan) from the entrance.
        private static Room FarthestRoom(IReadOnlyList<Room> rooms, Room from)
        {
            if (from == null) return null;

            Room farthest = from;
            int best = -1;
            foreach (Room room in rooms)
            {
                int distance = Coord.ManhattanDistance(from.Center, room.Center);
                if (distance > best)
                {
                    best = distance;
                    farthest = room;
                }
            }

            return farthest;
        }
    }
}
