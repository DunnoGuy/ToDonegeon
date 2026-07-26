using System;
using System.Collections.Generic;
using ToDonegeon.Generation;

namespace ToDonegeon.Gameplay.AI
{
    // Chooses an enemy's turn with a behaviour tree
    public sealed class EnemyBrain
    {
        private static readonly Coord[] Steps =
        {
            Coord.Up, Coord.Down, Coord.Left, Coord.Right,
            Coord.Up + Coord.Left, Coord.Up + Coord.Right,
            Coord.Down + Coord.Left, Coord.Down + Coord.Right
        };

        private readonly BehaviourNode<Decision> _tree;

        public EnemyBrain()
        {
            _tree = new Selector<Decision>(
                new Sequence<Decision>(
                    new Condition<Decision>(IsHurt),
                    new Condition<Decision>(SeesPlayer),
                    new ActionNode<Decision>(Retreat)),
                new Sequence<Decision>(
                    new Condition<Decision>(CanReachPlayer),
                    new ActionNode<Decision>(Attack)),
                new Sequence<Decision>(
                    new Condition<Decision>(SeesPlayer),
                    new ActionNode<Decision>(Pursue)),
                new ActionNode<Decision>(Patrol));
        }

        public EnemyAction Decide(WorldView world, EnemyState self, Random rng)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));
            if (self == null) throw new ArgumentNullException(nameof(self));
            if (rng == null) throw new ArgumentNullException(nameof(rng));

            var decision = new Decision(world, self, rng);
            _tree.Tick(decision);
            return decision.Action;
        }

        private static bool IsHurt(Decision d) => d.Self.IsLowHealth;

        private static bool SeesPlayer(Decision d)
            => LineOfSight.IsVisible(d.World.Grid, d.Self.Cell, d.World.PlayerCell, d.Self.SightRange);

        // the player stands in a cell the enemy could legally step into
        private static bool CanReachPlayer(Decision d)
        {
            Coord step = d.World.PlayerCell - d.Self.Cell;
            return IsSingleStep(step)
                && GridMovement.TryStep(d.World.Grid, d.Self.Cell, step, out Coord landing)
                && landing == d.World.PlayerCell;
        }

        private static NodeStatus Attack(Decision d)
        {
            d.Action = EnemyAction.Attack(d.World.PlayerCell);
            return NodeStatus.Success;
        }

        private static NodeStatus Pursue(Decision d)
        {
            IReadOnlyList<Coord> route = GridPathfinder.FindPath(d.World.Grid, d.Self.Cell, d.World.PlayerCell);
            if (route.Count == 0) return NodeStatus.Failure;

            Coord next = route[0];
            if (d.World.IsBlocked(next)) return NodeStatus.Failure;

            d.Action = EnemyAction.Move(next);
            return NodeStatus.Success;
        }

        // steps to the reachable neighbour furthest from the player
        private static NodeStatus Retreat(Decision d)
        {
            int here = Chebyshev(d.Self.Cell, d.World.PlayerCell);
            Coord best = d.Self.Cell;
            int bestDistance = here;
            foreach (Coord cell in FreeNeighbours(d))
            {
                int distance = Chebyshev(cell, d.World.PlayerCell);
                if (distance > bestDistance)
                {
                    bestDistance = distance;
                    best = cell;
                }
            }

            if (bestDistance <= here) return NodeStatus.Failure;

            d.Action = EnemyAction.Move(best);
            return NodeStatus.Success;
        }

        private static NodeStatus Patrol(Decision d)
        {
            List<Coord> options = FreeNeighbours(d);
            d.Action = options.Count > 0
                ? EnemyAction.Move(options[d.Rng.Next(options.Count)])
                : EnemyAction.Wait();
            return NodeStatus.Success;
        }

        private static List<Coord> FreeNeighbours(Decision d)
        {
            var cells = new List<Coord>();
            foreach (Coord step in Steps)
                if (GridMovement.TryStep(d.World.Grid, d.Self.Cell, step, out Coord to) && !d.World.IsBlocked(to))
                    cells.Add(to);
            return cells;
        }

        private static bool IsSingleStep(Coord step)
            => (step.X != 0 || step.Y != 0) && Math.Abs(step.X) <= 1 && Math.Abs(step.Y) <= 1;

        private static int Chebyshev(Coord a, Coord b) => Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));

        private sealed class Decision
        {
            public WorldView World { get; }
            public EnemyState Self { get; }
            public Random Rng { get; }
            public EnemyAction Action { get; set; }

            public Decision(WorldView world, EnemyState self, Random rng)
            {
                World = world;
                Self = self;
                Rng = rng;
                Action = EnemyAction.Wait();
            }
        }
    }
}
