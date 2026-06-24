using System;
using System.Collections.Generic;
using ToDonegeon.Generation;

namespace ToDonegeon.Validation
{
    // The dungeon's connectivity as a graph. Rooms are nodes; corridors are undirected edges.
    public sealed class RoomGraph
    {
        private readonly Dictionary<Room, List<Room>> _adjacency = new Dictionary<Room, List<Room>>();

        public RoomGraph(IEnumerable<Room> rooms, IEnumerable<RoomLink> links)
        {
            if (rooms == null) throw new ArgumentNullException(nameof(rooms));
            if (links == null) throw new ArgumentNullException(nameof(links));

            foreach (Room room in rooms) AddRoom(room);
            foreach (RoomLink link in links) AddEdge(link.A, link.B);
        }

        public IReadOnlyCollection<Room> Rooms => _adjacency.Keys;
        public int RoomCount => _adjacency.Count;

        public IReadOnlyList<Room> Neighbours(Room room)
        {
            if (!_adjacency.TryGetValue(room, out List<Room> neighbours))
                throw new ArgumentException("Room is not part of this graph.", nameof(room));
            return neighbours;
        }

        // All rooms reachable from the start.
        public IReadOnlyCollection<Room> ReachableFrom(Room start) => BuildReachable(start);

        public bool AreConnected(Room a, Room b)
            => a != null && b != null && BuildReachable(a).Contains(b);

        public bool IsConnected
        {
            get
            {
                if (RoomCount == 0) return true;
                foreach (Room start in _adjacency.Keys)
                    return BuildReachable(start).Count == RoomCount;
                return true;
            }
        }

        private HashSet<Room> BuildReachable(Room start)
        {
            var visited = new HashSet<Room>();
            if (start == null || !_adjacency.ContainsKey(start)) return visited;

            var queue = new Queue<Room>();
            visited.Add(start);
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                Room room = queue.Dequeue();
                foreach (Room neighbour in _adjacency[room])
                    if (visited.Add(neighbour)) queue.Enqueue(neighbour);
            }

            return visited;
        }

        private void AddRoom(Room room)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (!_adjacency.ContainsKey(room)) _adjacency[room] = new List<Room>();
        }

        private void AddEdge(Room a, Room b)
        {
            AddRoom(a);
            AddRoom(b);
            _adjacency[a].Add(b);
            _adjacency[b].Add(a);
        }
    }
}
