namespace ToDonegeon.Tasks
{
    // Self-assigned difficulty of a real-life task; determines the experience it grants on completion.
    public enum TaskDifficulty : byte
    {
        Trivial = 0,
        Easy = 1,
        Medium = 2,
        Hard = 3,
        Epic = 4
    }
}
