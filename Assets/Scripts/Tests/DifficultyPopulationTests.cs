using System.Collections.Generic;
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

        private static List<CombatStats> Enemies(Dungeon dungeon)
            => dungeon.Entities.Where(e => e.Kind == EntityKind.Enemy).Select(e => e.Stats.Value).ToList();

        [Test]
        public void Chests_Carry_No_Combat_Stats()
        {
            Dungeon dungeon = Generate(9, tier: 1);
            foreach (Entity chest in dungeon.Entities.Where(e => e.Kind == EntityKind.Chest))
                Assert.That(chest.Stats, Is.Null);
        }

        [Test]
        public void Enemy_Strength_Stays_Within_The_Floor_Ramp_Band()
        {
            const int tier = 2;
            int floorMin = DifficultyProfile.Default.ForEnemy(tier).MaxHealth;
            int floorMax = DifficultyProfile.Default.ForEnemy(tier + DungeonPopulator.IntraDungeonRampTiers).MaxHealth;

            foreach (CombatStats stats in Enemies(Generate(7, tier)))
                Assert.That(stats.MaxHealth, Is.InRange(floorMin, floorMax));
        }

        [Test]
        public void Difficulty_Ramps_Within_A_Floor()
        {
            // at least one floor puts weaker and stronger foes on the same map
            int rampedFloors = 0;
            for (int seed = 0; seed < 12; seed++)
            {
                int distinct = Enemies(Generate(seed, tier: 2)).Select(s => s.MaxHealth).Distinct().Count();
                if (distinct > 1) rampedFloors++;
            }

            Assert.That(rampedFloors, Is.GreaterThan(0));
        }

        [Test]
        public void A_Deeper_Floor_Outclasses_A_Shallower_One()
        {
            int weakestDeep = Enemies(Generate(11, tier: 4)).Min(s => s.MaxHealth);
            int strongestShallow = Enemies(Generate(11, tier: 1)).Max(s => s.MaxHealth);

            Assert.That(weakestDeep, Is.GreaterThan(strongestShallow));
        }

        [Test]
        public void The_Boss_Is_Never_Weaker_Than_The_Floor_And_Beats_Every_Enemy()
        {
            Dungeon dungeon = Generate(3, tier: 1);
            CombatStats boss = dungeon.Entities.First(e => e.Kind == EntityKind.Boss).Stats.Value;

            Assert.That(boss.MaxHealth, Is.GreaterThanOrEqualTo(DifficultyProfile.Default.ForBoss(1).MaxHealth));
            foreach (CombatStats enemy in Enemies(dungeon))
                Assert.That(boss.MaxHealth, Is.GreaterThan(enemy.MaxHealth));
        }
    }
}
