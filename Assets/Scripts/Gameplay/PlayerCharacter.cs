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

        public PlayerCharacter(int maxHealth, int attackPower) : base(maxHealth, attackPower)
        {
        }

        public void AddGold(int amount)
        {
            if (amount > 0) Gold += amount;
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
