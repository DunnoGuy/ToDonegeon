using System.Text;
using NUnit.Framework;
using ToDonegeon.Generation;
using ToDonegeon.Pipeline;

namespace ToDonegeon.Tests
{
    public class DungeonGeneratorTests
    {
        private const int Width = 64;
        private const int Height = 48;

        private static Dungeon Generate(int seed) => new BspDungeonGenerator().Generate(Width, Height, seed);

        private static string TileSignature(Grid grid)
        {
            var sb = new StringBuilder(Width * Height);
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    sb.Append((char)('0' + (int)grid[x, y]));
            return sb.ToString();
        }

        [Test]
        public void Same_Seed_Produces_Identical_Dungeon()
        {
            Dungeon a = Generate(2024);
            Dungeon b = Generate(2024);

            Assert.That(a.Seed, Is.EqualTo(b.Seed));
            Assert.That(TileSignature(a.Grid), Is.EqualTo(TileSignature(b.Grid)));
        }

        [Test]
        public void Generated_Dungeon_Is_Always_Valid()
        {
            for (int seed = 0; seed < 50; seed++)
            {
                Dungeon dungeon = Generate(seed);
                Assert.That(dungeon.IsValid, Is.True, $"seed {seed} should be valid");
                Assert.That(dungeon.Connectivity.IsFullyConnected, Is.True, $"seed {seed} grid connected");
                Assert.That(dungeon.EntranceExitConnected, Is.True, $"seed {seed} entrance reaches exit");
            }
        }

        [Test]
        public void Marks_Exactly_One_Entrance_And_One_Exit()
        {
            Dungeon dungeon = Generate(3);

            Assert.That(dungeon.Grid.Count(TileType.Entrance), Is.EqualTo(1));
            Assert.That(dungeon.Grid.Count(TileType.Exit), Is.EqualTo(1));
        }

        [Test]
        public void Entrance_Is_The_First_Room_And_Walkable()
        {
            Dungeon dungeon = Generate(3);

            Assert.That(dungeon.Entrance, Is.SameAs(dungeon.Rooms[0]));
            Assert.That(dungeon.Grid[dungeon.Entrance.Center], Is.EqualTo(TileType.Entrance));
            Assert.That(dungeon.Grid.IsWalkable(dungeon.Exit.Center), Is.True);
        }

        [Test]
        public void Result_Bundle_Is_Consistent()
        {
            Dungeon dungeon = Generate(11);

            Assert.That(dungeon.Grid.Width, Is.EqualTo(Width));
            Assert.That(dungeon.Grid.Height, Is.EqualTo(Height));
            Assert.That(dungeon.Rooms.Count, Is.EqualTo(dungeon.Tree.Leaves.Count));
            Assert.That(dungeon.Links.Count, Is.EqualTo(dungeon.Rooms.Count - 1));
            Assert.That(dungeon.Graph.RoomCount, Is.EqualTo(dungeon.Rooms.Count));
        }

        [Test]
        public void Seed_Is_Recorded()
        {
            Assert.That(Generate(987654).Seed, Is.EqualTo(987654));
        }
    }
}
