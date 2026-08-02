using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ToDonegeon.Generation;
using ToDonegeon.Pipeline;

namespace ToDonegeon.Tests
{
    public class PopulateTests
    {
        private const int Width = 64;
        private const int Height = 48;

        private static Dungeon Generate(int seed, PopulatorSettings populator = null)
            => new BspDungeonGenerator(null, null, populator).Generate(Width, Height, seed);

        private static int CountOf(Dungeon dungeon, EntityKind kind)
            => dungeon.Entities.Count(e => e.Kind == kind);

        private static Room RoomContaining(Dungeon dungeon, Coord cell)
            => dungeon.Rooms.FirstOrDefault(r => r.Bounds.Contains(cell));

        private static Dictionary<Room, int> Degrees(Dungeon dungeon)
        {
            var degree = dungeon.Rooms.ToDictionary(r => r, _ => 0);
            foreach (RoomLink link in dungeon.Links)
            {
                degree[link.A]++;
                degree[link.B]++;
            }
            return degree;
        }

        [Test]
        public void Same_Seed_Produces_Identical_Population()
        {
            Dungeon a = Generate(2024);
            Dungeon b = Generate(2024);

            string Signature(Dungeon d) =>
                string.Join("|", d.Entities.Select(e => $"{e.Kind}:{e.Position}"));

            Assert.That(Signature(a), Is.EqualTo(Signature(b)));
        }

        [Test]
        public void Entities_Only_Stand_On_Floor_Cells()
        {
            Dungeon dungeon = Generate(7);

            foreach (Entity entity in dungeon.Entities)
                Assert.That(dungeon.Grid[entity.Position], Is.EqualTo(TileType.Floor),
                    $"{entity.Kind} at {entity.Position} should be on floor");
        }

        [Test]
        public void No_Two_Entities_Share_A_Cell()
        {
            Dungeon dungeon = Generate(11);

            var positions = dungeon.Entities.Select(e => e.Position).ToList();
            Assert.That(positions.Distinct().Count(), Is.EqualTo(positions.Count));
        }

        [Test]
        public void Exactly_One_Boss_And_It_Sits_In_The_Exit_Room()
        {
            Dungeon dungeon = Generate(3);

            Assert.That(CountOf(dungeon, EntityKind.Boss), Is.EqualTo(1));

            Entity boss = dungeon.Entities.First(e => e.Kind == EntityKind.Boss);
            Assert.That(dungeon.Exit.Bounds.Contains(boss.Position), Is.True);
        }

        [Test]
        public void The_Entrance_Room_Is_Left_Empty()
        {
            Dungeon dungeon = Generate(5);

            foreach (Entity entity in dungeon.Entities)
                Assert.That(dungeon.Entrance.Bounds.Contains(entity.Position), Is.False,
                    "the start room should hold no content");
        }

        [Test]
        public void Chests_Do_Not_Exceed_The_Configured_Maximum()
        {
            var settings = new PopulatorSettings(maxChests: 4);
            Dungeon dungeon = Generate(9, settings);

            Assert.That(CountOf(dungeon, EntityKind.Chest), Is.LessThanOrEqualTo(4));
        }

        [Test]
        public void Chests_Prefer_Dead_End_Rooms_When_Enough_Exist()
        {
            var settings = new PopulatorSettings(maxChests: 3);
            Dungeon dungeon = Generate(7, settings);

            Dictionary<Room, int> degree = Degrees(dungeon);
            int deadEndRooms = degree.Count(kv => kv.Key != dungeon.Entrance && kv.Value <= 1);
            int chests = CountOf(dungeon, EntityKind.Chest);

            // The bias is only guaranteed when there is capacity for it.
            if (deadEndRooms >= chests)
            {
                foreach (Entity chest in dungeon.Entities.Where(e => e.Kind == EntityKind.Chest))
                {
                    Room room = RoomContaining(dungeon, chest.Position);
                    Assert.That(degree[room], Is.LessThanOrEqualTo(1),
                        "chests should be placed in dead-end rooms first");
                }
            }
        }

        [Test]
        public void Denser_Rooms_Yield_More_Enemies()
        {
            int EnemyTotal(int perRoom)
            {
                int total = 0;
                for (int seed = 0; seed < 30; seed++)
                    total += CountOf(Generate(seed, new PopulatorSettings(maxEnemiesPerRoom: perRoom)), EntityKind.Enemy);
                return total;
            }

            Assert.That(EnemyTotal(4), Is.GreaterThan(EnemyTotal(1)));
        }

        [Test]
        public void Entities_Are_Present_In_The_Result_Bundle()
        {
            Dungeon dungeon = Generate(1);
            Assert.That(dungeon.Entities, Is.Not.Null);
            Assert.That(dungeon.Entities.Count, Is.GreaterThan(0));
        }
    }

    public class PopulatorSettingsTests
    {
        [Test]
        public void Rejects_Negative_Chest_Count()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PopulatorSettings(maxChests: -1));
        }

        [Test]
        public void Rejects_Difficulty_Tier_Below_One()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PopulatorSettings(difficultyTier: 0));
        }

        [Test]
        public void Enemy_Cap_Is_The_Per_Room_Maximum()
        {
            Assert.That(new PopulatorSettings(maxEnemiesPerRoom: 2, difficultyTier: 3).EnemyCapPerRoom, Is.EqualTo(2));
        }
    }
}
