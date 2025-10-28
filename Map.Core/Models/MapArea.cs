namespace Map.Core.Models;

public readonly struct MapArea
{
    public readonly int X;
    public readonly int Y;
    public readonly int Width;
    public readonly int Height;

    public MapArea(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public bool Contains(int x, int y) => x >= X && x < X + Width && y >= Y && y < Y + Height;
}