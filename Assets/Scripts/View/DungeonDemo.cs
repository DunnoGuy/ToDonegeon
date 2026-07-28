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

        [Header("Space type")]
        [Tooltip("Carve each dungeon as contiguous zones of all three shapes")]
        [SerializeField] private bool _mixSpaceTypes = true;
        [Tooltip("Used only when Mix Space Types is off")]
        [SerializeField] private ChamberShape _chamberShape = ChamberShape.Rectangle;
        [SerializeField] private bool _enableLoops;
        [Tooltip("Cave chambers only")]
        [SerializeField] private bool _repairCavePockets = true;

        [Header("Populate")]
        [SerializeField] private int _maxChests = PopulatorSettings.DefaultMaxChests;
        [SerializeField] private int _maxEnemiesPerRoom = PopulatorSettings.DefaultMaxEnemiesPerRoom;
        [SerializeField] private int _difficultyTier = PopulatorSettings.DefaultDifficultyTier;

        [Header("References")]
        [SerializeField] private DungeonRenderer _renderer;
        [SerializeField] private EntitySpawner _entitySpawner;
        [SerializeField] private PlayerController _player;
        [SerializeField] private EnemyDirector _enemyDirector;
        [SerializeField] private Camera _camera;

        [Tooltip("Frame the whole dungeon on generate")]
        [SerializeField] private bool _frameCameraToDungeon = true;

        [Header("Controls")]
        [SerializeField] private Key _regenerateKey = Key.R;

        private const float CameraDepth = -10f;
        private const float CameraPadding = 1f;
        private const float PanelWidth = 320f;
        private const float PanelHeight = 232f;
        private const float PanelMargin = 10f;
        private const float GameOverWidth = 240f;
        private const float GameOverHeight = 96f;
        private const float LoopChance = 0.5f;
        private const int MaxLoops = 3;

        private static readonly ChamberShape[] AllShapes =
            { ChamberShape.Rectangle, ChamberShape.Cave, ChamberShape.RandomWalk };

        private static readonly System.Random SeedSource = new System.Random();

        private Dungeon _dungeon;
        private string _error;
        private int _bossCount;
        private int _chestCount;
        private int _enemyCount;

        private void Start()
        {
            if (_renderer == null) _renderer = GetComponentInChildren<DungeonRenderer>();
            if (_entitySpawner == null) _entitySpawner = GetComponentInChildren<EntitySpawner>();
            if (_enemyDirector == null) _enemyDirector = FindAnyObjectByType<EnemyDirector>();
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
                var caveSettings = new CaveSettings(keepOnlyLargestComponent: _repairCavePockets);
                ChamberShape[] mix = _mixSpaceTypes ? AllShapes : null;
                var carver = new RoomCarverSettings(
                    _edgePadding, _minRoomSize, _minFillRatio, _chamberShape, caveSettings, shapeMix: mix);
                var populator = new PopulatorSettings(_maxChests, _maxEnemiesPerRoom, _difficultyTier);
                var corridor = _enableLoops ? new CorridorSettings(LoopChance, MaxLoops) : null;
                var generator = new BspDungeonGenerator(bsp, carver, populator, corridor);

                _dungeon = generator.Generate(_width, _height, _seed);
                _error = null;
                CountEntities();

                if (_renderer != null)
                {
                    _renderer.Render(_dungeon.Grid, _dungeon.Rooms);
                    if (_frameCameraToDungeon) FrameCamera();
                }
                if (_entitySpawner != null) _entitySpawner.Spawn(_dungeon.Entities);
                if (_player != null && _dungeon.Entrance != null)
                    _player.Place(_dungeon.Grid, _dungeon.Entrance.Center);
                if (_enemyDirector != null)
                    _enemyDirector.Bind(_dungeon.Grid, _player, _entitySpawner, _seed);

                Debug.Log($"[DungeonDemo] seed={_dungeon.Seed} shape={SpaceType()} rooms={_dungeon.Rooms.Count} " +
                    $"valid={_dungeon.IsValid} attempts={_dungeon.Attempts} entities={_dungeon.Entities.Count}");
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
                GUILayout.Label($"Space type: {SpaceType()}");
                GUILayout.Label($"Size: {_width} x {_height}   Rooms: {_dungeon.Rooms.Count}");
                GridConnectivityLabel();
                GUILayout.Label($"Boss: {_bossCount}   Chests: {_chestCount}   Enemies: {_enemyCount}");
            }

            if (_error != null) GUILayout.Label("Error: " + _error);

            if (GUILayout.Button($"New dungeon  (key: {_regenerateKey})")) Regenerate();
            if (GUILayout.Button("Replay this seed")) Generate();

            GUILayout.EndArea();

            if (PlayerIsDead) DrawGameOver();
        }

        private bool PlayerIsDead =>
            _player != null && _player.Character != null && !_player.Character.IsAlive;

        // Centered overlay shown once an enemy turn kills the player
        private void DrawGameOver()
        {
            float x = (Screen.width - GameOverWidth) * 0.5f;
            float y = (Screen.height - GameOverHeight) * 0.5f;
            GUILayout.BeginArea(new Rect(x, y, GameOverWidth, GameOverHeight), GUI.skin.box);
            GUILayout.Label("You died.");
            GUILayout.Label("The dungeon claims another.");
            if (GUILayout.Button($"New run  (key: {_regenerateKey})")) Regenerate();
            GUILayout.EndArea();
        }

        private void GridConnectivityLabel()
        {
            string status = _dungeon.IsValid ? "VALID — fully connected" : "INVALID — see console";
            GUILayout.Label($"Validation: {status}   Attempts: {_dungeon.Attempts}");
            GUILayout.Label(
                $"Reachable: {_dungeon.Connectivity.ReachableCount}/{_dungeon.Connectivity.WalkableCount} walkable");
        }

        private string SpaceType()
        {
            string shape = _mixSpaceTypes ? "Mixed zones" : _chamberShape.ToString();
            return _enableLoops ? $"{shape} + loops" : shape;
        }

        private void CountEntities()
        {
            _bossCount = 0;
            _chestCount = 0;
            _enemyCount = 0;
            if (_dungeon == null) return;

            foreach (Entity entity in _dungeon.Entities)
            {
                switch (entity.Kind)
                {
                    case EntityKind.Boss: _bossCount++; break;
                    case EntityKind.Chest: _chestCount++; break;
                    case EntityKind.Enemy: _enemyCount++; break;
                }
            }
        }
    }
}
