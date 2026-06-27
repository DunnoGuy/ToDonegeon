using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using ToDonegeon.Generation;
using ToDonegeon.Gameplay;
using DungeonGrid = ToDonegeon.Generation.Grid;

namespace ToDonegeon.View
{
    // Reads input and drives one player turn: move, attack an enemy, or loot a chest.
    public sealed class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Tilemap _tilemap;
        [SerializeField] private Camera _camera;
        [SerializeField] private EntitySpawner _entitySpawner;

        [Header("Movement")]
        [Tooltip("Seconds for the visual hop between cells; also the minimum time between turns.")]
        [SerializeField] private float _stepDuration = 0.12f;

        [Header("Character")]
        [SerializeField] private int _maxHealth = 30;
        [SerializeField] private int _attackPower = 5;

        private const float CellCenterOffset = 0.5f;
        private const float HudWidth = 240f;
        private const float HudHeight = 92f;
        private const float HudMargin = 10f;

        private DungeonGrid _grid;
        private Coord _cell;
        private bool _placed;

        private PlayerCharacter _character;
        private string _lastMessage;

        private bool _isHopping;
        private Vector3 _hopFrom;
        private Vector3 _hopTo;
        private float _hopElapsed;

        private Queue<Coord> _path; // pending click-to-move route, walked one cell per turn

        // Raised after the player takes a turn (move, attack, or loot); the seam a future turn
        // manager uses to run enemy turns and to gate input while enemies are in sight.
        public event System.Action<Coord> Moved;

        public Coord Cell => _cell;
        public PlayerCharacter Character => _character;

        // Positions the player on a freshly generated grid at full health; called after each regeneration.
        public void Place(DungeonGrid grid, Coord startCell)
        {
            _grid = grid ?? throw new System.ArgumentNullException(nameof(grid));
            _cell = startCell;
            _placed = true;
            _isHopping = false;
            _path = null;
            _lastMessage = null;
            _character.Health.ResetToFull();
            transform.position = CellCenter(startCell);
        }

        private void Awake()
        {
            if (_camera == null) _camera = Camera.main;
            if (_entitySpawner == null) _entitySpawner = FindAnyObjectByType<EntitySpawner>();
            _character = new PlayerCharacter(Mathf.Max(1, _maxHealth), Mathf.Max(0, _attackPower));
        }

        private void Update()
        {
            if (_isHopping)
            {
                Animate();
                return;
            }
            if (!_placed) return;

            if (TryReadKeyboardStep(out Coord step))
            {
                _path = null;
                TryAct(step);
                return;
            }

            if (TryReadClickTarget(out Coord target))
                _path = BuildPath(target);

            StepAlongPath();
        }

        // One turn: move into the target cell, or interact with whatever occupies it.
        private void TryAct(Coord step)
        {
            if (!GridMovement.TryStep(_grid, _cell, step, out Coord target)) return;

            if (_entitySpawner != null && _entitySpawner.TryGetInteractable(target, out IInteractable interactable))
            {
                InteractionResult result = interactable.Interact(_character);
                _lastMessage = result.Message;
                if (result.Consumed) _entitySpawner.Remove(target);

                if (result.BlocksMovement) ConsumeTurnInPlace();
                else CommitMove(target);
                return;
            }

            CommitMove(target);
        }

        private void StepAlongPath()
        {
            if (_path == null || _path.Count == 0)
            {
                _path = null;
                return;
            }

            Coord next = _path.Dequeue();

            // Stop auto-walking when the next cell holds something to act on, and resolve it this turn
            if (_entitySpawner != null && _entitySpawner.TryGetInteractable(next, out _))
                _path = null;

            TryAct(next - _cell);
        }

        private void CommitMove(Coord to)
        {
            _cell = to;
            BeginHop(CellCenter(to));
            Moved?.Invoke(_cell);
        }

        // An attack or loot still costs a turn
        private void ConsumeTurnInPlace()
        {
            BeginHop(transform.position);
            Moved?.Invoke(_cell);
        }

        // Diagonals (Q/E/Z/C) are checked before the cardinals (WASD/arrows).
        private bool TryReadKeyboardStep(out Coord step)
        {
            step = Coord.Zero;
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return false;

            if (keyboard.qKey.isPressed) step = Coord.Up + Coord.Left;
            else if (keyboard.eKey.isPressed) step = Coord.Up + Coord.Right;
            else if (keyboard.zKey.isPressed) step = Coord.Down + Coord.Left;
            else if (keyboard.cKey.isPressed) step = Coord.Down + Coord.Right;
            else if (keyboard.upArrowKey.isPressed || keyboard.wKey.isPressed) step = Coord.Up;
            else if (keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed) step = Coord.Down;
            else if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed) step = Coord.Left;
            else if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed) step = Coord.Right;

            return step.X != 0 || step.Y != 0;
        }

        private bool TryReadClickTarget(out Coord target)
        {
            target = _cell;
            Mouse mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame || _camera == null || _tilemap == null)
                return false;

            Vector2 screen = mouse.position.ReadValue();
            if (ScreenGuiRegistry.IsPointerOverPanel(screen)) return false; // the click landed on a panel

            Vector3 world = _camera.ScreenToWorldPoint(screen);
            Vector3Int cell = _tilemap.WorldToCell(world);
            target = new Coord(cell.x, cell.y);
            return target != _cell;
        }

        private Queue<Coord> BuildPath(Coord target)
        {
            IReadOnlyList<Coord> route = GridPathfinder.FindPath(_grid, _cell, target);
            if (route.Count == 0) return null;

            var queue = new Queue<Coord>(route.Count);
            foreach (Coord cell in route) queue.Enqueue(cell);
            return queue;
        }

        private void BeginHop(Vector3 target)
        {
            _hopFrom = transform.position;
            _hopTo = target;
            _hopElapsed = 0f;
            _isHopping = true;
        }

        private void Animate()
        {
            _hopElapsed += Time.deltaTime;
            float t = _stepDuration <= 0f ? 1f : Mathf.Clamp01(_hopElapsed / _stepDuration);
            transform.position = Vector3.Lerp(_hopFrom, _hopTo, t);
            if (t >= 1f) _isHopping = false;
        }

        private Vector3 CellCenter(Coord cell)
        {
            if (_tilemap != null)
                return _tilemap.GetCellCenterWorld(new Vector3Int(cell.X, cell.Y, 0));

            return new Vector3(cell.X + CellCenterOffset, cell.Y + CellCenterOffset, 0f);
        }

        private void OnGUI()
        {
            if (!_placed || _character == null) return;

            var rect = new Rect(HudMargin, Screen.height - HudHeight - HudMargin, HudWidth, HudHeight);
            ScreenGuiRegistry.Register(rect);
            GUILayout.BeginArea(rect, GUI.skin.box);
            GUILayout.Label($"Level {_character.Experience.Level} HP {_character.Health.Current}/{_character.Health.Max}");
            GUILayout.Label($"ATK {_character.AttackPower} Gold {_character.Gold}");
            if (!string.IsNullOrEmpty(_lastMessage)) GUILayout.Label(_lastMessage);
            GUILayout.EndArea();
        }
    }
}
