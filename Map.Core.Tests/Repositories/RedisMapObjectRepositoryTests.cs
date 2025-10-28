using Map.Core.Interfaces;
using Map.Core.Models;
using Map.Core.Repositories;
using Moq;
using StackExchange.Redis;

namespace Map.Core.Tests.Repositories;

public class RedisMapObjectRepositoryTests
{
    private readonly Mock<IConnectionMultiplexer> _redisMock;
    private readonly Mock<IDatabase> _databaseMock;
    private readonly Mock<ICoordinateConverter> _converterMock;
    private readonly RedisMapObjectRepository _repository;

    public RedisMapObjectRepositoryTests()
    {
        _redisMock = new Mock<IConnectionMultiplexer>();
        _databaseMock = new Mock<IDatabase>();
        _converterMock = new Mock<ICoordinateConverter>();

        _redisMock.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_databaseMock.Object);

        _repository = new RedisMapObjectRepository(_redisMock.Object, _converterMock.Object);
    }

    [Fact]
    public async Task AddAsync_WithValidObject_ReturnsTrue()
    {
        // Arrange
        var obj = CreateTestObject("obj1", 10, 20, 5, 5);
        SetupConverterMock(15, 22, 1.5, 2.2);
        SetupSuccessfulTransaction();

        // Act
        var result = await _repository.AddAsync(obj);

        // Assert
        Assert.True(result);
        _databaseMock.Verify(x => x.CreateTransaction(It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task AddAsync_WithNullObject_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.AddAsync(null!));
    }

    [Fact]
    public async Task AddAsync_WithEmptyId_ThrowsArgumentException()
    {
        // Arrange
        var obj = CreateTestObject("", 10, 20, 5, 5);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _repository.AddAsync(obj));
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsObject()
    {
        // Arrange
        var expectedObj = CreateTestObject("obj1", 10, 20, 5, 5);
        var serialized = MemoryPack.MemoryPackSerializer.Serialize(expectedObj);

        _databaseMock.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync((RedisValue)serialized);

        // Act
        var result = await _repository.GetByIdAsync("obj1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("obj1", result.Id);
        Assert.Equal(10, result.X);
        Assert.Equal(20, result.Y);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
    {
        // Arrange
        _databaseMock.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        // Act
        var result = await _repository.GetByIdAsync("nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WithEmptyId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _repository.GetByIdAsync(""));
    }

    [Fact]
    public async Task RemoveAsync_WithExistingId_ReturnsTrue()
    {
        // Arrange
        SetupSuccessfulTransaction();

        // Act
        var result = await _repository.RemoveAsync("obj1");

        // Assert
        Assert.True(result);
        _databaseMock.Verify(x => x.CreateTransaction(It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_WithEmptyId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _repository.RemoveAsync(""));
    }

    [Fact]
    public async Task GetByCoordinatesAsync_WithValidCoordinates_ReturnsMatchingObjects()
    {
        // Arrange
        var obj = CreateTestObject("obj1", 10, 20, 5, 5);
        var serialized = MemoryPack.MemoryPackSerializer.Serialize(obj);

        SetupConverterMock(10, 20, 1.0, 2.0);
        _converterMock.Setup(x => x.CalculateSearchRadius(2, 2))
 .Returns(100.0);

        var geoResults = new GeoRadiusResult[]
        {
   new GeoRadiusResult("obj1", 0.0, null, null)
        };

        _databaseMock.Setup(x => x.GeoRadiusAsync(
     It.IsAny<RedisKey>(),
              It.IsAny<double>(),
             It.IsAny<double>(),
     It.IsAny<double>(),
     It.IsAny<GeoUnit>(),
        It.IsAny<int>(),
            It.IsAny<Order>(),
     It.IsAny<GeoRadiusOptions>(),
       It.IsAny<CommandFlags>()))
            .ReturnsAsync(geoResults);

        _databaseMock.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
    .ReturnsAsync((RedisValue)serialized);

        // Act
        var results = await _repository.GetByCoordinatesAsync(12, 22);

        // Assert
        Assert.Single(results);
        Assert.Equal("obj1", results[0].Id);
    }

    [Fact]
    public async Task GetInAreaAsync_WithValidArea_ReturnsObjectsInArea()
    {
        // Arrange
        var obj = CreateTestObject("obj1", 10, 20, 5, 5);
        var serialized = MemoryPack.MemoryPackSerializer.Serialize(obj);

        SetupConverterMock(15, 22, 1.5, 2.2);
        _converterMock.Setup(x => x.CalculateSearchRadius(10, 10))
            .Returns(500.0);

        var geoResults = new GeoRadiusResult[]
         {
      new GeoRadiusResult("obj1", 0.0, null, null)
           };

        _databaseMock.Setup(x => x.GeoRadiusAsync(
          It.IsAny<RedisKey>(),
                   It.IsAny<double>(),
                 It.IsAny<double>(),
                  It.IsAny<double>(),
                      It.IsAny<GeoUnit>(),
                      It.IsAny<int>(),
                  It.IsAny<Order>(),
                        It.IsAny<GeoRadiusOptions>(),
            It.IsAny<CommandFlags>()))
                .ReturnsAsync(geoResults);

        _databaseMock.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)serialized);

        // Act
        var results = await _repository.GetInAreaAsync(10, 20, 10, 10);

        // Assert
        Assert.Single(results);
        Assert.Equal("obj1", results[0].Id);
    }

    [Fact]
    public async Task GetInAreaAsync_WithNegativeDimensions_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _repository.GetInAreaAsync(0, 0, -1, 10));
        await Assert.ThrowsAsync<ArgumentException>(() => _repository.GetInAreaAsync(0, 0, 10, -1));
    }

    [Fact]
    public async Task ExistsAsync_WithExistingId_ReturnsTrue()
    {
        // Arrange
        _databaseMock.Setup(x => x.KeyExistsAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        var result = await _repository.ExistsAsync("obj1");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistingId_ReturnsFalse()
    {
        // Arrange
        _databaseMock.Setup(x => x.KeyExistsAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
        .ReturnsAsync(false);

        // Act
        var result = await _repository.ExistsAsync("nonexistent");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingObject_ReturnsTrue()
    {
        // Arrange
        var obj = CreateTestObject("obj1", 10, 20, 5, 5);

        _databaseMock.Setup(x => x.KeyExistsAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
         .ReturnsAsync(true);

        SetupConverterMock(12, 22, 1.2, 2.2);
        SetupSuccessfulTransaction();

        // Act
        var result = await _repository.UpdateAsync(obj);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingObject_ReturnsFalse()
    {
        // Arrange
        var obj = CreateTestObject("obj1", 10, 20, 5, 5);

        _databaseMock.Setup(x => x.KeyExistsAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        // Act
        var result = await _repository.UpdateAsync(obj);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        _databaseMock.Setup(x => x.SetLengthAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
          .ReturnsAsync(42);

        // Act
        var count = await _repository.GetCountAsync();

        // Assert
        Assert.Equal(42, count);
    }

    [Fact]
    public async Task ClearAsync_RemovesAllObjects()
    {
        // Arrange
        var ids = new RedisValue[] { "obj1", "obj2", "obj3" };

        _databaseMock.Setup(x => x.SetMembersAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
       .ReturnsAsync(ids);

        var transactionMock = new Mock<ITransaction>();
        transactionMock.Setup(x => x.ExecuteAsync(It.IsAny<CommandFlags>()))
 .ReturnsAsync(true);

        _databaseMock.Setup(x => x.CreateTransaction(It.IsAny<object>()))
        .Returns(transactionMock.Object);

        // Act
        await _repository.ClearAsync();

        // Assert
        transactionMock.Verify(x => x.KeyDeleteAsync(
      It.IsAny<RedisKey>(),
         It.IsAny<CommandFlags>()),
            Times.AtLeast(3));
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

    private void SetupConverterMock(int x, int y, double longitude, double latitude)
    {
        _converterMock.Setup(c => c.ToGeoCoordinates(x, y))
               .Returns((longitude, latitude));
    }

    private void SetupSuccessfulTransaction()
    {
        var transactionMock = new Mock<ITransaction>();
        transactionMock.Setup(x => x.ExecuteAsync(It.IsAny<CommandFlags>()))
         .ReturnsAsync(true);
        transactionMock.Setup(x => x.GeoAddAsync(
          It.IsAny<RedisKey>(),
                      It.IsAny<double>(),
             It.IsAny<double>(),
        It.IsAny<RedisValue>(),
                It.IsAny<CommandFlags>()))
        .ReturnsAsync(true);
        transactionMock.Setup(x => x.StringSetAsync(
      It.IsAny<RedisKey>(),
        It.IsAny<RedisValue>(),
     It.IsAny<TimeSpan?>(),
     It.IsAny<bool>(),
          It.IsAny<When>(),
     It.IsAny<CommandFlags>()))
      .ReturnsAsync(true);
        transactionMock.Setup(x => x.SetAddAsync(
                It.IsAny<RedisKey>(),
        It.IsAny<RedisValue>(),
       It.IsAny<CommandFlags>()))
    .ReturnsAsync(true);
        transactionMock.Setup(x => x.GeoRemoveAsync(
 It.IsAny<RedisKey>(),
       It.IsAny<RedisValue>(),
        It.IsAny<CommandFlags>()))
      .ReturnsAsync(true);
        transactionMock.Setup(x => x.KeyDeleteAsync(
         It.IsAny<RedisKey>(),
       It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);
        transactionMock.Setup(x => x.SetRemoveAsync(
     It.IsAny<RedisKey>(),
    It.IsAny<RedisValue>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        _databaseMock.Setup(x => x.CreateTransaction(It.IsAny<object>()))
  .Returns(transactionMock.Object);
    }

    #endregion
}
