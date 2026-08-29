using UnityEngine;
using UnityEngine.Tilemaps;
using ToDonegeon.Generation;

namespace ToDonegeon.View
{
    // Shared tile-to-world conversion
    public static class TileWorld
    {
        private const float CellCenterOffset = 0.5f;

        // World position of a grid cell's centre
        public static Vector3 CellCenter(Tilemap tilemap, Coord cell)
        {
            if (tilemap != null)
                return tilemap.GetCellCenterWorld(new Vector3Int(cell.X, cell.Y, 0));
            return new Vector3(cell.X + CellCenterOffset, cell.Y + CellCenterOffset, 0f);
        }
    }
}
