using System;

namespace ToDonegeon.Generation
{
    // Turns a difficulty tier into the stats an enemy or boss spawns with. Each tier above the first
    // adds a flat amount of health and attack; a boss is a same-tier enemy made several times
    // tougher. Tier 1 reproduces the baseline foe, so the default tier leaves balance unchanged.
    public sealed class DifficultyProfile
    {
        public const int DefaultBaseHealth = 12;
        public const int DefaultBaseAttack = 3;
        public const int DefaultHealthPerTier = 6;
        public const int DefaultAttackPerTier = 1;
        public const int DefaultBossHealthMultiplier = 4;
        public const int DefaultBossAttackBonus = 3;

        private readonly int _baseHealth;
        private readonly int _baseAttack;
        private readonly int _healthPerTier;
        private readonly int _attackPerTier;
        private readonly int _bossHealthMultiplier;
        private readonly int _bossAttackBonus;

        public static DifficultyProfile Default { get; } = new DifficultyProfile();

        public DifficultyProfile(
            int baseHealth = DefaultBaseHealth,
            int baseAttack = DefaultBaseAttack,
            int healthPerTier = DefaultHealthPerTier,
            int attackPerTier = DefaultAttackPerTier,
            int bossHealthMultiplier = DefaultBossHealthMultiplier,
            int bossAttackBonus = DefaultBossAttackBonus)
        {
            if (baseHealth < 1)
                throw new ArgumentOutOfRangeException(nameof(baseHealth), "Base health must be at least 1.");
            if (baseAttack < 0)
                throw new ArgumentOutOfRangeException(nameof(baseAttack), "Base attack must be non-negative.");
            if (healthPerTier < 0)
                throw new ArgumentOutOfRangeException(nameof(healthPerTier), "Health growth must be non-negative.");
            if (attackPerTier < 0)
                throw new ArgumentOutOfRangeException(nameof(attackPerTier), "Attack growth must be non-negative.");
            if (bossHealthMultiplier < 1)
                throw new ArgumentOutOfRangeException(nameof(bossHealthMultiplier), "Boss health multiplier must be at least 1.");
            if (bossAttackBonus < 0)
                throw new ArgumentOutOfRangeException(nameof(bossAttackBonus), "Boss attack bonus must be non-negative.");

            _baseHealth = baseHealth;
            _baseAttack = baseAttack;
            _healthPerTier = healthPerTier;
            _attackPerTier = attackPerTier;
            _bossHealthMultiplier = bossHealthMultiplier;
            _bossAttackBonus = bossAttackBonus;
        }

        public CombatStats ForEnemy(int tier)
        {
            int steps = StepsAboveBase(tier);
            return new CombatStats(
                _baseHealth + steps * _healthPerTier,
                _baseAttack + steps * _attackPerTier);
        }

        public CombatStats ForBoss(int tier)
        {
            CombatStats enemy = ForEnemy(tier);
            return new CombatStats(
                enemy.MaxHealth * _bossHealthMultiplier,
                enemy.AttackPower + _bossAttackBonus);
        }

        // stats for a placed entity, or null when the kind carries none (chests)
        public CombatStats? ForKind(EntityKind kind, int tier)
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
