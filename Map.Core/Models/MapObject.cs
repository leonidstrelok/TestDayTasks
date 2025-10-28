using MemoryPack;

namespace Map.Core.Models;

[MemoryPackable]
public partial class MapObject
{
    public string Id { get; set; } = string.Empty;

    public int X { get; set; }

    public int Y { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public string Type { get; set; } = string.Empty;

    public Dictionary<string, string> Metadata { get; set; } = new();

    public bool ContainsPoint(int x, int y)
    {
        return x >= X && x < X + Width && y >= Y && y < Y + Height;
    }

    public bool IntersectsArea(int x, int y, int width, int height)
    {
        return !(X + Width <= x || X >= x + width || Y + Height <= y || Y >= y + height);
    }

    public bool IsFullyWithinArea(int x, int y, int width, int height)
    {
        return X >= x && Y >= y && X + Width <= x + width && Y + Height <= y + height;
    }

    public (int X, int Y) GetCenter()
    {
        return (X + Width / 2, Y + Height / 2);
    }
}
