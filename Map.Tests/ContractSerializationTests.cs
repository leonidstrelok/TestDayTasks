using Map.Shared.Events;
using Map.Shared.Models;
using MemoryPack;

namespace Map.Tests;

public class ContractSerializationTests
{
    [Fact]
    public void GetObjectsInAreaRequest_SerializesAndDeserializes()
    {
        // Arrange
        var request = new GetObjectsInAreaRequest
        {
            X1 = 10,
            Y1 = 20,
            X2 = 100,
            Y2 = 200
        };

        // Act
        var bytes = MemoryPackSerializer.Serialize(request);
        var deserialized = MemoryPackSerializer.Deserialize<GetObjectsInAreaRequest>(bytes);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(request.X1, deserialized.X1);
        Assert.Equal(request.Y1, deserialized.Y1);
        Assert.Equal(request.X2, deserialized.X2);
        Assert.Equal(request.Y2, deserialized.Y2);
    }

    [Fact]
    public void GetObjectsInAreaResponse_SerializesWithMultipleObjects()
    {
        // Arrange
        var response = new GetObjectsInAreaResponse
        {
            Objects = new List<ObjectDto>
            {
                new ObjectDto { Id = "obj1", X = 10, Y = 20, Width = 5, Height = 5, Type = "Building" },
                new ObjectDto { Id = "obj2", X = 30, Y = 40, Width = 10, Height = 10, Type = "Unit" },
                new ObjectDto { Id = "obj3", X = 50, Y = 60, Width = 2, Height = 2, Type = "Resource" }
            }
        };

        // Act
        var bytes = MemoryPackSerializer.Serialize(response);
        var deserialized = MemoryPackSerializer.Deserialize<GetObjectsInAreaResponse>(bytes);

        // Assert
        Assert.Equal(3, deserialized.Objects.Count);
        Assert.Equal("obj1", deserialized.Objects[0].Id);
        Assert.Equal("obj2", deserialized.Objects[1].Id);
        Assert.Equal("obj3", deserialized.Objects[2].Id);
    }

    [Fact]
    public void GetObjectsInAreaResponse_SerializesEmptyList()
    {
        // Arrange
        var response = new GetObjectsInAreaResponse
        {
            Objects = new List<ObjectDto>()
        };

        // Act
        var bytes = MemoryPackSerializer.Serialize(response);
        var deserialized = MemoryPackSerializer.Deserialize<GetObjectsInAreaResponse>(bytes);

        // Assert
        Assert.NotNull(deserialized.Objects);
        Assert.Empty(deserialized.Objects);
    }

    [Fact]
    public void GetRegionsInAreaRequest_SerializesCorrectly()
    {
        // Arrange
        var request = new GetRegionsInAreaRequest
        {
            X1 = 0,
            Y1 = 0,
            X2 = 500,
            Y2 = 500
        };

        // Act
        var bytes = MemoryPackSerializer.Serialize(request);
        var deserialized = MemoryPackSerializer.Deserialize<GetRegionsInAreaRequest>(bytes);

        // Assert
        Assert.Equal(request.X1, deserialized.X1);
        Assert.Equal(request.Y1, deserialized.Y1);
        Assert.Equal(request.X2, deserialized.X2);
        Assert.Equal(request.Y2, deserialized.Y2);
    }

    [Fact]
    public void GetRegionsInAreaResponse_SerializesWithRegions()
    {
        // Arrange
        var response = new GetRegionsInAreaResponse
        {
            Regions = new List<RegionDto>
            {
                new RegionDto { Id = 1, Name = "Kingdom North", TileCount = 10000 },
                new RegionDto { Id = 2, Name = "Empire South", TileCount = 15000 }
            }
        };

        // Act
        var bytes = MemoryPackSerializer.Serialize(response);
        var deserialized = MemoryPackSerializer.Deserialize<GetRegionsInAreaResponse>(bytes);

        // Assert
        Assert.Equal(2, deserialized.Regions.Count);
        Assert.Equal("Kingdom North", deserialized.Regions[0].Name);
        Assert.Equal(15000, deserialized.Regions[1].TileCount);
    }

