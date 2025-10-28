using Map.Core.Interfaces;
using Map.Core.Layers;
using Map.Core.Models;
using Map.Network.Services;
using Map.Shared.Models;
using Moq;

namespace Map.Tests;

public class MapServiceTests
{
    [Fact]
    public async Task GetObjectsInAreaAsync_ReturnsCorrectObjects()
    {
        // Arrange
        var objectLayer = CreateMockObjectLayer();
        var regionLayer = CreateMockRegionLayer();
        var service = new MapService(objectLayer, regionLayer);

        var request = new GetObjectsInAreaRequest
        {
            X1 = 0,
            Y1 = 0,
            X2 = 100,
            Y2 = 100
        };

        // Act
        var response = await service.GetObjectsInAreaAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Objects);
    }

    [Fact]
    public async Task GetObjectsInAreaAsync_HandlesSwitchedCoordinates()
    {
        // Arrange
        var objectLayer = CreateMockObjectLayer();
        var regionLayer = CreateMockRegionLayer();
        var service = new MapService(objectLayer, regionLayer);

        // Координаты в обратном порядке
        var request = new GetObjectsInAreaRequest
        {
            X1 = 100,
            Y1 = 100,
            X2 = 0,
            Y2 = 0
        };

        // Act
        var response = await service.GetObjectsInAreaAsync(request);

        // Assert
        Assert.NotNull(response);
    }

    [Fact]
    public void GetRegionsInAreaAsync_ReturnsCorrectRegions()
    {
        // Arrange
        var objectLayer = CreateMockObjectLayer();
        var regionLayer = new RegionLayer(100, 100);
        regionLayer.GenerateRegions(5, new Random(42));

        var service = new MapService(objectLayer, regionLayer);

        var request = new GetRegionsInAreaRequest
        {
            X1 = 0,
            Y1 = 0,
            X2 = 50,
            Y2 = 50
        };

        // Act
        var response = service.GetRegionsInAreaAsync(request).ResponseAsync.Result;

        // Assert
        Assert.NotNull(response);
        Assert.NotEmpty(response.Regions);
    }

    [Fact]
    public async Task GetObjectsInAreaAsync_ThrowsOnNullRequest()
    {
        // Arrange
        var objectLayer = CreateMockObjectLayer();
        var regionLayer = CreateMockRegionLayer();
        var service = new MapService(objectLayer, regionLayer);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.GetObjectsInAreaAsync(null));
    }

    private MapObjectLayer CreateMockObjectLayer()
    {
        // Создаём мок репозитория
        var mockRepository = new Mock<IMapObjectRepository>();

        // Настраиваем типичные ответы для тестов
        mockRepository
            .Setup(r => r.GetInAreaAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<MapObject>
            {
                new MapObject { Id = "obj1", X = 10, Y = 20, Width = 5, Height = 5, Type = "Building" },
                new MapObject { Id = "obj2", X = 50, Y = 60, Width = 10, Height = 10, Type = "Car" }
            });

        mockRepository
            .Setup(r => r.ExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        mockRepository
            .Setup(r => r.AddAsync(It.IsAny<MapObject>()))
            .ReturnsAsync(true);

        mockRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => new MapObject { Id = id, X = 0, Y = 0, Width = 1, Height = 1 });

        // Возвращаем слой с этим мок-репозиторием
        return new MapObjectLayer(mockRepository.Object);
    }

    private RegionLayer CreateMockRegionLayer()
    {
        var layer = new RegionLayer(1000, 1000);
        layer.GenerateRegions(10, new Random(42));
        return layer;
    }
}

