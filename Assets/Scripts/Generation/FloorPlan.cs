namespace ToDonegeon.Generation
{
    // The generation knobs for one dungeon floor at a given difficulty tier: how large the map is,
    // how deeply it splits into rooms, and how many enemies a room may hold.
    public readonly struct FloorPlan
    {
        public int Tier { get; }
        public int Width { get; }
        public int Height { get; }
        public int MaxBspDepth { get; }
        public int MaxEnemiesPerRoom { get; }

        public FloorPlan(int tier, int width, int height, int maxBspDepth, int maxEnemiesPerRoom)
        {
            Tier = tier;
            Width = width;
            Height = height;
            MaxBspDepth = maxBspDepth;
            MaxEnemiesPerRoom = maxEnemiesPerRoom;
        }
    }
}
