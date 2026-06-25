using UnityEngine;
using UnityEngine.Tilemaps;
using ToDonegeon.Generation;
using DungeonGrid = ToDonegeon.Generation.Grid;

namespace ToDonegeon.View
{
    // Paints a generated dungeon grid onto a Unity Tilemap, one TileBase per TileType
    public sealed class DungeonRenderer : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Tilemap _tilemap;

        [Header("Tiles per cell type")]
        [SerializeField] private TileBase _wallTile;
        [SerializeField] private TileBase _floorTile;
        [SerializeField] private TileBase _corridorTile;
        [SerializeField] private TileBase _entranceTile;
        [SerializeField] private TileBase _exitTile;

        // Clears the tilemap and paints every cell of the grid.
        public void Render(DungeonGrid grid)
        {
            if (grid == null) throw new System.ArgumentNullException(nameof(grid));
            EnsureTilemap();

            _tilemap.ClearAllTiles();

            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    TileBase tile = TileFor(grid[x, y]);
                    if (tile != null) _tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }

        // World-space bounds of the area a grid of this size occupies (for the camera to frame it).
        public Bounds GetWorldBounds(DungeonGrid grid)
        {
            if (grid == null) throw new System.ArgumentNullException(nameof(grid));
            EnsureTilemap();

            Vector3 min = _tilemap.CellToWorld(new Vector3Int(0, 0, 0));
            Vector3 max = _tilemap.CellToWorld(new Vector3Int(grid.Width, grid.Height, 0));

            var bounds = new Bounds();
            bounds.SetMinMax(min, max);
            return bounds;
        }

        // Corridor/entrance/exit tiles are optional - an empty slot falls back to the floor tile.
        private TileBase TileFor(TileType type)
        {
            switch (type)
            {
                case TileType.Wall: return _wallTile;
                case TileType.Floor: return _floorTile;
                case TileType.Corridor: return _corridorTile != null ? _corridorTile : _floorTile;
                case TileType.Entrance: return _entranceTile != null ? _entranceTile : _floorTile;
                case TileType.Exit: return _exitTile != null ? _exitTile : _floorTile;
                default: return null;
            }
        }

        private void EnsureTilemap()
        {
            if (_tilemap == null) _tilemap = GetComponentInChildren<Tilemap>();
            if (_tilemap == null)
                throw new System.InvalidOperationException(
                    "DungeonRenderer has no Tilemap assigned and none in its children.");
        }
    }
}
