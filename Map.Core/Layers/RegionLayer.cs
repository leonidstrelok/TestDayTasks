using Map.Core.Models;

namespace Map.Core.Layers;

public class RegionLayer
{
    private readonly ushort[] _regionIds;
    private readonly Dictionary<ushort, Region> _regions;
    private readonly int _width;
    private readonly int _height;

    public int Width => _width;
    public int Height => _height;
    public IReadOnlyDictionary<ushort, Region> Regions => _regions;

    public RegionLayer(int width, int height)
    {
        if (width <= 0 || height <= 0)
            throw new ArgumentException("Размеры карты должны быть положительными");

        _width = width;
        _height = height;
        _regionIds = new ushort[width * height];
        _regions = new Dictionary<ushort, Region>();
    }

    public ushort GetRegionId(int x, int y)
    {
        if (!IsInBounds(x, y))
            throw new ArgumentOutOfRangeException($"Координаты ({x}, {y}) выходят за границы карты");

        return _regionIds[y * _width + x];
    }

    public ushort GetRegionIdSafe(int x, int y, ushort defaultValue = 0)
    {
        return IsInBounds(x, y) ? _regionIds[y * _width + x] : defaultValue;
    }

    public Region? GetRegionMetadata(ushort regionId)
    {
        return _regions.GetValueOrDefault(regionId);
    }

    public bool TileBelongsToRegion(int x, int y, ushort regionId)
    {
        if (!IsInBounds(x, y))
            return false;

        return GetRegionId(x, y) == regionId;
    }

    public HashSet<ushort> GetRegionsInArea(int x, int y, int width, int height)
    {
        var regionIds = new HashSet<ushort>();

        var x1 = Math.Max(0, x);
        var y1 = Math.Max(0, y);
        var x2 = Math.Min(_width, x + width);
        var y2 = Math.Min(_height, y + height);

        for (var cy = y1; cy < y2; cy++)
        {
            var rowStart = cy * _width;
            for (var cx = x1; cx < x2; cx++)
            {
                regionIds.Add(_regionIds[rowStart + cx]);
            }
        }

        return regionIds;
    }

    public List<Region?> GetRegionMetadataInArea(int x, int y, int width, int height)
    {
        var regionIds = GetRegionsInArea(x, y, width, height);
        return regionIds
            .Select(GetRegionMetadata)
            .Where(r => r != null)
            .ToList();
    }

    public void GenerateRegions(int regionCount, Random random = null)
    {
        random ??= new Random();

        if (regionCount <= 0)
            throw new ArgumentException("Количество регионов должно быть положительным");

        _regions.Clear();
        Array.Clear(_regionIds, 0, _regionIds.Length);

        // Создаем регионы
        for (ushort i = 1; i <= regionCount; i++)
        {
            _regions[i] = new Region
            {
                Id = i,
                Name = $"Регион {i}",
                TileCount = 0
            };
        }

        // Распределяем тайлы по регионам равномерно
        var totalTiles = _width * _height;
        var baseTilesPerRegion = totalTiles / regionCount;
        var extraTiles = totalTiles % regionCount;

        var regionTiles = new int[regionCount + 1]; // +1 т.к. ID начинаются с 1
        for (var i = 1; i <= regionCount; i++)
        {
            regionTiles[i] = baseTilesPerRegion + (i <= extraTiles ? 1 : 0);
        }

        // Заполняем карту регионами
        var currentRegion = 1;
        var tilesInCurrentRegion = 0;

        for (var y = 0; y < _height; y++)
        {
            var rowStart = y * _width;
            for (var x = 0; x < _width; x++)
            {
                _regionIds[rowStart + x] = (ushort)currentRegion;
                tilesInCurrentRegion++;

                // Переходим к следующему региону при заполнении квоты
                if (tilesInCurrentRegion >= regionTiles[currentRegion])
                {
                    currentRegion++;
                    tilesInCurrentRegion = 0;
                }
            }
        }

        // Обновляем счетчики тайлов в регионах
        UpdateRegionTileCounts();
    }

    private void UpdateRegionTileCounts()
    {
        var tileCounts = new Dictionary<ushort, int>();

        foreach (var regionId in _regionIds)
        {
            tileCounts.TryGetValue(regionId, out var count);
            tileCounts[regionId] = count + 1;
        }

        foreach (var (regionId, tileCount) in tileCounts)
        {
            if (_regions.TryGetValue(regionId, out var region))
            {
                region.TileCount = tileCount;
            }
        }
    }

    public void SetRegionName(ushort regionId, string name)
    {
        if (_regions.TryGetValue(regionId, out var region))
        {
            region.Name = name;
        }
    }

    private bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < _width && y >= 0 && y < _height;
    }

    public long GetMemoryUsage()
    {
        return _regionIds.Length * sizeof(ushort) +
               _regions.Count * 100;
    }
}