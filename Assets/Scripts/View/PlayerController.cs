using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using ToDonegeon.Generation;
using ToDonegeon.Gameplay;
using DungeonGrid = ToDonegeon.Generation.Grid;

namespace ToDonegeon.View
{
    public sealed class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Tilemap _tilemap;
        [SerializeField] private Camera _camera;

        [Header("Movement")]
        [Tooltip("Seconds for the visual hop between cells; also the minimum time between steps.")]
        [SerializeField] private float _stepDuration = 0.12f;

        private const float CellCenterOffset = 0.5f;

        private DungeonGrid _grid;
        private Coord _cell;
        private bool _placed;

        private bool _isHopping;
        private Vector3 _hopFrom;
        private Vector3 _hopTo;
        private float _hopElapsed;

        private Queue<Coord> _path; // pending click-to-move route, walked one cell per step

        // Raised after a successful step, for a future turn manager
        public event System.Action<Coord> Moved;

        public Coord Cell => _cell;

        public void Place(DungeonGrid grid, Coord startCell)
        {
            _grid = grid ?? throw new System.ArgumentNullException(nameof(grid));
            _cell = startCell;
            _placed = true;
            _isHopping = false;
            _path = null;
            transform.position = CellCenter(startCell);
        }

        private void Awake()
        {
            if (_camera == null) _camera = Camera.main;
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
                TryMove(step);
                return;
            }

            if (TryReadClickTarget(out Coord target))
                _path = BuildPath(target);

            StepAlongPath();
        }

        private void TryMove(Coord step)
        {
            if (GridMovement.TryStep(_grid, _cell, step, out Coord to))
                CommitMove(to);
        }

        private void StepAlongPath()
        {
            if (_path == null || _path.Count == 0)
            {
                _path = null;
                return;
            }

            Coord next = _path.Dequeue();
            if (!GridMovement.TryStep(_grid, _cell, next - _cell, out Coord to))
            {
                _path = null; // route no longer valid
                return;
            }
            CommitMove(to);
        }

        private void CommitMove(Coord to)
        {
            _cell = to;
            BeginHop(CellCenter(to));
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

            Vector3 world = _camera.ScreenToWorldPoint(mouse.position.ReadValue());
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
    }
}
