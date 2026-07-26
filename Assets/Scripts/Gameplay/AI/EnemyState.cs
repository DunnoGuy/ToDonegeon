using System;
using ToDonegeon.Generation;

namespace ToDonegeon.Gameplay.AI
{
    // An enemy data for one decision
    public sealed class EnemyState
    {
        public Coord Cell { get; set; }
        public Coord Home { get; }
        public Character Character { get; }
        public int SightRange { get; }

        public float RetreatHealthFraction { get; }

        public EnemyState(Coord cell, Character character, int sightRange, float retreatHealthFraction)
        {
            if (character == null) throw new ArgumentNullException(nameof(character));
            if (sightRange < 1) throw new ArgumentOutOfRangeException(nameof(sightRange), "Must be at least 1.");
            if (retreatHealthFraction < 0f || retreatHealthFraction > 1f)
                throw new ArgumentOutOfRangeException(nameof(retreatHealthFraction), "Must lie in [0, 1].");

            Cell = cell;
            Home = cell;
            Character = character;
            SightRange = sightRange;
            RetreatHealthFraction = retreatHealthFraction;
        }

        public bool IsLowHealth => Character.Health.Current < RetreatHealthFraction * Character.Health.Max;
    }
}
