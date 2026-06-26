using System;

namespace ToDonegeon.Gameplay
{
    // A living creature
    public class Character
    {
        public Health Health { get; }
        public int AttackPower { get; protected set; }

        public Character(int maxHealth, int attackPower)
        {
            if (attackPower < 0)
                throw new ArgumentOutOfRangeException(nameof(attackPower), "Attack power must be non-negative.");
            Health = new Health(maxHealth);
            AttackPower = attackPower;
        }

        public bool IsAlive => !Health.IsDead;
    }
}
