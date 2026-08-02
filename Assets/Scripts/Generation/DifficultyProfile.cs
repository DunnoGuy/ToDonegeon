using System;

namespace ToDonegeon.Generation
{
    // Turns a difficulty tier into the stats an enemy or boss spawns with. Each tier above the first
    // adds a flat amount of health and attack; a boss is a same-tier enemy made several times
    // tougher. 
    public static class DifficultyProfile
    {
        // tier-1 baseline foe
        private const int BaseHealth = 12;
        private const int BaseAttack = 3;

        // growth per tier, and how much a boss outclasses a same-tier enemy
        private const int HealthPerTier = 6;
        private const int AttackPerTier = 1;
        private const int BossHealthMultiplier = 4;
        private const int BossAttackBonus = 3;

        public static CombatStats ForEnemy(int tier)
        {
            int steps = StepsAboveBase(tier);
            return new CombatStats(BaseHealth + steps * HealthPerTier, BaseAttack + steps * AttackPerTier);
        }

        public static CombatStats ForBoss(int tier)
        {
            CombatStats enemy = ForEnemy(tier);
            return new CombatStats(enemy.MaxHealth * BossHealthMultiplier, enemy.AttackPower + BossAttackBonus);
        }

        // stats for a placed entity, or null when the kind carries none (chests)
        public static CombatStats? ForKind(EntityKind kind, int tier)
        {
            switch (kind)
            {
                case EntityKind.Enemy: return ForEnemy(tier);
                case EntityKind.Boss: return ForBoss(tier);
                default: return null;
            }
        }

        private static int StepsAboveBase(int tier)
        {
            if (tier < 1) throw new ArgumentOutOfRangeException(nameof(tier), "Tier must be at least 1.");
            return tier - 1;
        }
    }
}
