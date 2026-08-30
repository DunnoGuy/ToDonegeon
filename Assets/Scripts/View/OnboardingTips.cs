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

        private const int FirstFloorDepth = 1;
        private const float TipWidth = 340f;
        private const float TipHeight = 28f;
        private const float TopMargin = 8f;

        private const string MoveTip = "Move with WASD or click. Find a pink tile to advance";
        private const string CombatTip = "Bump into an enemy to attack. Be careful, they can attack back!";
        private const string ItemTip = "Use looted items from the panel (bottom-left)";

        private bool _onFirstFloor;
        private bool _moveTipDone;
        private bool _combatTipDone;
        private bool _itemTipDone;

        private string _currentTip;
        private float _tipExpiry;
        private GUIStyle _style;

        private void Awake()
        {
            if (_demo == null) _demo = FindAnyObjectByType<DungeonDemo>();
            if (_player == null) _player = FindAnyObjectByType<PlayerController>();
            if (_spawner == null) _spawner = FindAnyObjectByType<EntitySpawner>();
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
            _moveTipDone = _combatTipDone = _itemTipDone = false;
            _currentTip = null;

            if (_onFirstFloor) Show(MoveTip, ref _moveTipDone);
        }

        private void Update()
        {
            if (!_onFirstFloor) return;

            if (_currentTip != null && Time.time >= _tipExpiry) _currentTip = null;

            if (!_combatTipDone && EnemyNearby()) Show(CombatTip, ref _combatTipDone);
            else if (!_itemTipDone && PlayerHasItem()) Show(ItemTip, ref _itemTipDone);
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

        private void Show(string tip, ref bool done)
        {
            _currentTip = tip;
            _tipExpiry = Time.time + _tipDuration;
            done = true;
        }

        private void OnGUI()
        {
            if (_currentTip == null) return;

            _style ??= new GUIStyle(GUI.skin.box) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
            float x = (Screen.width - TipWidth) * 0.5f;
            GUI.Box(new Rect(x, TopMargin, TipWidth, TipHeight), _currentTip, _style);
        }
    }
}
