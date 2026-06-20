namespace ToDonegeon.Generation
{
    public enum TileType : byte
    {
        Wall = 0,
        Floor = 1,
        Corridor = 2,
        Entrance = 3,
        Exit = 4
    }

    public static class TileTypeExtensions
    {
        public static bool IsWalkable(this TileType type)
        {
            switch (type)
            {
                case TileType.Floor:
                case TileType.Corridor:
                case TileType.Entrance:
                case TileType.Exit:
                    return true;
                default:
                    return false;
            }
        }
    }
}
