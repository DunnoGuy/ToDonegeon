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
        
        public event System.Action<Coord> Moved;

        public Coord Cell => _cell;
        
        public void Place(DungeonGrid grid, Coord startCell)
        {
            _grid = grid ?? throw new System.ArgumentNullException(nameof(grid));
            _cell = startCell;
            _placed = true;
            _isHopping = false;
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

            if (_placed && TryReadStep(out Coord step))
                TryMove(step);
        }

        private void TryMove(Coord step)
        {
            if (!GridMovement.TryStep(_grid, _cell, step, out Coord to)) return;

            _cell = to;
            BeginHop(CellCenter(to));
            Moved?.Invoke(_cell);
        }

        private bool TryReadStep(out Coord step)
        {
            step = Coord.Zero;

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.upArrowKey.isPressed || keyboard.wKey.isPressed) step = Coord.Up;
                else if (keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed) step = Coord.Down;
                else if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed) step = Coord.Left;
                else if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed) step = Coord.Right;

                if (step != Coord.Zero) return true;
            }

            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame && _camera != null && _tilemap != null)
            {
                Vector3 world = _camera.ScreenToWorldPoint(mouse.position.ReadValue());
                Vector3Int cell = _tilemap.WorldToCell(world);
                step = new Coord(System.Math.Sign(cell.x - _cell.X), System.Math.Sign(cell.y - _cell.Y));
                return step.X != 0 || step.Y != 0;
            }

            return false;
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
