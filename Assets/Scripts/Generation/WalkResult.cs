namespace ToDonegeon.Generation
{
    public sealed class WalkResult
    {
        public Coord Anchor { get; }
        public GridRect FloorBounds { get; }
        public int FloorCount { get; }

        public WalkResult(Coord anchor, GridRect floorBounds, int floorCount)
        {
            Anchor = anchor;
            FloorBounds = floorBounds;
            FloorCount = floorCount;
        }
    }
}
