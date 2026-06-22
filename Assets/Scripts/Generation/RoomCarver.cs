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

            var rooms = new List<Room>(tree.Leaves.Count);
            foreach (BspNode leaf in tree.Leaves)
            {
                if (TryCarve(grid, leaf, rng, out Room room))
                    rooms.Add(room);
            }

            return rooms;
        }

        private bool TryCarve(Grid grid, BspNode leaf, Random rng, out Room room)
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
