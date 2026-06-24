using System;
using System.Collections.Generic;

namespace ToDonegeon.Generation
{
    // The stage 3 of the pipeline (connection)
    // Walks the BSP tree bottom-up and, at every internal node, carves an L-shaped corridor joining a room from
    // its left subtree to a room from its right subtree.
    // Each split is reconnected, so the corridors form a spanning backbone over every room.
    public sealed class CorridorConnector
    {
        public IReadOnlyList<RoomLink> Connect(Grid grid, BspTree tree, IReadOnlyList<Room> rooms, Random rng)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));
            if (tree == null) throw new ArgumentNullException(nameof(tree));
            if (rooms == null) throw new ArgumentNullException(nameof(rooms));
            if (rng == null) throw new ArgumentNullException(nameof(rng));

            var roomByLeaf = new Dictionary<BspNode, Room>(rooms.Count);
            foreach (Room room in rooms) roomByLeaf[room.Leaf] = room;

            var links = new List<RoomLink>();
            ConnectSubtree(tree.Root, grid, roomByLeaf, rng, links);
            return links;
        }

        // connects everything below the node and returns one representative room of the subtree
        private Room ConnectSubtree(
            BspNode node, Grid grid, IReadOnlyDictionary<BspNode, Room> roomByLeaf,
            Random rng, List<RoomLink> links)
        {
            if (node.IsLeaf)
                return roomByLeaf.TryGetValue(node, out Room leafRoom) ? leafRoom : null;

            Room left = ConnectSubtree(node.Left, grid, roomByLeaf, rng, links);
            Room right = ConnectSubtree(node.Right, grid, roomByLeaf, rng, links);

            if (left == null) return right;
            if (right == null) return left;

            CarveLShaped(grid, left.Center, right.Center, rng);
            links.Add(new RoomLink(left, right));

            // Either room represents the now-connected subtree; picks one at random for variety.
            return rng.Next(2) == 0 ? left : right;
        }

        // an axis-aligned L between two points, with the elbow on a randomly chosen side.
        private static void CarveLShaped(Grid grid, Coord a, Coord b, Random rng)
        {
            if (rng.Next(2) == 0)
            {
                CarveHorizontal(grid, a.X, b.X, a.Y);
                CarveVertical(grid, a.Y, b.Y, b.X);
            }
            else
            {
                CarveVertical(grid, a.Y, b.Y, a.X);
                CarveHorizontal(grid, a.X, b.X, b.Y);
            }
        }

        private static void CarveHorizontal(Grid grid, int x0, int x1, int y)
        {
            int from = Math.Min(x0, x1);
            int to = Math.Max(x0, x1);
            for (int x = from; x <= to; x++) Dig(grid, x, y);
        }

        private static void CarveVertical(Grid grid, int y0, int y1, int x)
        {
            int from = Math.Min(y0, y1);
            int to = Math.Max(y0, y1);
            for (int y = from; y <= to; y++) Dig(grid, x, y);
        }

        // Only walls are turned into corridor. Existing floor/corridor is left untouched.
        private static void Dig(Grid grid, int x, int y)
        {
            if (!grid.InBounds(x, y)) return;
            if (grid[x, y] == TileType.Wall) grid[x, y] = TileType.Corridor;
        }
    }
}
