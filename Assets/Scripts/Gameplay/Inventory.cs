using System.Collections.Generic;

namespace ToDonegeon.Gameplay
{
    // The items the player is carrying, in the order they were picked up.
    public sealed class Inventory
    {
        private readonly List<Item> _items = new List<Item>();

        public IReadOnlyList<Item> Items => _items;
        public int Count => _items.Count;

        public void Add(Item item) => _items.Add(item);

        // pulls the item at index out of the bag; fails cleanly on a bad index
        public bool TryRemoveAt(int index, out Item item)
        {
            if (index < 0 || index >= _items.Count)
            {
                item = default;
                return false;
            }

            item = _items[index];
            _items.RemoveAt(index);
            return true;
        }
    }
}
