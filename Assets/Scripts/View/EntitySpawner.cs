using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using ToDonegeon.Generation;

namespace ToDonegeon.View
{
    // Instantiates a prefab for each generated entity, separate from the generator
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

        // clears the previous batch and spawns a prefab for every entity that has one assigned
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
                _spawned.Add(Instantiate(prefab, world, Quaternion.identity, _container));
            }
        }

        // destroys all currently spawned entity GameObjects
        public void Clear()
        {
            foreach (GameObject instance in _spawned)
                if (instance != null) Destroy(instance);
            _spawned.Clear();
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
