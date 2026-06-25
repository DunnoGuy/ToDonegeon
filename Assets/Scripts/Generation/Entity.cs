namespace ToDonegeon.Generation
{
    // A piece of any placed content (enemy, chest, boss, etc).
    public sealed class Entity
    {
        public Coord Position { get; }
        public EntityKind Kind { get; }

        public Entity(Coord position, EntityKind kind)
        {
            Position = position;
            Kind = kind;
        }
    }
}
