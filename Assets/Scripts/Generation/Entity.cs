namespace ToDonegeon.Generation
{
    // A piece of any placed content (enemy, chest, boss, etc)
    public sealed class Entity
    {
        public Coord Position { get; }
        public EntityKind Kind { get; }
        public CombatStats? Stats { get; }

        public Entity(Coord position, EntityKind kind, CombatStats? stats = null)
        {
            Position = position;
            Kind = kind;
            Stats = stats;
        }
    }
}
