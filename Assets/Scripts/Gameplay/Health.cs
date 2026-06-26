using System;

namespace ToDonegeon.Gameplay
{
    public sealed class Health
    {
        public int Max { get; }
        public int Current { get; private set; }

        public Health(int max)
        {
            if (max < 1) throw new ArgumentOutOfRangeException(nameof(max), "Max health must be at least 1.");
            Max = max;
            Current = max;
        }

        public bool IsDead => Current <= 0;

        public int TakeDamage(int amount)
        {
            if (amount <= 0) return 0;
            int before = Current;
            Current = Math.Max(0, Current - amount);
            return before - Current;
        }

        public int Heal(int amount)
        {
            if (amount <= 0) return 0;
            int before = Current;
            Current = Math.Min(Max, Current + amount);
            return Current - before;
        }

        public void ResetToFull() => Current = Max;
    }
}
