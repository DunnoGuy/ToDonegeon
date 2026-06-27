using System;

namespace ToDonegeon.Tasks
{
    // A single to-do item. Carries a stable Id so the UI can edit, reorder, and later nest it
    // without breaking references, and is meant to grow (a parent id for subtasks, a project
    // slot) without reshaping this type.
    public sealed class TaskItem
    {
        public Guid Id { get; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public TaskDifficulty Difficulty { get; private set; }
        public bool IsDone { get; private set; }

        public TaskItem(Guid id, string title, TaskDifficulty difficulty, string description = "")
        {
            Id = id;
            Title = Normalize(title);
            Difficulty = difficulty;
            Description = description ?? string.Empty;
        }

        public int XpReward => TaskRewards.XpFor(Difficulty);

        public void Edit(string title, string description)
        {
            Title = Normalize(title);
            Description = description ?? string.Empty;
        }

        public void SetDifficulty(TaskDifficulty difficulty) => Difficulty = difficulty;

        internal bool MarkDone()
        {
            if (IsDone) return false;
            IsDone = true;
            return true;
        }

        private static string Normalize(string title)
            => string.IsNullOrWhiteSpace(title) ? "Untitled task" : title.Trim();
    }
}
