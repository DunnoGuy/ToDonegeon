using System;
using System.Collections.Generic;

namespace ToDonegeon.Tasks
{
    // Ordered task list
    public sealed class TaskList
    {
        private readonly List<TaskItem> _items = new List<TaskItem>();

        public IReadOnlyList<TaskItem> Items => _items;
        public int Count => _items.Count;

        public TaskItem Add(string title, TaskDifficulty difficulty, string description = "")
        {
            var task = new TaskItem(Guid.NewGuid(), title, difficulty, description);
            _items.Add(task);
            return task;
        }

        public bool Remove(Guid id) => _items.RemoveAll(t => t.Id == id) > 0;

        public TaskItem Get(Guid id) => _items.Find(t => t.Id == id);

        public int Complete(Guid id)
        {
            TaskItem task = Get(id);
            if (task == null || !task.MarkDone()) return 0;
            return task.XpReward;
        }

        public void Move(Guid id, int newIndex)
        {
            int current = _items.FindIndex(t => t.Id == id);
            if (current < 0) return;

            TaskItem task = _items[current];
            _items.RemoveAt(current);

            if (newIndex < 0) newIndex = 0;
            if (newIndex > _items.Count) newIndex = _items.Count;
            _items.Insert(newIndex, task);
        }
    }
}
