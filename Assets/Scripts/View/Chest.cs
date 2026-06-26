using UnityEngine;
using ToDonegeon.Gameplay;

namespace ToDonegeon.View
{
    // Prototype loot container: grants a random amount of gold; to be reworked for the MVP.
    public sealed class Chest : MonoBehaviour, IInteractable
    {
        [SerializeField] private int _minGold = 10;
        [SerializeField] private int _maxGold = 40;

        public InteractionResult Interact(PlayerCharacter player)
        {
            int gold = Random.Range(_minGold, Mathf.Max(_minGold, _maxGold) + 1);
            player.AddGold(gold);
            Destroy(gameObject);
            return new InteractionResult(consumed: true, blocksMovement: false, message: $"Looted {gold} gold");
        }
    }
}