    [Fact]
    public void ObjectDto_SerializesAllProperties()
    {
        // Arrange
        var dto = new ObjectDto
        {
            Id = "castle_123",
            X = 100,
            Y = 200,
            Width = 15,
            Height = 20,
            Type = "Castle"
        };

        // Act
        var bytes = MemoryPackSerializer.Serialize(dto);
        var deserialized = MemoryPackSerializer.Deserialize<ObjectDto>(bytes);

        // Assert
        Assert.Equal(dto.Id, deserialized.Id);
        Assert.Equal(dto.X, deserialized.X);
        Assert.Equal(dto.Y, deserialized.Y);
        Assert.Equal(dto.Width, deserialized.Width);
        Assert.Equal(dto.Height, deserialized.Height);
        Assert.Equal(dto.Type, deserialized.Type);
    }

    [Fact]
    public void RegionDto_SerializesCorrectly()
    {
        // Arrange
        var dto = new RegionDto
        {
            Id = 42,
            Name = "Test Region",
            TileCount = 5000
        };

        // Act
        var bytes = MemoryPackSerializer.Serialize(dto);
        var deserialized = MemoryPackSerializer.Deserialize<RegionDto>(bytes);

        // Assert
        Assert.Equal(dto.Id, deserialized.Id);
        Assert.Equal(dto.Name, deserialized.Name);
        Assert.Equal(dto.TileCount, deserialized.TileCount);
    }

    [Fact]
    public void ObjectAddedEvent_SerializesWithTimestamp()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var evt = new ObjectAddedEvent
        {
            Object = new ObjectDto { Id = "obj1", X = 10, Y = 20, Width = 5, Height = 5 },
            Timestamp = timestamp
        };

        // Act
        var bytes = MemoryPackSerializer.Serialize(evt);
        var deserialized = MemoryPackSerializer.Deserialize<ObjectAddedEvent>(bytes);

        // Assert
        Assert.Equal(evt.Object.Id, deserialized.Object.Id);
        Assert.Equal(timestamp.ToString(), deserialized.Timestamp.ToString());
    }

    [Fact]
    public void ObjectUpdatedEvent_SerializesCorrectly()
    {
        // Arrange
        var evt = new ObjectUpdatedEvent
        {
            Object = new ObjectDto { Id = "obj1", X = 50, Y = 60, Width = 10, Height = 10 },
            Timestamp = DateTime.UtcNow
        };

        // Act
        var bytes = MemoryPackSerializer.Serialize(evt);
        var deserialized = MemoryPackSerializer.Deserialize<ObjectUpdatedEvent>(bytes);

        // Assert
        Assert.Equal(evt.Object.Id, deserialized.Object.Id);
        Assert.Equal(evt.Object.X, deserialized.Object.X);
    }

    [Fact]
    public void ObjectDeletedEvent_SerializesWithoutFullObject()
    {
        // Arrange
        var evt = new ObjectDeletedEvent
        {
            ObjectId = "deleted_obj",
            Timestamp = DateTime.UtcNow
        };

        // Act
        var bytes = MemoryPackSerializer.Serialize(evt);
        var deserialized = MemoryPackSerializer.Deserialize<ObjectDeletedEvent>(bytes);

        // Assert
        Assert.Equal(evt.ObjectId, deserialized.ObjectId);
        Assert.NotEqual(default(DateTime), deserialized.Timestamp);
    }

    [Theory]
    [InlineData(-1000, -1000, 1000, 1000)]
    [InlineData(0, 0, 0, 0)]
    [InlineData(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue)]
    public void GetObjectsInAreaRequest_SerializesExtremeValues(int x1, int y1, int x2, int y2)
    {
        // Arrange
        var request = new GetObjectsInAreaRequest { X1 = x1, Y1 = y1, X2 = x2, Y2 = y2 };

        // Act
        var bytes = MemoryPackSerializer.Serialize(request);
        var deserialized = MemoryPackSerializer.Deserialize<GetObjectsInAreaRequest>(bytes);

        // Assert
        Assert.Equal(request.X1, deserialized.X1);
        Assert.Equal(request.Y1, deserialized.Y1);
        Assert.Equal(request.X2, deserialized.X2);
        Assert.Equal(request.Y2, deserialized.Y2);
    }
}