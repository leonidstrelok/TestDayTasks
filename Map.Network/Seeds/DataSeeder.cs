using Map.Core.Interfaces;
using Map.Core.Layers;
using Map.Core.Models;

namespace Map.Network.Seeds;

public class DataSeeder
{
    private readonly IMapObjectRepository _repository;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(IMapObjectRepository repository, ILogger<DataSeeder> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SeedAsync(int mapWidth = 1000, int mapHeight = 1000, int regionCount = 10)
    {
        _logger.LogInformation("Инициализация данных карты {Width}x{Height}...", mapWidth, mapHeight);

        var regionLayer = new RegionLayer(mapWidth, mapHeight);
        if (regionLayer.Regions.Count == 0)
        {
            regionLayer.GenerateRegions(regionCount);

            var random = new Random(42);

            int createdCount = 0;

            foreach (var region in regionLayer.Regions.Values)
            {
                // Создаём 3 объекта на каждый регион
                for (int i = 0; i < 5; i++)
                {
                    var obj = new MapObject
                    {
                        Id = $"{region.Id}-{i}",
                        X = random.Next(0, mapWidth),
                        Y = random.Next(0, mapHeight),
                        Width = 10,
                        Height = 10,
                        Type = "RegionObject",
                        Metadata = new Dictionary<string, string>
                        {
                            ["RegionName"] = region.Name,
                            ["CreatedAt"] = DateTime.UtcNow.ToString("O")
                        }
                    };

                    await _repository.AddAsync(obj);
                    createdCount++;
                }
            }

            _logger.LogInformation("Создано {Count} объектов в Redis", createdCount);
        }
    }
}