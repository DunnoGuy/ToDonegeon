using System;

namespace ToDonegeon.Generation
{
    // Health and attack a spawned creature starts with
    public readonly struct CombatStats
    {
        public int MaxHealth { get; }
        public int AttackPower { get; }

        public CombatStats(int maxHealth, int attackPower)
        {
            if (maxHealth < 1)
                throw new ArgumentOutOfRangeException(nameof(maxHealth), "Max health must be at least 1.");
            if (attackPower < 0)
                throw new ArgumentOutOfRangeException(nameof(attackPower), "Attack power must be non-negative.");

            MaxHealth = maxHealth;
            AttackPower = attackPower;
        }

        public override string ToString() => $"HP {MaxHealth} / ATK {AttackPower}";
    }
}
