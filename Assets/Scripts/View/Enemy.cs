using UnityEngine;
using ToDonegeon.Gameplay;

namespace ToDonegeon.View
{
    // Minimal interactive enemy for the prototype
    public sealed class Enemy : MonoBehaviour, IInteractable
    {
        [SerializeField] private int _maxHealth = 12;
        [SerializeField] private int _attackPower = 3;

        private Character _character;

        private void Awake() => _character = new Character(Mathf.Max(1, _maxHealth), Mathf.Max(0, _attackPower));

        public InteractionResult Interact(PlayerCharacter player)
        {
            BumpOutcome outcome = Combat.ResolveBump(player, _character);
            string message = outcome.TargetKilled
                ? $"Slew enemy (dealt {outcome.DamageDealt})"
                : $"Hit enemy for {outcome.DamageDealt}; took {outcome.DamageTaken}";

            if (outcome.TargetKilled) Destroy(gameObject);
            return new InteractionResult(outcome.TargetKilled, blocksMovement: true, message: message);
        }
    }
}
