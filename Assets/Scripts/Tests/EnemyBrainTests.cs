using System;
using System.Collections.Generic;
using NUnit.Framework;
using ToDonegeon.Generation;
using ToDonegeon.Gameplay;
using ToDonegeon.Gameplay.AI;

namespace ToDonegeon.Tests
{
    public class EnemyBrainTests
    {
        private const int Size = 24;
        private static readonly EnemyBrain Brain = new EnemyBrain();

        private static Grid Open() => new Grid(Size, Size, TileType.Floor);

        private static EnemyState Enemy(Coord cell, int currentHp = 20, int maxHp = 20, int sight = 10, float retreat = 0.3f)
        {
            var character = new Character(maxHp, 3);
            if (currentHp < maxHp) character.Health.TakeDamage(maxHp - currentHp);
            return new EnemyState(cell, character, sight, retreat);
        }

        private static WorldView World(Grid grid, Coord player, params Coord[] occupied)
            => new WorldView(grid, player, occupied);

        private static int Chebyshev(Coord a, Coord b) => Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));

        [Test]
        public void Attacks_An_Adjacent_Player()
        {
            EnemyAction action = Brain.Decide(World(Open(), new Coord(6, 5)), Enemy(new Coord(5, 5)), new Random(1));
            Assert.That(action.Kind, Is.EqualTo(EnemyActionKind.Attack));
            Assert.That(action.Target, Is.EqualTo(new Coord(6, 5)));
        }

        [Test]
        public void Pursues_A_Visible_Player_By_Stepping_Closer()
        {
            var self = Enemy(new Coord(3, 3));
            var player = new Coord(9, 3);
            EnemyAction action = Brain.Decide(World(Open(), player), self, new Random(1));
            Assert.That(action.Kind, Is.EqualTo(EnemyActionKind.Move));
            Assert.That(Chebyshev(action.Target, player), Is.LessThan(Chebyshev(self.Cell, player)));
        }

        [Test]
        public void Retreats_When_Hurt_By_Stepping_Away()
        {
            var self = Enemy(new Coord(6, 6), currentHp: 4);
            var player = new Coord(9, 6);
            EnemyAction action = Brain.Decide(World(Open(), player), self, new Random(1));
            Assert.That(action.Kind, Is.EqualTo(EnemyActionKind.Move));
            Assert.That(Chebyshev(action.Target, player), Is.GreaterThan(Chebyshev(self.Cell, player)));
        }

        [Test]
        public void Pursuit_Ignores_The_Random_Source()
        {
            WorldView world = World(Open(), new Coord(9, 3));
            EnemyAction a = Brain.Decide(world, Enemy(new Coord(3, 3)), new Random(1));
            EnemyAction b = Brain.Decide(world, Enemy(new Coord(3, 3)), new Random(999));
            Assert.That(a.Target, Is.EqualTo(b.Target));
        }

        [Test]
        public void Patrols_When_The_Player_Is_Out_Of_Sight()
        {
            WorldView world = World(Open(), new Coord(22, 22));
            var targets = new HashSet<Coord>();
            for (int seed = 0; seed < 20; seed++)
                targets.Add(Brain.Decide(world, Enemy(new Coord(3, 3), sight: 5), new Random(seed)).Target);

            Assert.That(targets.Count, Is.GreaterThan(1));
        }

        [Test]
        public void Never_Moves_Onto_A_Blocked_Cell()
        {
            var player = new Coord(8, 5);
            WorldView world = World(Open(), player, new Coord(6, 5), new Coord(6, 6), new Coord(6, 4));
            for (int seed = 0; seed < 20; seed++)
            {
                EnemyAction action = Brain.Decide(world, Enemy(new Coord(5, 5)), new Random(seed));
                if (action.Kind == EnemyActionKind.Move)
                {
                    Assert.That(world.IsBlocked(action.Target), Is.False, $"seed {seed}");
                    Assert.That(action.Target, Is.Not.EqualTo(player));
                }
            }
        }

        [Test]
        public void Waits_When_Boxed_In()
        {
            Grid grid = Open();
            for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                    if (dx != 0 || dy != 0) grid[5 + dx, 5 + dy] = TileType.Wall;

            EnemyAction action = Brain.Decide(World(grid, new Coord(15, 15)), Enemy(new Coord(5, 5)), new Random(1));
            Assert.That(action.Kind, Is.EqualTo(EnemyActionKind.Wait));
        }

        [Test]
        public void Rejects_Null_Arguments()
        {
            var self = Enemy(new Coord(1, 1));
            WorldView world = World(Open(), new Coord(2, 2));
            Assert.Throws<ArgumentNullException>(() => Brain.Decide(null, self, new Random(1)));
            Assert.Throws<ArgumentNullException>(() => Brain.Decide(world, null, new Random(1)));
            Assert.Throws<ArgumentNullException>(() => Brain.Decide(world, self, null));
        }
    }
}
