using Map.Core.Events;
using Map.Core.Interfaces;
using Map.Core.Layers;
using Map.Core.Models;
using Moq;

namespace Map.Core.Tests.Layers;

public class MapObjectLayerTests
{
    private readonly Mock<IMapObjectRepository> _repositoryMock;
    private readonly MapObjectLayer _layer;

    public MapObjectLayerTests()
    {
        _repositoryMock = new Mock<IMapObjectRepository>();
        _layer = new MapObjectLayer(_repositoryMock.Object);
    }

    [Fact]
    public void Constructor_WithNullRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new MapObjectLayer(null!));
    }

    [Fact]
    public async Task AddObjectAsync_WithValidObject_ReturnsTrue()
    {
        // Arrange
        var obj = CreateTestObject("obj1", 10, 20, 5, 5);
        _repositoryMock.Setup(x => x.ExistsAsync("obj1")).ReturnsAsync(false);
        _repositoryMock.Setup(x => x.AddAsync(obj)).ReturnsAsync(true);

        // Act
        var result = await _layer.AddObjectAsync(obj);

        // Assert
        Assert.True(result);
        _repositoryMock.Verify(x => x.AddAsync(obj), Times.Once);
    }

    [Fact]
    public async Task AddObjectAsync_WithNullObject_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _layer.AddObjectAsync(null!));
    }

    [Fact]
    public async Task AddObjectAsync_WithExistingId_ThrowsInvalidOperationException()
    {
        // Arrange
        var obj = CreateTestObject("obj1", 10, 20, 5, 5);
        _repositoryMock.Setup(x => x.ExistsAsync("obj1")).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _layer.AddObjectAsync(obj));
    }

    [Fact]
    public async Task AddObjectAsync_RaisesObjectCreatedEvent()
    {
        // Arrange
        var obj = CreateTestObject("obj1", 10, 20, 5, 5);
        _repositoryMock.Setup(x => x.ExistsAsync("obj1")).ReturnsAsync(false);
        _repositoryMock.Setup(x => x.AddAsync(obj)).ReturnsAsync(true);

        MapObjectEventArgs? capturedEvent = null;
        _layer.ObjectCreated += (sender, e) => capturedEvent = e;

        // Act
        await _layer.AddObjectAsync(obj);

        // Assert
        Assert.NotNull(capturedEvent);
        Assert.Equal(MapObjectEventType.Created, capturedEvent.EventType);
        Assert.Equal("obj1", capturedEvent.Object.Id);
    }

    [Fact]
    public async Task AddObjectAsync_RaisesObjectChangedEvent()
    {
        // Arrange
        var obj = CreateTestObject("obj1", 10, 20, 5, 5);
        _repositoryMock.Setup(x => x.ExistsAsync("obj1")).ReturnsAsync(false);
        _repositoryMock.Setup(x => x.AddAsync(obj)).ReturnsAsync(true);

        MapObjectEventArgs? capturedEvent = null;
        _layer.ObjectChanged += (sender, e) => capturedEvent = e;

        // Act
        await _layer.AddObjectAsync(obj);

        // Assert
        Assert.NotNull(capturedEvent);
        Assert.Equal(MapObjectEventType.Created, capturedEvent.EventType);
    }

    [Fact]
    public async Task GetObjectByIdAsync_WithExistingId_ReturnsObject()
    {
        // Arrange
        var obj = CreateTestObject("obj1", 10, 20, 5, 5);
        _repositoryMock.Setup(x => x.GetByIdAsync("obj1")).ReturnsAsync(obj);

        // Act
        var result = await _layer.GetObjectByIdAsync("obj1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("obj1", result.Id);
    }

    [Fact]
    public async Task GetObjectByIdAsync_WithNonExistingId_ReturnsNull()
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetByIdAsync("nonexistent")).ReturnsAsync((MapObject?)null);

        // Act
        var result = await _layer.GetObjectByIdAsync("nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetObjectByIdAsync_WithEmptyId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _layer.GetObjectByIdAsync(""));
    }

    [Fact]
    public async Task RemoveObjectAsync_WithExistingId_ReturnsTrue()
    {
        // Arrange
        var obj = CreateTestObject("obj1", 10, 20, 5, 5);
        _repositoryMock.Setup(x => x.GetByIdAsync("obj1")).ReturnsAsync(obj);
        _repositoryMock.Setup(x => x.RemoveAsync("obj1")).ReturnsAsync(true);

        // Act
        var result = await _layer.RemoveObjectAsync("obj1");

        // Assert
        Assert.True(result);
        _repositoryMock.Verify(x => x.RemoveAsync("obj1"), Times.Once);
    }

    [Fact]
    public async Task RemoveObjectAsync_WithNonExistingId_ReturnsFalse()
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetByIdAsync("nonexistent")).ReturnsAsync((MapObject?)null);

        // Act
        var result = await _layer.RemoveObjectAsync("nonexistent");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RemoveObjectAsync_RaisesObjectRemovedEvent()
    {
        // Arrange
        var obj = CreateTestObject("obj1", 10, 20, 5, 5);
        _repositoryMock.Setup(x => x.GetByIdAsync("obj1")).ReturnsAsync(obj);
        _repositoryMock.Setup(x => x.RemoveAsync("obj1")).ReturnsAsync(true);

        MapObjectEventArgs? capturedEvent = null;
        _layer.ObjectRemoved += (sender, e) => capturedEvent = e;

        // Act
        await _layer.RemoveObjectAsync("obj1");

        // Assert
        Assert.NotNull(capturedEvent);
        Assert.Equal(MapObjectEventType.Removed, capturedEvent.EventType);
        Assert.Equal("obj1", capturedEvent.Object.Id);
    }

    [Fact]
    public async Task UpdateObjectAsync_WithExistingObject_ReturnsTrue()
    {
        // Arrange
        var oldObj = CreateTestObject("obj1", 10, 20, 5, 5);
        var newObj = CreateTestObject("obj1", 15, 25, 6, 6);

        _repositoryMock.Setup(x => x.GetByIdAsync("obj1")).ReturnsAsync(oldObj);
        _repositoryMock.Setup(x => x.UpdateAsync(newObj)).ReturnsAsync(true);

        // Act
        var result = await _layer.UpdateObjectAsync(newObj);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UpdateObjectAsync_WithNonExistingObject_ThrowsInvalidOperationException()
    {
        // Arrange
        var obj = CreateTestObject("obj1", 10, 20, 5, 5);
        _repositoryMock.Setup(x => x.GetByIdAsync("obj1")).ReturnsAsync((MapObject?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _layer.UpdateObjectAsync(obj));
    }

    [Fact]
    public async Task UpdateObjectAsync_RaisesObjectUpdatedEvent()
    {
        // Arrange
        var oldObj = CreateTestObject("obj1", 10, 20, 5, 5);
        var newObj = CreateTestObject("obj1", 15, 25, 6, 6);

        _repositoryMock.Setup(x => x.GetByIdAsync("obj1")).ReturnsAsync(oldObj);
        _repositoryMock.Setup(x => x.UpdateAsync(newObj)).ReturnsAsync(true);

        MapObjectEventArgs? capturedEvent = null;
        _layer.ObjectUpdated += (sender, e) => capturedEvent = e;

        // Act
        await _layer.UpdateObjectAsync(newObj);

        // Assert
        Assert.NotNull(capturedEvent);
        Assert.Equal(MapObjectEventType.Updated, capturedEvent.EventType);
        Assert.Equal("obj1", capturedEvent.Object.Id);
        Assert.NotNull(capturedEvent.PreviousState);
    }

    [Fact]
    public async Task GetObjectsByCoordinatesAsync_ReturnsMatchingObjects()
    {
        // Arrange
        var objects = new List<MapObject>
        {
  CreateTestObject("obj1", 10, 20, 5, 5),
        CreateTestObject("obj2", 10, 20, 5, 5)
        };
        _repositoryMock.Setup(x => x.GetByCoordinatesAsync(12, 22)).ReturnsAsync(objects);

        // Act
        var results = await _layer.GetObjectsByCoordinatesAsync(12, 22);

        // Assert
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task GetObjectsInAreaTaskAsync_WithValidArea_ReturnsObjects()
    {
        // Arrange
        var objects = new List<MapObject>
{
            CreateTestObject("obj1", 10, 20, 5, 5),
   CreateTestObject("obj2", 15, 25, 5, 5)
        };
        _repositoryMock.Setup(x => x.GetInAreaAsync(10, 20, 20, 20)).ReturnsAsync(objects);

        // Act
        var results = await _layer.GetObjectsInAreaTaskAsync(10, 20, 20, 20);

        // Assert
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task GetObjectsInAreaTaskAsync_WithNegativeDimensions_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _layer.GetObjectsInAreaTaskAsync(0, 0, -1, 10));
    }

    [Fact]
    public async Task ObjectExistsAsync_WithExistingId_ReturnsTrue()
    {
        // Arrange
        _repositoryMock.Setup(x => x.ExistsAsync("obj1")).ReturnsAsync(true);

        // Act
        var result = await _layer.ObjectExistsAsync("obj1");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ObjectExistsAsync_WithNonExistingId_ReturnsFalse()
    {
        // Arrange
        _repositoryMock.Setup(x => x.ExistsAsync("nonexistent")).ReturnsAsync(false);

        // Act
        var result = await _layer.ObjectExistsAsync("nonexistent");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsObjectInAreaAsync_WithObjectInArea_ReturnsTrue()
    {
        // Arrange
        var obj = CreateTestObject("obj1", 10, 20, 5, 5);
        _repositoryMock.Setup(x => x.GetByIdAsync("obj1")).ReturnsAsync(obj);

        // Act
        var result = await _layer.IsObjectInAreaAsync("obj1", 8, 18, 10, 10);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsObjectInAreaAsync_WithObjectOutsideArea_ReturnsFalse()
    {
        // Arrange
        var obj = CreateTestObject("obj1", 10, 20, 5, 5);
        _repositoryMock.Setup(x => x.GetByIdAsync("obj1")).ReturnsAsync(obj);

        // Act
        var result = await _layer.IsObjectInAreaAsync("obj1", 50, 50, 10, 10);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsObjectInAreaAsync_WithNonExistingObject_ReturnsFalse()
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetByIdAsync("nonexistent")).ReturnsAsync((MapObject?)null);

        // Act
        var result = await _layer.IsObjectInAreaAsync("nonexistent", 0, 0, 10, 10);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetObjectCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetCountAsync()).ReturnsAsync(10);

        // Act
        var count = await _layer.GetObjectCountAsync();

        // Assert
        Assert.Equal(10, count);
    }

    [Fact]
    public async Task ClearAllObjectsAsync_CallsRepositoryClear()
    {
        // Arrange
        _repositoryMock.Setup(x => x.ClearAsync()).Returns(Task.CompletedTask);

        // Act
        await _layer.ClearAllObjectsAsync();

        // Assert
        _repositoryMock.Verify(x => x.ClearAsync(), Times.Once);
    }

    #region Helper Methods

    private MapObject CreateTestObject(string id, int x, int y, int width, int height)
    {
        return new MapObject
        {
            Id = id,
            X = x,
            Y = y,
            Width = width,
            Height = height,
            Type = "TestType"
        };
    }

    #endregion
}
