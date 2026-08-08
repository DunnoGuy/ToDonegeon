using System.Collections.Generic;
using NUnit.Framework;
using ToDonegeon.Generation;
using ToDonegeon.Pipeline;

namespace ToDonegeon.Tests
{
    public class DungeonDifficultyTests
    {
        private static Dungeon Generate(int seed, int tier)
            => new BspDungeonGenerator(null, null, new PopulatorSettings(difficultyTier: tier))
                .Generate(64, 48, seed);

        [Test]
        public void Threat_Sums_Health_Times_Attack_Over_Fighters()
        {
            var entities = new List<Entity>
            {
                new Entity(new Coord(0, 0), EntityKind.Enemy, new CombatStats(10, 2)), // 20
                new Entity(new Coord(1, 0), EntityKind.Boss, new CombatStats(40, 5)),  // 200
                new Entity(new Coord(2, 0), EntityKind.Chest)                          // no stats
            };

            DungeonDifficulty difficulty = DungeonDifficulty.Measure(entities);

            Assert.That(difficulty.TotalThreat, Is.EqualTo(20 + 200));
            Assert.That(difficulty.BossThreat, Is.EqualTo(200));
            Assert.That(difficulty.EnemyThreat, Is.EqualTo(20));
            Assert.That(difficulty.EnemyCount, Is.EqualTo(1));
        }

        [Test]
        public void No_Fighters_Means_No_Threat()
        {
            var chestsOnly = new List<Entity> { new Entity(new Coord(0, 0), EntityKind.Chest) };
            Assert.That(DungeonDifficulty.Measure(chestsOnly).TotalThreat, Is.EqualTo(0));
        }

        [Test]
        public void A_Deeper_Floor_Is_More_Threatening()
        {
            long shallow = DungeonDifficulty.Measure(Generate(11, tier: 1).Entities).TotalThreat;
            long deep = DungeonDifficulty.Measure(Generate(11, tier: 4).Entities).TotalThreat;

            Assert.That(deep, Is.GreaterThan(shallow));
        }

        [Test]
        public void The_Boss_Is_A_Real_Share_Of_The_Threat()
        {
            DungeonDifficulty difficulty = DungeonDifficulty.Measure(Generate(3, tier: 1).Entities);

            Assert.That(difficulty.BossThreat, Is.GreaterThan(0));
            Assert.That(difficulty.TotalThreat, Is.GreaterThan(difficulty.BossThreat));
        }
    }
}
