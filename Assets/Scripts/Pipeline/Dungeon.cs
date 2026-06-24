using System.Collections.Generic;
using ToDonegeon.Generation;
using ToDonegeon.Validation;

namespace ToDonegeon.Pipeline
{
    // A finished grid
    public sealed class Dungeon
    {
        public int Seed { get; }
        public Grid Grid { get; }
        public BspTree Tree { get; }
        public IReadOnlyList<Room> Rooms { get; }
        public IReadOnlyList<RoomLink> Links { get; }
        public RoomGraph Graph { get; }
        public Room Entrance { get; }
        public Room Exit { get; }
        public GridConnectivity Connectivity { get; }
        public bool EntranceExitConnected { get; }

        public Dungeon(
            int seed, Grid grid, BspTree tree, IReadOnlyList<Room> rooms, IReadOnlyList<RoomLink> links,
            RoomGraph graph, Room entrance, Room exit, GridConnectivity connectivity, bool entranceExitConnected)
        {
            Seed = seed;
            Grid = grid;
            Tree = tree;
            Rooms = rooms;
            Links = links;
            Graph = graph;
            Entrance = entrance;
            Exit = exit;
            Connectivity = connectivity;
            EntranceExitConnected = entranceExitConnected;
        }

        public bool IsValid => Connectivity.IsFullyConnected && EntranceExitConnected;
    }
}
