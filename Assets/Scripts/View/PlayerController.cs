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

        private const float HudWidth = 240f;
        private const float HudHeight = 92f;
        private const float HudMargin = 10f;
        private const float ItemRowHeight = 22f;
        private const float FlashDuration = 0.35f;
        private const float ShakeDuration = 0.28f;
        private const float ShakeMagnitude = 0.14f;
        private const float NumberDuration = 0.9f;
        private const float NumberRise = 100f;
        private const float NumberJitter = 22f;
        private const float NumberAnchorX = 52f;
        private const float NumberWidth = 100f;
        private const float NumberHeight = 40f;
        private const int NumberFontSize = 40;
        private const float LowHpFraction = 0.3f;

        private static readonly Color DamageFlashColor = new Color(1f, 0.3f, 0.3f, 1f);
        private static readonly Color DamageNumberColor = new Color(1f, 0.3f, 0.3f, 1f);
        private static readonly Color LowHpColor = new Color(0.9f, 0.2f, 0.2f, 1f);

        private DungeonGrid _grid;
        private Coord _cell;
        private bool _placed;

        private PlayerCharacter _character;
        private string _lastMessage;
        private int _pendingUseIndex = -1;

        private bool _isHopping;
        private Vector3 _hopFrom;
        private Vector3 _hopTo;
        private float _hopElapsed;

        private SpriteRenderer _sprite;
        private Color _baseColor = Color.white;
        private Vector3 _visualBase; // position before the damage shake offset
        private float _shakeElapsed = ShakeDuration;
        private float _flashElapsed = FlashDuration;

        private readonly List<DamageNumber> _damageNumbers = new List<DamageNumber>();
        private GUIStyle _damageStyle;
        private GUIStyle _hpStyle;
        private Color _defaultLabelColor;

        private Queue<Coord> _path; // pending click-to-move route, walked one cell per turn

        // Raised after the player takes a turn (move, attack, or loot); the seam a future turn
        // manager uses to run enemy turns and to gate input while enemies are in sight.
        public event System.Action<Coord> Moved;

        public event System.Action ReachedExit;

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
            EnsureCharacter();
            _character.Health.ResetToFull();
            _visualBase = TileWorld.CellCenter(_tilemap, startCell);
            transform.position = _visualBase;
        }

        private void Awake()
        {
            if (_camera == null) _camera = Camera.main;
            if (_entitySpawner == null) _entitySpawner = FindAnyObjectByType<EntitySpawner>();
            _sprite = GetComponentInChildren<SpriteRenderer>();
            if (_sprite != null) _baseColor = _sprite.color;
            EnsureCharacter();
        }

        // Builds the character on first use, so Place still works when it runs before Awake
        private void EnsureCharacter()
        {
            if (_character == null)
            {
                _character = new PlayerCharacter(Mathf.Max(1, _maxHealth), Mathf.Max(0, _attackPower));
                _character.Health.Damaged += OnDamaged;
            }
        }

        private void Update()
        {
            if (WindowDock.IsDocked) return; // paused while docked as a task widget

            // applied here rather than inside OnGUI
            if (_pendingUseIndex >= 0)
            {
                if (_character.TryUseItem(_pendingUseIndex, out Item used))
                    _lastMessage = $"Used {used.Name}";
                _pendingUseIndex = -1;
            }

            if (_isHopping)
            {
                Animate();
                return;
            }
            if (!_placed) return;
            if (!_character.IsAlive) return; // no turns once dead

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
            BeginHop(TileWorld.CellCenter(_tilemap, to));
            Moved?.Invoke(_cell);
            if (_grid[to] == TileType.Exit) ReachedExit?.Invoke();
        }

        // An attack or loot still costs a turn
        private void ConsumeTurnInPlace()
        {
            BeginHop(_visualBase);
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
            _hopFrom = _visualBase;
            _hopTo = target;
            _hopElapsed = 0f;
            _isHopping = true;
        }

        private void Animate()
        {
            _hopElapsed += Time.deltaTime;
            float t = _stepDuration <= 0f ? 1f : Mathf.Clamp01(_hopElapsed / _stepDuration);
            _visualBase = Vector3.Lerp(_hopFrom, _hopTo, t);
            ApplyVisual();
            if (t >= 1f) _isHopping = false;
        }

        private void LateUpdate()
        {
            UpdateShake();
            UpdateFlash();
            UpdateDamageNumbers();
        }

        // flashes the sprite, shakes the character, and floats the amount lost out of the HUD
        private void OnDamaged(int amount)
        {
            _shakeElapsed = 0f;
            _flashElapsed = 0f;
            _damageNumbers.Add(new DamageNumber
            {
                Amount = amount,
                Elapsed = 0f,
                OffsetX = Random.Range(-NumberJitter, NumberJitter)
            });
        }

        private void UpdateDamageNumbers()
        {
            for (int i = _damageNumbers.Count - 1; i >= 0; i--)
            {
                DamageNumber number = _damageNumbers[i];
                number.Elapsed += Time.deltaTime;
                if (number.Elapsed >= NumberDuration) _damageNumbers.RemoveAt(i);
                else _damageNumbers[i] = number;
            }
        }

        private void UpdateShake()
        {
            if (_shakeElapsed >= ShakeDuration) return;

            _shakeElapsed += Time.deltaTime;
            ApplyVisual();
            if (_shakeElapsed >= ShakeDuration) transform.position = _visualBase; // settle back
        }

        private void ApplyVisual() => transform.position = _visualBase + ShakeOffset();

        private Vector3 ShakeOffset()
        {
            if (_shakeElapsed >= ShakeDuration) return Vector3.zero;
            float magnitude = ShakeMagnitude * (1f - _shakeElapsed / ShakeDuration);
            return new Vector3(Random.Range(-magnitude, magnitude), Random.Range(-magnitude, magnitude), 0f);
        }

        private void UpdateFlash()
        {
            if (_sprite == null || _flashElapsed >= FlashDuration) return;

            _flashElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_flashElapsed / FlashDuration);
            _sprite.color = Color.Lerp(DamageFlashColor, _baseColor, t);
        }

        private void OnGUI()
        {
            if (WindowDock.IsDocked) return; // only the task list shows while docked
            if (!_placed || _character == null) return;

            Inventory inventory = _character.Inventory;
            float height = HudHeight + inventory.Count * ItemRowHeight;
            var rect = new Rect(HudMargin, Screen.height - height - HudMargin, HudWidth, height);
            ScreenGuiRegistry.Register(rect);

            GUILayout.BeginArea(rect, GUI.skin.box);
            DrawHealthLine();
            GUILayout.Label($"ATK {_character.AttackPower} Gold {_character.Gold}");
            if (!string.IsNullOrEmpty(_lastMessage)) GUILayout.Label(_lastMessage);
            DrawInventory(inventory);
            GUILayout.EndArea();

            // drawn outside the area so the numbers can rise out past the panel's top edge
            DrawDamageNumbers(rect.y);
        }

        // "Level N" plus a bold HP readout that turns red at low health
        private void DrawHealthLine()
        {
            if (_hpStyle == null)
            {
                _hpStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
                _defaultLabelColor = GUI.skin.label.normal.textColor;
            }

            Health health = _character.Health;
            bool lowHp = health.Max > 0 && (float)health.Current / health.Max < LowHpFraction;
            _hpStyle.normal.textColor = lowHp ? LowHpColor : _defaultLabelColor;

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Level {_character.Experience.Level}");
            GUILayout.Label($"HP {health.Current}/{health.Max}", _hpStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        // floating "-N" numbers that rise out of the HUD's HP line and fade
        private void DrawDamageNumbers(float panelTop)
        {
            if (_damageNumbers.Count == 0) return;

            _damageStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = NumberFontSize
            };

            Color previous = GUI.color;
            for (int i = 0; i < _damageNumbers.Count; i++)
            {
                DamageNumber number = _damageNumbers[i];
                float t = number.Elapsed / NumberDuration;
                float x = HudMargin + NumberAnchorX + number.OffsetX;
                float y = panelTop - NumberRise * t;
                GUI.color = new Color(DamageNumberColor.r, DamageNumberColor.g, DamageNumberColor.b, 1f - t);
                GUI.Label(new Rect(x, y, NumberWidth, NumberHeight), $"-{number.Amount}", _damageStyle);
            }
            GUI.color = previous;
        }

        private struct DamageNumber
        {
            public int Amount;
            public float Elapsed;
            public float OffsetX;
        }

        // lists carried items
        private void DrawInventory(Inventory inventory)
        {
            bool canUse = _character.IsAlive;
            IReadOnlyList<Item> items = inventory.Items;
            for (int i = 0; i < items.Count; i++)
            {
                Item item = items[i];
                string label = $"{item.Name} (+{item.Magnitude})";
                if (canUse)
                {
                    if (GUILayout.Button("Use " + label)) _pendingUseIndex = i;
                }
                else
                {
                    GUILayout.Label(label);
                }
            }
        }
    }
}
