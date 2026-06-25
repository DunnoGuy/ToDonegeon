using UnityEngine;
using UnityEngine.InputSystem;
using ToDonegeon.Generation;
using ToDonegeon.Pipeline;

namespace ToDonegeon.View
{
    // Demo harness: builds generator settings from the Inspector, generates a dungeon, renders it,
    // and shows the seed and connectivity result.
    public sealed class DungeonDemo : MonoBehaviour
    {
        [Header("Dungeon size (cells)")]
        [SerializeField] private int _width = 48;
        [SerializeField] private int _height = 32;

        [Header("Seed")]
        [SerializeField] private int _seed;
        [SerializeField] private bool _randomSeedOnStart = true;

        [Header("BSP partition")]
        [SerializeField] private int _minLeafSize = BspSettings.DefaultMinLeafSize;
        [SerializeField] private int _maxLeafSize = BspSettings.DefaultMaxLeafSize;
        [SerializeField] private int _maxDepth = BspSettings.DefaultMaxDepth;

        [Header("Rooms")]
        [SerializeField] private int _edgePadding = RoomCarverSettings.DefaultEdgePadding;
        [SerializeField] private int _minRoomSize = RoomCarverSettings.DefaultMinRoomSize;
        [Range(0.1f, 1f)]
        [SerializeField] private float _minFillRatio = RoomCarverSettings.DefaultMinFillRatio;

        [Header("References")]
        [SerializeField] private DungeonRenderer _renderer;
        [SerializeField] private Camera _camera;

        [Header("Controls")]
        [SerializeField] private Key _regenerateKey = Key.R;

        private const float CameraDepth = -10f;
        private const float CameraPadding = 1f;
        private const float PanelWidth = 320f;
        private const float PanelHeight = 168f;
        private const float PanelMargin = 10f;

        private static readonly System.Random SeedSource = new System.Random();

        private Dungeon _dungeon;
        private string _error;

        private void Start()
        {
            if (_renderer == null) _renderer = GetComponentInChildren<DungeonRenderer>();
            if (_camera == null) _camera = Camera.main;
            if (_randomSeedOnStart) _seed = SeedSource.Next();

            Generate();
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard[_regenerateKey].wasPressedThisFrame)
                Regenerate();
        }

        // Generates and renders a fresh dungeon using a new random seed
        public void Regenerate()
        {
            _seed = SeedSource.Next();
            Generate();
        }

        // generates and renders a dungeon for the current seed and settings
        public void Generate()
        {
            try
            {
                var bsp = new BspSettings(_minLeafSize, _maxLeafSize, _maxDepth);
                var carver = new RoomCarverSettings(_edgePadding, _minRoomSize, _minFillRatio);
                var generator = new BspDungeonGenerator(bsp, carver);

                _dungeon = generator.Generate(_width, _height, _seed);
                _error = null;

                if (_renderer != null)
                {
                    _renderer.Render(_dungeon.Grid);
                    FrameCamera();
                }

                Debug.Log($"[DungeonDemo] seed={_dungeon.Seed} rooms={_dungeon.Rooms.Count} valid={_dungeon.IsValid}");
            }
            catch (System.Exception ex)
            {
                _error = ex.Message;
                Debug.LogError("[DungeonDemo] generation failed: " + ex);
            }
        }

        private void FrameCamera()
        {
            if (_camera == null || _dungeon == null) return;

            Bounds bounds = _renderer.GetWorldBounds(_dungeon.Grid);
            _camera.orthographic = true;
            _camera.transform.position = new Vector3(bounds.center.x, bounds.center.y, CameraDepth);

            float halfHeight = bounds.extents.y;
            float halfWidth = bounds.extents.x / _camera.aspect;
            _camera.orthographicSize = Mathf.Max(halfHeight, halfWidth) + CameraPadding;
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(PanelMargin, PanelMargin, PanelWidth, PanelHeight), GUI.skin.box);

            if (_dungeon != null)
            {
                GUILayout.Label($"Seed: {_dungeon.Seed}");
                GUILayout.Label($"Size: {_width} x {_height}   Rooms: {_dungeon.Rooms.Count}");
                GridConnectivityLabel();
            }

            if (_error != null) GUILayout.Label("Error: " + _error);

            if (GUILayout.Button($"New dungeon  (key: {_regenerateKey})")) Regenerate();
            if (GUILayout.Button("Replay this seed")) Generate();

            GUILayout.EndArea();
        }

        private void GridConnectivityLabel()
        {
            string status = _dungeon.IsValid ? "VALID — fully connected" : "INVALID — see console";
            GUILayout.Label($"Validation: {status}");
            GUILayout.Label(
                $"Reachable: {_dungeon.Connectivity.ReachableCount}/{_dungeon.Connectivity.WalkableCount} walkable");
        }
    }
}
