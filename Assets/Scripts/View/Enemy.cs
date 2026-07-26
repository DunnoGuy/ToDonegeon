using UnityEngine;
using UnityEngine.Tilemaps;
using ToDonegeon.Generation;
using ToDonegeon.Gameplay;

namespace ToDonegeon.View
{
    // An enemy actor. The player can bump it, and the turn director moves it and lets it strike back.
    public sealed class Enemy : MonoBehaviour, IInteractable
    {
        [Header("Character")]
        [SerializeField] private int _maxHealth = 12;
        [SerializeField] private int _attackPower = 3;

        [Header("AI")]
        [SerializeField] private int _sightRange = 8;
        [Range(0f, 1f)]
        [SerializeField] private float _retreatHealthFraction = 0.25f;

        private Character _character;
        private Tilemap _tilemap;
        private Coord _cell;

        public Coord Cell => _cell;
        public int SightRange => _sightRange;
        public float RetreatHealthFraction => _retreatHealthFraction;
        public Character Character { get { EnsureCharacter(); return _character; } }
        public bool IsAlive => Character.IsAlive;

        private void Awake() => EnsureCharacter();

        // fixes the starting cell and the tilemap used to place the enemy in the world
        public void Initialise(Coord cell, Tilemap tilemap)
        {
            _tilemap = tilemap;
            _cell = cell;
            transform.position = CellCenter(cell);
        }

        public void MoveTo(Coord cell)
        {
            _cell = cell;
            transform.position = CellCenter(cell);
        }

        // the enemy's own turn: a one-way hit on the player
        public StrikeOutcome StrikePlayer(PlayerCharacter player) => Combat.Strike(Character, player);

        public InteractionResult Interact(PlayerCharacter player)
        {
            StrikeOutcome outcome = Combat.Strike(player, Character);
            string message = outcome.TargetKilled
                ? $"Slew enemy (dealt {outcome.DamageDealt})"
                : $"Hit enemy for {outcome.DamageDealt}";

            if (outcome.TargetKilled) Destroy(gameObject);
            return new InteractionResult(outcome.TargetKilled, blocksMovement: true, message: message);
        }

        private void EnsureCharacter()
        {
            if (_character == null)
                _character = new Character(Mathf.Max(1, _maxHealth), Mathf.Max(0, _attackPower));
        }

        private Vector3 CellCenter(Coord cell)
        {
            if (_tilemap != null)
                return _tilemap.GetCellCenterWorld(new Vector3Int(cell.X, cell.Y, 0));
            return new Vector3(cell.X + 0.5f, cell.Y + 0.5f, 0f);
        }
    }
}
