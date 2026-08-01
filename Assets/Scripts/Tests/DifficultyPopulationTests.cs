using System.Linq;
using NUnit.Framework;
using ToDonegeon.Generation;
using ToDonegeon.Pipeline;

namespace ToDonegeon.Tests
{
    public class DifficultyPopulationTests
    {
        private const int Width = 64;
        private const int Height = 48;

        private static Dungeon Generate(int seed, int tier)
            => new BspDungeonGenerator(null, null, new PopulatorSettings(difficultyTier: tier))
                .Generate(Width, Height, seed);

        [Test]
        public void Enemies_Spawn_With_The_Tier_Enemy_Stats()
        {
            Dungeon dungeon = Generate(7, tier: 2);
            CombatStats expected = DifficultyProfile.Default.ForEnemy(2);

            var enemies = dungeon.Entities.Where(e => e.Kind == EntityKind.Enemy).ToList();
            Assert.That(enemies, Is.Not.Empty);
            foreach (Entity enemy in enemies)
                Assert.That(enemy.Stats.Value, Is.EqualTo(expected));
        }

        [Test]
        public void The_Boss_Spawns_With_The_Tier_Boss_Stats()
        {
            Dungeon dungeon = Generate(3, tier: 1);
            Entity boss = dungeon.Entities.First(e => e.Kind == EntityKind.Boss);

            Assert.That(boss.Stats.Value, Is.EqualTo(DifficultyProfile.Default.ForBoss(1)));
        }

        [Test]
        public void Chests_Carry_No_Combat_Stats()
        {
            Dungeon dungeon = Generate(9, tier: 1);
            foreach (Entity chest in dungeon.Entities.Where(e => e.Kind == EntityKind.Chest))
                Assert.That(chest.Stats, Is.Null);
        }

        [Test]
        public void Higher_Tier_Stamps_Tougher_Enemies()
        {
            CombatStats low = FirstEnemyStats(Generate(11, tier: 1));
            CombatStats high = FirstEnemyStats(Generate(11, tier: 4));

            Assert.That(high.MaxHealth, Is.GreaterThan(low.MaxHealth));
            Assert.That(high.AttackPower, Is.GreaterThan(low.AttackPower));
        }

        private static CombatStats FirstEnemyStats(Dungeon dungeon)
            => dungeon.Entities.First(e => e.Kind == EntityKind.Enemy).Stats.Value;
    }
}
