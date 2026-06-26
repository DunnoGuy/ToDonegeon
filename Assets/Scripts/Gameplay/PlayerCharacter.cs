namespace ToDonegeon.Gameplay
{
    // A Character controlled by the player
    public sealed class PlayerCharacter : Character
    {
        public int Gold { get; private set; }

        public PlayerCharacter(int maxHealth, int attackPower) : base(maxHealth, attackPower)
        {
        }

        public void AddGold(int amount)
        {
            if (amount > 0) Gold += amount;
        }
    }
}
