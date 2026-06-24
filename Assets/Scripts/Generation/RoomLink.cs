namespace ToDonegeon.Generation
{
    // a corridor connection between two rooms. These become the edges of the room graph
    // used by the connectivity validator.
    public sealed class RoomLink
    {
        public Room A { get; }
        public Room B { get; }

        public RoomLink(Room a, Room b)
        {
            A = a ?? throw new System.ArgumentNullException(nameof(a));
            B = b ?? throw new System.ArgumentNullException(nameof(b));
        }
    }
}
