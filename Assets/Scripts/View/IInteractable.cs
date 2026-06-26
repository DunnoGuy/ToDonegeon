using ToDonegeon.Gameplay;

namespace ToDonegeon.View
{
    public readonly struct InteractionResult
    {
        public bool Consumed { get; }
        public bool BlocksMovement { get; }
        public string Message { get; }

        public InteractionResult(bool consumed, bool blocksMovement, string message)
        {
            Consumed = consumed;
            BlocksMovement = blocksMovement;
            Message = message;
        }
    }

    public interface IInteractable
    {
        InteractionResult Interact(PlayerCharacter player);
    }
}
