using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using ToDonegeon.Generation;

namespace ToDonegeon.View
{
    // Instantiates a prefab for each generated entity, separate from the generator, and tracks the
    // interactable (if any) on each cell so the player can act on it.
    public sealed class EntitySpawner : MonoBehaviour
    {
        [Header("Coordinate source")]
        [SerializeField] private Tilemap _tilemap;
        [SerializeField] private Transform _container;

        [Header("Prefabs per entity kind")]
        [SerializeField] private GameObject _enemyPrefab;
        [SerializeField] private GameObject _chestPrefab;
        [SerializeField] private GameObject _bossPrefab;

        private readonly List<GameObject> _spawned = new List<GameObject>();
        private readonly Dictionary<Coord, IInteractable> _interactables = new Dictionary<Coord, IInteractable>();

        // Clears the previous batch and spawns a prefab for every entity that has one assigned.
        public void Spawn(IReadOnlyList<Entity> entities)
        {
            if (entities == null) throw new System.ArgumentNullException(nameof(entities));
            EnsureRefs();
            Clear();

            foreach (Entity entity in entities)
            {
                GameObject prefab = PrefabFor(entity.Kind);
                if (prefab == null) continue;

                Vector3 world = _tilemap.GetCellCenterWorld(new Vector3Int(entity.Position.X, entity.Position.Y, 0));
                GameObject instance = Instantiate(prefab, world, Quaternion.identity, _container);
                _spawned.Add(instance);

                if (instance.TryGetComponent(out IInteractable interactable))
                    _interactables[entity.Position] = interactable;
            }
        }

        // The interactable currently on the cell, if any
        // drops the entry if it was destroyed.
        public bool TryGetInteractable(Coord cell, out IInteractable interactable)
        {
            if (_interactables.TryGetValue(cell, out interactable))
            {
                if (interactable is Object obj && obj == null)
                {
                    _interactables.Remove(cell);
                    interactable = null;
                    return false;
                }
                return true;
            }

            interactable = null;
            return false;
        }

        // forget the interactable on the cell after the player has consumed it
        public void Remove(Coord cell) => _interactables.Remove(cell);

        // destroy all currently spawned entity GameObjects
        public void Clear()
        {
            foreach (GameObject instance in _spawned)
                if (instance != null) Destroy(instance);
            _spawned.Clear();
            _interactables.Clear();
        }

        private void EnsureRefs()
        {
            if (_tilemap == null) _tilemap = GetComponentInChildren<Tilemap>();
            if (_container == null) _container = transform;
            if (_tilemap == null)
                throw new System.InvalidOperationException(
                    "EntitySpawner has no Tilemap assigned and none in its children.");
        }

        private GameObject PrefabFor(EntityKind kind)
        {
            switch (kind)
            {
                case EntityKind.Enemy: return _enemyPrefab;
                case EntityKind.Chest: return _chestPrefab;
                case EntityKind.Boss: return _bossPrefab;
                default: return null;
            }
        }
    }
}
