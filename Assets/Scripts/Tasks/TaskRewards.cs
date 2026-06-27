namespace ToDonegeon.Tasks
{
    // Prototype: fixed XP per difficulty; to be reworked for the MVP.
    public static class TaskRewards
    {
        public const int TrivialXp = 5;
        public const int EasyXp = 10;
        public const int MediumXp = 25;
        public const int HardXp = 50;
        public const int EpicXp = 100;

        public static int XpFor(TaskDifficulty difficulty)
        {
            switch (difficulty)
            {
                case TaskDifficulty.Trivial: return TrivialXp;
                case TaskDifficulty.Easy: return EasyXp;
                case TaskDifficulty.Medium: return MediumXp;
                case TaskDifficulty.Hard: return HardXp;
                case TaskDifficulty.Epic: return EpicXp;
                default: return 0;
            }
        }
    }
}
