using Map.Core.Interfaces;
using Map.Core.Models;
using Map.Core.Repositories;
using Moq;
using StackExchange.Redis;

namespace Map.Core.Tests.Repositories;

public class RedisMapObjectRepositoryTests_Extended
{
    private readonly Mock<IConnectionMultiplexer> _redisMock;
    private readonly Mock<IDatabase> _databaseMock;
    private readonly Mock<ICoordinateConverter> _converterMock;
    private readonly RedisMapObjectRepository _repository;

    public RedisMapObjectRepositoryTests_Extended()
    {
        _redisMock = new Mock<IConnectionMultiplexer>();
        _databaseMock = new Mock<IDatabase>();
        _converterMock = new Mock<ICoordinateConverter>();

        _redisMock.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_databaseMock.Object);

        _repository = new RedisMapObjectRepository(_redisMock.Object, _converterMock.Object);
    }

    [Fact]
    public async Task AddAsync_WhenRedisThrows_ThrowsInvalidOperationException()
    {
        // Arrange
        var obj = CreateTestObject("obj1", 10, 20, 5, 5);
        SetupConverterMock(15, 22, 1.5, 2.2);

        _databaseMock.Setup(x => x.CreateTransaction(It.IsAny<object>()))
            .Throws(new RedisException("Redis error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.AddAsync(obj));
    }

    [Fact]
    public async Task GetByIdAsync_WhenDataIsCorrupted_ThrowsInvalidOperationException()
    {
        // Arrange
        _databaseMock.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)new byte[] { 0x01, 0x02, 0x03 }); // повреждённые данные

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.GetByIdAsync("obj1"));
    }

    [Fact]
    public async Task RemoveAsync_WhenTransactionFails_ThrowsInvalidOperationException()
    {
        // Arrange
        _databaseMock.Setup(x => x.CreateTransaction(It.IsAny<object>()))
            .Throws(new RedisTimeoutException("Timeout", CommandStatus.Unknown));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.RemoveAsync("obj1"));
    }

    [Fact]
    public async Task GetByCoordinatesAsync_WhenConverterFails_ThrowsInvalidOperationException()
    {
        // Arrange
        _converterMock.Setup(x => x.ToGeoCoordinates(It.IsAny<int>(), It.IsAny<int>()))
            .Throws(new Exception("Conversion failed"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.GetByCoordinatesAsync(10, 20));
    }

    [Fact]
    public async Task GetInAreaAsync_WhenRedisFails_ThrowsInvalidOperationException()
    {
        // Arrange
        SetupConverterMock(5, 5, 1.0, 2.0);
        _converterMock.Setup(x => x.CalculateSearchRadius(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(100.0);

        // Настраиваем полную сигнатуру с указанием всех параметров
        _databaseMock.Setup(x => x.GeoRadiusAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<double>(),
                It.IsAny<double>(),
                It.IsAny<double>(),
                It.IsAny<GeoUnit>(),
                It.IsAny<int>(),
                It.IsAny<Order?>(),
                It.IsAny<GeoRadiusOptions>(),
                It.IsAny<CommandFlags>()))
            .Throws(new RedisConnectionException(ConnectionFailureType.SocketFailure, "Connection failed"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.GetInAreaAsync(0, 0, 10, 10));
    }



    [Fact]
    public async Task ExistsAsync_WithEmptyId_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _repository.ExistsAsync(""));
        Assert.Contains("cannot be empty", ex.Message);
    }

    [Fact]
    public async Task ClearAsync_WhenRedisThrowsDuringDelete_ThrowsInvalidOperationException()
    {
        // Arrange
        _databaseMock.Setup(x => x.SetMembersAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .Throws(new RedisException("Internal error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.ClearAsync());
    }

    [Fact]
    public async Task UpdateAsync_WhenRemoveThrows_ThrowsInvalidOperationException()
    {
        // Arrange
        var obj = CreateTestObject("obj1", 10, 20, 5, 5);
        _databaseMock.Setup(x => x.KeyExistsAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        _databaseMock.Setup(x => x.CreateTransaction(It.IsAny<object>()))
            .Throws(new RedisException("Remove failed"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.UpdateAsync(obj));
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

    #endregion
}
