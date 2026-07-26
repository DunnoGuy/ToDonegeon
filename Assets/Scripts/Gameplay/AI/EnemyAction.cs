using ToDonegeon.Generation;

namespace ToDonegeon.Gameplay.AI
{
    public enum EnemyActionKind { Wait, Move, Attack }

    public readonly struct EnemyAction
    {
        public EnemyActionKind Kind { get; }

        // the destination for Move or Attack
        public Coord Target { get; }

        private EnemyAction(EnemyActionKind kind, Coord target)
        {
            Kind = kind;
            Target = target;
        }

        public static EnemyAction Wait() => new EnemyAction(EnemyActionKind.Wait, Coord.Zero);
        public static EnemyAction Move(Coord to) => new EnemyAction(EnemyActionKind.Move, to);
        public static EnemyAction Attack(Coord playerCell) => new EnemyAction(EnemyActionKind.Attack, playerCell);
    }
}
