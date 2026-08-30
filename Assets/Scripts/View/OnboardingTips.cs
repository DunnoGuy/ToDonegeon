using System.Collections.Generic;
using UnityEngine;
using ToDonegeon.Generation;

namespace ToDonegeon.View
{
    // Minimal first-floor onboarding: a few one-line tips that surface in context and never
    // appear beyond the opening floor.
    public sealed class OnboardingTips : MonoBehaviour
    {
        [SerializeField] private DungeonDemo _demo;
        [SerializeField] private PlayerController _player;
        [SerializeField] private EntitySpawner _spawner;

        [Tooltip("Seconds each tip stays on screen")]
        [SerializeField] private float _tipDuration = 5f;
        [Tooltip("An enemy within this many tiles triggers the combat tip")]
        [SerializeField] private int _enemyTipRange = 6;
        [SerializeField] private Camera _camera;
        [Range(0f, 1f)]
        [Tooltip("Where the tip sits above the character: 0 hugs the character, 1 climbs to the top edge")]
        [SerializeField] private float _heightBias = 0.5f;

        private const int FirstFloorDepth = 1;
        private const float TipWidth = 380f;
        private const float TipHeight = 55f;
        private const float TopMargin = 12f;
        private const float SideMargin = 8f;
        private const float HeadWorldOffset = 0.8f;
        private const float BorderThickness = 2f;
        private const int TipFontSize = 15;

        private static readonly Color BorderColor = new Color(0.05f, 0.05f, 0.08f, 1f);
        private static readonly Color FillColor = new Color(0.98f, 0.62f, 0.09f, 1f);
        private static readonly Color TextColor = new Color(0.08f, 0.05f, 0.02f, 1f);

        private const string MoveTip = "Move with WASD or click. Find a pink tile to advance";
        private const string CombatTip = "You can bump into an enemy to attack, but they will fight back! Only fight when it's unavoidable";
        private const string ItemTip = "Use looted items from the panel (bottom-left)";
        private const string TaskTip = "Tick off real to-dos in the Tasks panel (right) to earn XP";
        private const string XpTip = "Harder tasks give more XP; level-ups boost attack and heal";

        private bool _onFirstFloor;
        private bool _moveTipDone;
        private bool _combatTipDone;
        private bool _itemTipDone;
        private bool _taskTipDone;
        private bool _xpTipDone;
        private int _xpAtFloorStart;

        private string _currentTip;
        private float _tipExpiry;
        private GUIStyle _style;

        private void Awake()
        {
            if (_demo == null) _demo = FindAnyObjectByType<DungeonDemo>();
            if (_player == null) _player = FindAnyObjectByType<PlayerController>();
            if (_spawner == null) _spawner = FindAnyObjectByType<EntitySpawner>();
            if (_camera == null) _camera = Camera.main;
        }

        private void OnEnable()
        {
            if (_demo != null) _demo.FloorGenerated += OnFloorGenerated;
        }

        private void OnDisable()
        {
            if (_demo != null) _demo.FloorGenerated -= OnFloorGenerated;
        }

        private void OnFloorGenerated(int depth)
        {
            _onFirstFloor = depth <= FirstFloorDepth;
            _moveTipDone = _combatTipDone = _itemTipDone = _taskTipDone = _xpTipDone = false;
            _xpAtFloorStart = CurrentExperience();
            _currentTip = null;

            if (_onFirstFloor) Show(MoveTip, ref _moveTipDone);
        }

        private void Update()
        {
            if (!_onFirstFloor) return;

            if (_currentTip != null && Time.time >= _tipExpiry) _currentTip = null;

            if (!_combatTipDone && EnemyNearby()) Show(CombatTip, ref _combatTipDone);
            else if (!_xpTipDone && GainedExperience()) Show(XpTip, ref _xpTipDone);
            else if (!_itemTipDone && PlayerHasItem()) Show(ItemTip, ref _itemTipDone);
            else if (!_taskTipDone && _moveTipDone && _currentTip == null) Show(TaskTip, ref _taskTipDone);
        }

        private bool EnemyNearby()
        {
            if (_player == null || _spawner == null) return false;

            Coord here = _player.Cell;
            IReadOnlyList<Enemy> enemies = _spawner.Enemies;
            for (int i = 0; i < enemies.Count; i++)
            {
                Enemy enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive) continue;
                if (Coord.ManhattanDistance(here, enemy.Cell) <= _enemyTipRange) return true;
            }
            return false;
        }

        private bool PlayerHasItem()
            => _player != null && _player.Character != null && _player.Character.Inventory.Count > 0;

        private bool GainedExperience() => CurrentExperience() > _xpAtFloorStart;

        private int CurrentExperience()
            => _player != null && _player.Character != null ? _player.Character.Experience.Total : 0;

        private void Show(string tip, ref bool done)
        {
            _currentTip = tip;
            _tipExpiry = Time.time + _tipDuration;
            done = true;
        }

        private void OnGUI()
        {
            if (_currentTip == null) return;

            _style ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = TipFontSize,
                wordWrap = true
            };
            _style.normal.textColor = TextColor;

            Rect outer = TipRect();
            var inner = new Rect(
                outer.x + BorderThickness, outer.y + BorderThickness,
                outer.width - BorderThickness * 2f, outer.height - BorderThickness * 2f);

            Color previous = GUI.color;
            GUI.color = BorderColor;
            GUI.DrawTexture(outer, Texture2D.whiteTexture);
            GUI.color = FillColor;
            GUI.DrawTexture(inner, Texture2D.whiteTexture);
            GUI.color = previous;

            GUI.Label(outer, _currentTip, _style);
        }

        // Floats the tip above the character, in the gap between it and the top edge; falls back to
        // top-centre when the camera or player is unavailable.
        private Rect TipRect()
        {
            float left = (Screen.width - TipWidth) * 0.5f;
            float top = TopMargin;

            if (_camera == null) _camera = Camera.main;
            if (_camera != null && _player != null)
            {
                Vector3 screen = _camera.WorldToScreenPoint(_player.transform.position + Vector3.up * HeadWorldOffset);
                if (screen.z > 0f)
                {
                    float headGuiY = Screen.height - screen.y;
                    left = screen.x - TipWidth * 0.5f;
                    top = Mathf.Lerp(headGuiY - TipHeight, TopMargin, _heightBias);
                }
            }

            left = Mathf.Clamp(left, SideMargin, Screen.width - TipWidth - SideMargin);
            top = Mathf.Clamp(top, TopMargin, Screen.height - TipHeight);
            return new Rect(left, top, TipWidth, TipHeight);
        }
    }
}
