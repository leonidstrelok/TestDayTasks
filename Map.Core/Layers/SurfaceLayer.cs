using Map.Core.Extensions;
using Map.Core.Models;

namespace Map.Core.Layers;

public class SurfaceLayer
{
    private readonly byte[] _tiles;
    private readonly int _width;
    private readonly int _height;

    public int Width => _width;
    public int Height => _height;

    public SurfaceLayer(int width, int height)
    {
        if (width <= 0 || height <= 0)
            throw new ArgumentException("Размеры карты должны быть положительными");

        _width = width;
        _height = height;
        _tiles = new byte[width * height];
    }

    public static SurfaceLayer FromTiles(int width, int height, IEnumerable<TileType> tiles)
    {
        var layer = new SurfaceLayer(width, height);
        var index = 0;

        foreach (var tile in tiles)
        {
            if (index >= layer._tiles.Length)
                break;
            layer._tiles[index++] = (byte)tile;
        }

        return layer;
    }

    public static SurfaceLayer FromArray(TileType[,] tiles)
    {
        var height = tiles.GetLength(0);
        var width = tiles.GetLength(1);
        var layer = new SurfaceLayer(width, height);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                layer.SetTileUnchecked(x, y, tiles[y, x]);
            }
        }

        return layer;
    }

    public TileType GetTile(int x, int y)
    {
        if (!IsInBounds(x, y))
            throw new ArgumentOutOfRangeException($"Координаты ({x}, {y}) выходят за границы карты");

        return (TileType)_tiles[y * _width + x];
    }

    public TileType GetTileSafe(int x, int y, TileType defaultValue = TileType.Plain)
    {
        return IsInBounds(x, y) ? (TileType)_tiles[y * _width + x] : defaultValue;
    }

    public void SetTile(int x, int y, TileType type)
    {
        if (!IsInBounds(x, y))
            throw new ArgumentOutOfRangeException($"Координаты ({x}, {y}) выходят за границы карты");

        _tiles[y * _width + x] = (byte)type;
    }

    private void SetTileUnchecked(int x, int y, TileType type)
    {
        _tiles[y * _width + x] = (byte)type;
    }

    public void FillArea(int x, int y, int width, int height, TileType type)
    {
        // Обрезаем область до границ карты
        var x1 = Math.Max(0, x);
        var y1 = Math.Max(0, y);
        var x2 = Math.Min(_width, x + width);
        var y2 = Math.Min(_height, y + height);

        var tileValue = (byte)type;

        for (var cy = y1; cy < y2; cy++)
        {
            var rowStart = cy * _width;
            for (var cx = x1; cx < x2; cx++)
            {
                _tiles[rowStart + cx] = tileValue;
            }
        }
    }

    public void FillArea(MapArea area, TileType type)
    {
        FillArea(area.X, area.Y, area.Width, area.Height, type);
    }

    public bool CanPlaceObjectInArea(int x, int y, int width, int height)
    {
        if (x < 0 || y < 0 || x + width > _width || y + height > _height)
            return false;

        for (var cy = y; cy < y + height; cy++)
        {
            var rowStart = cy * _width;
            for (var cx = x; cx < x + width; cx++)
            {
                var tileType = (TileType)_tiles[rowStart + cx];
                if (!tileType.CanPlaceObject())
                    return false;
            }
        }

        return true;
    }

    public bool CanPlaceObjectInArea(MapArea area)
    {
        return CanPlaceObjectInArea(area.X, area.Y, area.Width, area.Height);
    }

    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < _width && y >= 0 && y < _height;
    }

    public long GetMemoryUsage()
    {
        return _tiles.Length +
               sizeof(int) * 2 + IntPtr.Size;
    }
}