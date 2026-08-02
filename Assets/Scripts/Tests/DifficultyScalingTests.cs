using System;
using NUnit.Framework;
using ToDonegeon.Generation;

namespace ToDonegeon.Tests
{
    public class CombatStatsTests
    {
        [Test]
        public void Rejects_Health_Below_One()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CombatStats(0, 3));
        }

        [Test]
        public void Rejects_Negative_Attack()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CombatStats(10, -1));
        }
    }

    public class DifficultyProfileTests
    {
        [Test]
        public void Tier_One_Is_The_Baseline_Foe()
        {
            CombatStats stats = DifficultyProfile.ForEnemy(1);

            Assert.That(stats.MaxHealth, Is.EqualTo(12));
            Assert.That(stats.AttackPower, Is.EqualTo(3));
        }

        [Test]
        public void Enemy_Stats_Grow_Each_Tier()
        {
            CombatStats tier1 = DifficultyProfile.ForEnemy(1);
            CombatStats tier4 = DifficultyProfile.ForEnemy(4);

            Assert.That(tier4.MaxHealth, Is.GreaterThan(tier1.MaxHealth));
            Assert.That(tier4.AttackPower, Is.GreaterThan(tier1.AttackPower));
        }

        [Test]
        public void Enemy_Growth_Is_Linear_In_The_Tier()
        {
            // +6 health and +1 attack per tier above the first
            CombatStats tier3 = DifficultyProfile.ForEnemy(3);

            Assert.That(tier3.MaxHealth, Is.EqualTo(12 + 2 * 6));
            Assert.That(tier3.AttackPower, Is.EqualTo(3 + 2 * 1));
        }

        [Test]
        public void Boss_Is_Tougher_Than_A_Same_Tier_Enemy()
        {
            CombatStats enemy = DifficultyProfile.ForEnemy(2);
            CombatStats boss = DifficultyProfile.ForBoss(2);

            Assert.That(boss.MaxHealth, Is.GreaterThan(enemy.MaxHealth));
            Assert.That(boss.AttackPower, Is.GreaterThan(enemy.AttackPower));
        }

        [Test]
        public void Boss_Multiplies_Health_And_Adds_Attack()
        {
            // a boss is x4 health and +3 attack over the same-tier enemy
            CombatStats enemy = DifficultyProfile.ForEnemy(1);
            CombatStats boss = DifficultyProfile.ForBoss(1);

            Assert.That(boss.MaxHealth, Is.EqualTo(enemy.MaxHealth * 4));
            Assert.That(boss.AttackPower, Is.EqualTo(enemy.AttackPower + 3));
        }

        [Test]
        public void For_Kind_Gives_No_Stats_To_A_Chest()
        {
            Assert.That(DifficultyProfile.ForKind(EntityKind.Chest, 1), Is.Null);
        }

        [Test]
        public void For_Kind_Matches_The_Enemy_And_Boss_Helpers()
        {
            Assert.That(DifficultyProfile.ForKind(EntityKind.Enemy, 3), Is.EqualTo(DifficultyProfile.ForEnemy(3)));
            Assert.That(DifficultyProfile.ForKind(EntityKind.Boss, 3), Is.EqualTo(DifficultyProfile.ForBoss(3)));
        }

        [Test]
        public void Tier_Below_One_Is_Rejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => DifficultyProfile.ForEnemy(0));
        }
    }
}
