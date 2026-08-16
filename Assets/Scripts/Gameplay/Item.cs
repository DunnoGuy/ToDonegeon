using System;

namespace ToDonegeon.Gameplay
{
    // A single piece of loot. Kind decides what using it does, Magnitude how much.
    public readonly struct Item
    {
        public string Name { get; }
        public ItemKind Kind { get; }
        public int Magnitude { get; }

        public Item(string name, ItemKind kind, int magnitude)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("An item needs a name.", nameof(name));
            if (magnitude < 1)
                throw new ArgumentOutOfRangeException(nameof(magnitude), "Magnitude must be at least 1.");

            Name = name;
            Kind = kind;
            Magnitude = magnitude;
        }

        public override string ToString() => $"{Name} (+{Magnitude})";
    }
}
