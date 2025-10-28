using Map.Core.Models;

namespace Map.Core.Extensions;

public static class TilePropertiesExtensions
{
    public static bool CanPlaceObject(this TileType type)
    {
        return type == TileType.Plain;
    }
    
    public static int MovementCost(this TileType type)
    {
        return type switch
        {
            TileType.Plain => 1,
            TileType.Mountain => int.MaxValue,
            _ => 1
        };
    }
}