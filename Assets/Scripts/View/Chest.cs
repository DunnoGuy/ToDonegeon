using UnityEngine;
using ToDonegeon.Gameplay;

namespace ToDonegeon.View
{
    // Loot container: grants gold and a rolled item.
    public sealed class Chest : MonoBehaviour, IInteractable
    {
        [SerializeField] private int _minGold = 10;
        [SerializeField] private int _maxGold = 40;

        private static readonly System.Random LootRng = new System.Random();

        public InteractionResult Interact(PlayerCharacter player)
        {
            int gold = Random.Range(_minGold, Mathf.Max(_minGold, _maxGold) + 1);
            player.AddGold(gold);

            Item loot = LootTable.Roll(LootRng);
            player.Inventory.Add(loot);

            Destroy(gameObject);
            return new InteractionResult(consumed: true, blocksMovement: false,
                message: $"Looted {gold} gold and a {loot.Name}");
        }
    }
}
