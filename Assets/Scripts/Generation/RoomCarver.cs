using System;
using System.Collections.Generic;

namespace ToDonegeon.Generation
{
    // stage 2 of the pipeline (carve rooms): for each BSP leaf, writes a Floor room into the
    // grid and returns the rooms
    public sealed class RoomCarver
    {
        private readonly RoomCarverSettings _settings;

        public RoomCarver(RoomCarverSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public IReadOnlyList<Room> Carve(Grid grid, BspTree tree, Random rng)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));
            if (tree == null) throw new ArgumentNullException(nameof(tree));
            if (rng == null) throw new ArgumentNullException(nameof(rng));

            Dictionary<BspNode, ChamberShape> zoneShapes = BuildZoneShapes(tree, rng);

            var rooms = new List<Room>(tree.Leaves.Count);
            foreach (BspNode leaf in tree.Leaves)
            {
                ChamberShape shape = zoneShapes != null && zoneShapes.TryGetValue(leaf, out ChamberShape zoned)
                    ? zoned
                    : _settings.Shape;
                if (TryCarve(grid, leaf, shape, rng, out Room room))
                    rooms.Add(room);
            }

            return rooms;
        }

        private bool TryCarve(Grid grid, BspNode leaf, ChamberShape shape, Random rng, out Room room)
        {
            switch (shape)
            {
                case ChamberShape.Cave:
                    return TryCarveCave(grid, leaf, rng, out room);
                case ChamberShape.RandomWalk:
                    return TryCarveRandomWalk(grid, leaf, rng, out room);
                default:
                    return TryCarveRectangle(grid, leaf, rng, out room);
            }
        }

        // Draws one shape per BSP subtree down to the zone depth and hands it to every leaf below.
        // Null when no mix is configured.
        private Dictionary<BspNode, ChamberShape> BuildZoneShapes(BspTree tree, Random rng)
        {
            IReadOnlyList<ChamberShape> mix = _settings.ShapeMix;
            if (mix == null || mix.Count == 0) return null;

            var shapes = new Dictionary<BspNode, ChamberShape>();
            AssignZone(tree.Root, 0, mix, rng, shapes);
            return shapes;
        }

        private void AssignZone(
            BspNode node, int depth, IReadOnlyList<ChamberShape> mix, Random rng,
            Dictionary<BspNode, ChamberShape> shapes)
        {
            if (node.IsLeaf || depth >= _settings.ShapeZoneDepth)
            {
                AssignLeaves(node, mix[rng.Next(mix.Count)], shapes);
                return;
            }

            AssignZone(node.Left, depth + 1, mix, rng, shapes);
            AssignZone(node.Right, depth + 1, mix, rng, shapes);
        }

        private static void AssignLeaves(BspNode node, ChamberShape shape, Dictionary<BspNode, ChamberShape> shapes)
        {
            if (node.IsLeaf)
            {
                shapes[node] = shape;
                return;
            }

            AssignLeaves(node.Left, shape, shapes);
            AssignLeaves(node.Right, shape, shapes);
        }

        private bool TryCarveRectangle(Grid grid, BspNode leaf, Random rng, out Room room)
        {
            GridRect region = leaf.Region;

            int width = RoomExtent(region.Width, rng);
            int height = RoomExtent(region.Height, rng);
            if (width <= 0 || height <= 0)
            {
                room = null;
                return false;
            }

            int x = region.X + _settings.EdgePadding + Jitter(region.Width, width, rng);
            int y = region.Y + _settings.EdgePadding + Jitter(region.Height, height, rng);

            var bounds = new GridRect(x, y, width, height);
            grid.Fill(bounds, TileType.Floor);

            room = new Room(bounds, leaf);
            return true;
        }

        // grows a cellular-automata cave inside the padded leaf
        private bool TryCarveCave(Grid grid, BspNode leaf, Random rng, out Room room)
        {
            if (!TryPaddedRegion(leaf.Region, out GridRect region))
            {
                room = null;
                return false;
            }

            CaveResult cave = new CellularAutomata(_settings.CaveSettings).Fill(grid, region, rng);
            room = new Room(cave.FloorBounds, leaf, cave.Anchor, ChamberShape.Cave);
            return true;
        }

        // carves a random-walk passage inside the padded leaf
        private bool TryCarveRandomWalk(Grid grid, BspNode leaf, Random rng, out Room room)
        {
            if (!TryPaddedRegion(leaf.Region, out GridRect region))
            {
                room = null;
                return false;
            }

            WalkResult walk = new RandomWalk(_settings.WalkSettings).Carve(grid, region, rng);
            room = new Room(walk.FloorBounds, leaf, walk.Anchor, ChamberShape.RandomWalk);
            return true;
        }

        private bool TryPaddedRegion(GridRect leafRegion, out GridRect region)
        {
            int pad = _settings.EdgePadding;
            int width = leafRegion.Width - pad * 2;
            int height = leafRegion.Height - pad * 2;
            if (width < 1 || height < 1)
            {
                region = default;
                return false;
            }

            region = new GridRect(leafRegion.X + pad, leafRegion.Y + pad, width, height);
            return true;
        }

        private int RoomExtent(int leafLength, Random rng)
        {
            int available = leafLength - _settings.EdgePadding * 2;
            if (available <= 0) return 0;

            int byRatio = (int)Math.Round(available * _settings.MinFillRatio);
            int desiredMin = Math.Max(_settings.MinRoomSize, byRatio);
            int lower = Math.Min(desiredMin, available);

            return rng.Next(lower, available + 1);
        }

        // random offset that slides the room within whatever slack remains in its padded leaf
        private int Jitter(int leafLength, int roomLength, Random rng)
        {
            int slack = leafLength - _settings.EdgePadding * 2 - roomLength;
            if (slack <= 0) return 0;
            return rng.Next(slack + 1);
        }
    }
}
