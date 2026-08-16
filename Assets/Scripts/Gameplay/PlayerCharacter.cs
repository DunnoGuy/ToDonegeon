namespace ToDonegeon.Gameplay
{
    // A Character controlled by the player
    // Prototype: levelling up grants a flat +1 attack and a full heal
    // to be reworked for the MVP.
    public sealed class PlayerCharacter : Character
    {
        public const int AttackPerLevel = 1;

        public int Gold { get; private set; }
        public Experience Experience { get; } = new Experience();
        public Inventory Inventory { get; } = new Inventory();

        public PlayerCharacter(int maxHealth, int attackPower) : base(maxHealth, attackPower)
        {
        }

        public void AddGold(int amount)
        {
            if (amount > 0) Gold += amount;
        }

        // consumes the item at index
        public bool TryUseItem(int index, out Item used)
        {
            if (!Inventory.TryRemoveAt(index, out used)) return false;

            switch (used.Kind)
            {
                case ItemKind.Potion: Health.Heal(used.Magnitude); break;
                case ItemKind.Weapon: AttackPower += used.Magnitude; break;
                case ItemKind.Charm: Health.RaiseMax(used.Magnitude); break;
            }
            return true;
        }

        // Adds experience and applies one LevelUp per level crossed; returns the number gained
        public int GainExperience(int amount)
        {
            if (amount <= 0) return 0;

            int before = Experience.Level;
            Experience.Add(amount);
            int gained = Experience.Level - before;
            for (int i = 0; i < gained; i++) LevelUp();
            return gained;
        }

        private void LevelUp()
        {
            AttackPower += AttackPerLevel;
            Health.ResetToFull();
        }
    }
}
