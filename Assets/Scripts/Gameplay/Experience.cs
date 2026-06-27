namespace ToDonegeon.Gameplay
{
    // By design, experience comes only from completing real tasks.
    // Prototype: a flat linear curve - every XpPerLevel points is one level
    // to be replaced with a tuned curve for the MVP.
    public sealed class Experience
    {
        public const int XpPerLevel = 100;

        public int Total { get; private set; }

        public int Level => 1 + Total / XpPerLevel;
        public int XpIntoLevel => Total % XpPerLevel;
        public int XpForNextLevel => XpPerLevel;

        public void Add(int amount)
        {
            if (amount > 0) Total += amount;
        }
    }
}
