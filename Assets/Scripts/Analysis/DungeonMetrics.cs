using System.Collections.Generic;
using ToDonegeon.Generation;
using ToDonegeon.Validation;
using ToDonegeon.Pipeline;

namespace ToDonegeon.Analysis
{
    // Per-dungeon expressive-range metrics.
    public sealed class DungeonMetrics
    {
        public int Seed { get; }
        public int RoomCount { get; }
        public float RoomDensity { get; }       // room footprint / grid area
        public float WalkableDensity { get; }   // walkable cells / grid area
        public float CorridorShare { get; }     // corridor cells / walkable cells
        public float Linearity { get; }         // graph diameter / (rooms - 1); 1 = a single chain
        public float Difficulty { get; }        // summed enemy and boss threat; higher = harder
        public bool IsValid { get; }

        public DungeonMetrics(int seed, int roomCount, float roomDensity, float walkableDensity,
            float corridorShare, float linearity, float difficulty, bool isValid)
        {
            Seed = seed;
            RoomCount = roomCount;
            RoomDensity = roomDensity;
            WalkableDensity = walkableDensity;
            CorridorShare = corridorShare;
            Linearity = linearity;
            Difficulty = difficulty;
            IsValid = isValid;
        }

        public static DungeonMetrics Measure(Dungeon dungeon)
        {
            if (dungeon == null) throw new System.ArgumentNullException(nameof(dungeon));

            Grid grid = dungeon.Grid;
            float total = grid.CellCount;

            int roomCells = 0;
            foreach (Room room in dungeon.Rooms) roomCells += room.Bounds.Area;

            int corridor = grid.Count(TileType.Corridor);
            int walkable = grid.Count(TileType.Floor) + corridor
                         + grid.Count(TileType.Entrance) + grid.Count(TileType.Exit);

            int roomCount = dungeon.Rooms.Count;
            int diameter = roomCount > 1 ? GraphDiameter(dungeon.Graph, dungeon.Rooms) : 0;

            float roomDensity = roomCells / total;
            float walkableDensity = walkable / total;
            float corridorShare = walkable > 0 ? corridor / (float)walkable : 0f;
            float linearity = roomCount > 1 ? diameter / (float)(roomCount - 1) : 0f;
            float difficulty = DungeonDifficulty.Measure(dungeon.Entities).TotalThreat;

            return new DungeonMetrics(dungeon.Seed, roomCount, roomDensity, walkableDensity,
                corridorShare, linearity, difficulty, dungeon.IsValid);
        }

        // Longest shortest-path (in corridors) between any two rooms:
        private static int GraphDiameter(RoomGraph graph, IReadOnlyList<Room> rooms)
        {
            int diameter = 0;
            var distance = new Dictionary<Room, int>();
            var queue = new Queue<Room>();

            foreach (Room source in rooms)
            {
                distance.Clear();
                distance[source] = 0;
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
    }
}
