using System;
using System.Collections.Generic;

namespace ToDonegeon.Generation
{
    // Stage 4 places a boss, chests, and enemies. Chests are biased
    // toward dead-end rooms, and the boss gates the exit.
    public sealed class DungeonPopulator
    {
        private readonly PopulatorSettings _settings;

        public DungeonPopulator(PopulatorSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public IReadOnlyList<Entity> Populate(
            Grid grid, IReadOnlyList<Room> rooms, IReadOnlyList<RoomLink> links,
            Room entrance, Room exit, Random rng)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));
            if (rooms == null) throw new ArgumentNullException(nameof(rooms));
            if (links == null) throw new ArgumentNullException(nameof(links));
            if (rng == null) throw new ArgumentNullException(nameof(rng));

            var entities = new List<Entity>();
            var occupied = new HashSet<Coord>();

            // The entrance and exit tiles are kept clear of content.
            if (entrance != null) occupied.Add(entrance.Center);
            if (exit != null) occupied.Add(exit.Center);

            Room bossRoom = (exit != null && exit != entrance) ? exit : null;
            PlaceBoss(grid, bossRoom, occupied, rng, entities);
            PlaceChests(grid, rooms, links, entrance, occupied, rng, entities);
            PlaceEnemies(grid, rooms, entrance, occupied, rng, entities);

            return entities;
        }

        private void PlaceBoss(Grid grid, Room bossRoom, HashSet<Coord> occupied, Random rng, List<Entity> entities)
        {
            if (bossRoom == null) return;
            if (TryPickFloorCell(grid, bossRoom, occupied, rng, out Coord cell))
            {
                entities.Add(new Entity(cell, EntityKind.Boss));
                occupied.Add(cell);
            }
        }

        // Chests prefer dead-end rooms (one corridor) so reward sits off the main path; the
        // entrance room is left empty as a safe start.
        private void PlaceChests(
            Grid grid, IReadOnlyList<Room> rooms, IReadOnlyList<RoomLink> links, Room entrance,
            HashSet<Coord> occupied, Random rng, List<Entity> entities)
        {
            if (_settings.MaxChests <= 0) return;

            Dictionary<Room, int> degree = ComputeDegrees(rooms, links);
            var deadEnds = new List<Room>();
            var others = new List<Room>();
            foreach (Room room in rooms)
            {
                if (room == entrance) continue;
                if (degree[room] <= 1) deadEnds.Add(room);
                else others.Add(room);
            }

            int placed = 0;
            placed = PlaceChestsIn(deadEnds, grid, occupied, rng, entities, placed);
            PlaceChestsIn(others, grid, occupied, rng, entities, placed);
        }

        private int PlaceChestsIn(
            List<Room> candidates, Grid grid, HashSet<Coord> occupied, Random rng,
            List<Entity> entities, int placed)
        {
            foreach (Room room in candidates)
            {
                if (placed >= _settings.MaxChests) break;
                if (TryPickFloorCell(grid, room, occupied, rng, out Coord cell))
                {
                    entities.Add(new Entity(cell, EntityKind.Chest));
                    occupied.Add(cell);
                    placed++;
                }
            }
            return placed;
        }

        private void PlaceEnemies(
            Grid grid, IReadOnlyList<Room> rooms, Room entrance,
            HashSet<Coord> occupied, Random rng, List<Entity> entities)
        {
            int cap = _settings.EnemyCapPerRoom;
            if (cap <= 0) return;

            foreach (Room room in rooms)
            {
                if (room == entrance) continue;

                int count = rng.Next(0, cap + 1);
                for (int i = 0; i < count; i++)
                {
                    if (!TryPickFloorCell(grid, room, occupied, rng, out Coord cell)) break;
                    entities.Add(new Entity(cell, EntityKind.Enemy));
                    occupied.Add(cell);
                }
            }
        }

        private static Dictionary<Room, int> ComputeDegrees(IReadOnlyList<Room> rooms, IReadOnlyList<RoomLink> links)
        {
            var degree = new Dictionary<Room, int>(rooms.Count);
            foreach (Room room in rooms) degree[room] = 0;
            foreach (RoomLink link in links)
            {
                if (degree.ContainsKey(link.A)) degree[link.A]++;
                if (degree.ContainsKey(link.B)) degree[link.B]++;
            }
            return degree;
        }

        // Picks a uniformly random free floor cell inside the room, or fails if the room is full.
        private static bool TryPickFloorCell(
            Grid grid, Room room, HashSet<Coord> occupied, Random rng, out Coord cell)
        {
            var candidates = new List<Coord>();
            GridRect bounds = room.Bounds;
            for (int y = bounds.Y; y < bounds.MaxY; y++)
            {
                for (int x = bounds.X; x < bounds.MaxX; x++)
                {
                    var candidate = new Coord(x, y);
                    if (grid[x, y] == TileType.Floor && !occupied.Contains(candidate))
                        candidates.Add(candidate);
                }
            }

            if (candidates.Count == 0)
            {
                cell = default;
                return false;
            }

            cell = candidates[rng.Next(candidates.Count)];
            return true;
        }
    }
}
