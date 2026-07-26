using System.Collections.Generic;
using UnityEngine;
using ToDonegeon.Generation;
using ToDonegeon.Gameplay.AI;
using DungeonGrid = ToDonegeon.Generation.Grid;

namespace ToDonegeon.View
{
    // Runs one enemy turn after every player turn
    public sealed class EnemyDirector : MonoBehaviour
    {
        private readonly EnemyBrain _brain = new EnemyBrain();
        private readonly HashSet<Coord> _occupied = new HashSet<Coord>();

        private DungeonGrid _grid;
        private PlayerController _player;
        private EntitySpawner _spawner;
        private System.Random _rng;

        // Points the director at a freshly generated dungeon
        public void Bind(DungeonGrid grid, PlayerController player, EntitySpawner spawner, int seed)
        {
            if (_player != null) _player.Moved -= OnPlayerMoved;

            _grid = grid;
            _player = player;
            _spawner = spawner;
            _rng = new System.Random(seed);

            if (_player != null) _player.Moved += OnPlayerMoved;
        }

        private void OnDisable()
        {
            if (_player != null) _player.Moved -= OnPlayerMoved;
        }

        private void OnPlayerMoved(Coord playerCell)
        {
            if (_grid == null || _spawner == null || _player == null) return;

            IReadOnlyList<Enemy> enemies = _spawner.Enemies;
            for (int i = 0; i < enemies.Count; i++)
            {
                Enemy enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive) continue;

                Apply(enemy, Decide(enemy, playerCell));
            }
        }

        private EnemyAction Decide(Enemy enemy, Coord playerCell)
        {
            _occupied.Clear();
            foreach (Coord cell in _spawner.OccupiedCells) _occupied.Add(cell);
            _occupied.Remove(enemy.Cell);

            var world = new WorldView(_grid, playerCell, _occupied);
            var self = new EnemyState(enemy.Cell, enemy.Character, enemy.SightRange, enemy.RetreatHealthFraction);
            return _brain.Decide(world, self, _rng);
        }

        private void Apply(Enemy enemy, EnemyAction action)
        {
            switch (action.Kind)
            {
                case EnemyActionKind.Move:
                    _spawner.Move(enemy.Cell, action.Target);
                    enemy.MoveTo(action.Target);
                    break;
                case EnemyActionKind.Attack:
                    enemy.StrikePlayer(_player.Character);
                    break;
            }
        }
    }
}
