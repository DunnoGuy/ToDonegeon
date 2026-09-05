using System;

namespace ToDonegeon.Gameplay
{
    public sealed class Health
    {
        public int Max { get; private set; }
        public int Current { get; private set; }

        public Health(int max)
        {
            if (max < 1) throw new ArgumentOutOfRangeException(nameof(max), "Max health must be at least 1.");
            Max = max;
            Current = max;
        }

        public bool IsDead => Current <= 0;

        // raised with the amount actually lost whenever damage is taken
        public event Action<int> Damaged;

        public int TakeDamage(int amount)
        {
            if (amount <= 0) return 0;
            int before = Current;
            Current = Math.Max(0, Current - amount);
            int dealt = before - Current;
            if (dealt > 0) Damaged?.Invoke(dealt);
            return dealt;
        }

        public int Heal(int amount)
        {
            if (amount <= 0) return 0;
            int before = Current;
            Current = Math.Min(Max, Current + amount);
            return Current - before;
        }

        public void RaiseMax(int amount)
        {
            if (amount <= 0) return;
            Max += amount;
            Current += amount;
        }

        public void ResetToFull() => Current = Max;
    }
}
