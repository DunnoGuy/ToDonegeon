namespace ToDonegeon.Generation
{
    // What the cave shaper produced in a region (an interior floor cell to connect the chamber,
    // the tight bounding box of its floor, and counters used by tests and callers)
    public sealed class CaveResult
    {
        public Coord Anchor { get; }
        public GridRect FloorBounds { get; }
        public int FloorCount { get; }
        public int LargestComponentSize { get; }
        public int ComponentCount { get; }

        public CaveResult(
            Coord anchor, GridRect floorBounds, int floorCount, int largestComponentSize, int componentCount)
        {
            Anchor = anchor;
            FloorBounds = floorBounds;
            FloorCount = floorCount;
            LargestComponentSize = largestComponentSize;
            ComponentCount = componentCount;
        }
    }
}
