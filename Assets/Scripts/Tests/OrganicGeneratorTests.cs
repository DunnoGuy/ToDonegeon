using System.Text;
using NUnit.Framework;
using ToDonegeon.Generation;
using ToDonegeon.Pipeline;

namespace ToDonegeon.Tests
{
    public class OrganicGeneratorTests
    {
        private const int Width = 64;
        private const int Height = 48;

        private static RoomCarverSettings Cave(bool repair = true) => new RoomCarverSettings(
            shape: ChamberShape.Cave, caveSettings: new CaveSettings(keepOnlyLargestComponent: repair));

        private static Dungeon Generate(RoomCarverSettings carver, CorridorSettings corridor, int attempts, int seed)
            => new BspDungeonGenerator(null, carver, null, corridor, attempts).Generate(Width, Height, seed);

        private static int CountValid(RoomCarverSettings carver, int attempts, int seeds)
        {
            var generator = new BspDungeonGenerator(null, carver, null, null, attempts);
            int valid = 0;
            for (int seed = 0; seed < seeds; seed++)
                if (generator.Generate(Width, Height, seed).IsValid) valid++;
            return valid;
        }

        private static string TileSignature(Grid grid)
        {
            var sb = new StringBuilder(Width * Height);
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    sb.Append((char)('0' + (int)grid[x, y]));
            return sb.ToString();
        }

        [Test]
        public void Cave_Dungeons_Are_Valid_With_One_Entrance_And_Exit()
        {
            for (int seed = 0; seed < 100; seed++)
            {
                Dungeon dungeon = Generate(Cave(), null, 8, seed);
                Assert.That(dungeon.IsValid, Is.True, $"seed {seed}");
                Assert.That(dungeon.Grid.Count(TileType.Entrance), Is.EqualTo(1), $"seed {seed}");
                Assert.That(dungeon.Grid.Count(TileType.Exit), Is.EqualTo(1), $"seed {seed}");
            }
        }

        [Test]
        public void Random_Walk_Dungeons_Are_Valid()
        {
            var walk = new RoomCarverSettings(shape: ChamberShape.RandomWalk);
            for (int seed = 0; seed < 100; seed++)
                Assert.That(Generate(walk, null, 8, seed).IsValid, Is.True, $"seed {seed}");
        }

        [Test]
        public void Cave_With_Loops_Is_Valid()
        {
            var loops = new CorridorSettings(0.5f, 3);
            for (int seed = 0; seed < 100; seed++)
                Assert.That(Generate(Cave(), loops, 8, seed).IsValid, Is.True, $"seed {seed}");
        }

        [Test]
        public void Organic_Generation_Is_Deterministic()
        {
            var loops = new CorridorSettings(0.5f, 3);
            string first = TileSignature(Generate(Cave(), loops, 8, 314).Grid);
            string second = TileSignature(Generate(Cave(), loops, 8, 314).Grid);
            Assert.That(first, Is.EqualTo(second));
        }

        // With pockets kept (the flood-fill repair off) some raw layouts strand cells, so the
        // regenerate-on-failure loop has to retry - the point the report says it becomes load bearing.
        [Test]
        public void Regenerate_Loop_Recovers_Layouts_That_Repair_Off_Leaves_Stranded()
        {
            const int seeds = 200;
            int rawValid = CountValid(Cave(repair: false), attempts: 1, seeds: seeds);
            int retriedValid = CountValid(Cave(repair: false), attempts: 8, seeds: seeds);

            Assert.That(rawValid, Is.LessThan(seeds), "raw single-attempt validity should fall below 100%");
            Assert.That(retriedValid, Is.GreaterThan(rawValid), "retrying should recover more layouts");
        }
    }
}
