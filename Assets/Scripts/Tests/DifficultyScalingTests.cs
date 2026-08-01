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
        public void Tier_One_Enemy_Uses_The_Baseline_Stats()
        {
            CombatStats stats = DifficultyProfile.Default.ForEnemy(1);

            Assert.That(stats.MaxHealth, Is.EqualTo(DifficultyProfile.DefaultBaseHealth));
            Assert.That(stats.AttackPower, Is.EqualTo(DifficultyProfile.DefaultBaseAttack));
        }

        [Test]
        public void Enemy_Stats_Grow_With_The_Tier()
        {
            CombatStats tier1 = DifficultyProfile.Default.ForEnemy(1);
            CombatStats tier4 = DifficultyProfile.Default.ForEnemy(4);

            Assert.That(tier4.MaxHealth, Is.GreaterThan(tier1.MaxHealth));
            Assert.That(tier4.AttackPower, Is.GreaterThan(tier1.AttackPower));
        }

        [Test]
        public void Enemy_Growth_Is_Linear_In_The_Tier()
        {
            var profile = new DifficultyProfile(
                baseHealth: 10, baseAttack: 2, healthPerTier: 5, attackPerTier: 2);

            CombatStats tier3 = profile.ForEnemy(3);

            // two steps above tier 1
            Assert.That(tier3.MaxHealth, Is.EqualTo(10 + 2 * 5));
            Assert.That(tier3.AttackPower, Is.EqualTo(2 + 2 * 2));
        }

        [Test]
        public void Boss_Is_Tougher_Than_An_Enemy_Of_The_Same_Tier()
        {
            CombatStats enemy = DifficultyProfile.Default.ForEnemy(2);
            CombatStats boss = DifficultyProfile.Default.ForBoss(2);

            Assert.That(boss.MaxHealth, Is.GreaterThan(enemy.MaxHealth));
            Assert.That(boss.AttackPower, Is.GreaterThan(enemy.AttackPower));
        }

        [Test]
        public void Boss_Applies_The_Multiplier_And_Bonus()
        {
            var profile = new DifficultyProfile(
                baseHealth: 10, baseAttack: 3, healthPerTier: 0, attackPerTier: 0,
                bossHealthMultiplier: 4, bossAttackBonus: 2);

            CombatStats boss = profile.ForBoss(1);

            Assert.That(boss.MaxHealth, Is.EqualTo(10 * 4));
            Assert.That(boss.AttackPower, Is.EqualTo(3 + 2));
        }

        [Test]
        public void For_Kind_Gives_No_Stats_To_A_Chest()
        {
            Assert.That(DifficultyProfile.Default.ForKind(EntityKind.Chest, 1), Is.Null);
        }

        [Test]
        public void For_Kind_Matches_The_Enemy_And_Boss_Helpers()
        {
            DifficultyProfile profile = DifficultyProfile.Default;

            Assert.That(profile.ForKind(EntityKind.Enemy, 3), Is.EqualTo(profile.ForEnemy(3)));
            Assert.That(profile.ForKind(EntityKind.Boss, 3), Is.EqualTo(profile.ForBoss(3)));
        }

        [Test]
        public void Tier_Below_One_Is_Rejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => DifficultyProfile.Default.ForEnemy(0));
        }

        [Test]
        public void Rejects_A_Boss_Multiplier_Below_One()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DifficultyProfile(bossHealthMultiplier: 0));
        }
    }
}
