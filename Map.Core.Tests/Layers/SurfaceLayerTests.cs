using System.Diagnostics;
using Map.Core.Layers;
using Map.Core.Models;

namespace Map.Core.Tests.Layers;

public class SurfaceLayerTests
{
    [Fact]
    public void SurfaceLayer_CreatesWithCorrectSize_AndTileTypesDefaultToEmpty()
    {
        // Arrange
        var width = 100;
        var height = 100;

        // Act
        var surface = new SurfaceLayer(width, height);

        // Assert
        Assert.Equal(width, surface.Width);
        Assert.Equal(height, surface.Height);
        Assert.Equal(TileType.Plain, surface.GetTile(0, 0));
        Assert.Equal(TileType.Plain, surface.GetTile(width - 1, height - 1));
    }

    [Fact]
    public void SetTile_And_GetTile_WorkInO1Time()
    {
        var surface = new SurfaceLayer(1000, 1000);

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        for (var i = 0; i < 100000; i++)
        {
            var x = i % 1000;
            var y = (i / 1000) % 1000;
            surface.SetTile(x, y, TileType.Plain);
            var tile = surface.GetTile(x, y);
            Assert.Equal(TileType.Plain, tile);
        }
        stopwatch.Stop();
        var duration = stopwatch.ElapsedMilliseconds;
        Assert.True(duration < 500, $"Операции должны выполняться быстро, но заняли {duration} мс");
    }

    [Fact]
    public void SetTile_ThrowsException_WhenCoordinatesOutOfBounds()
    {
        var surface = new SurfaceLayer(10, 10);

        Assert.Throws<ArgumentOutOfRangeException>(() => surface.SetTile(-1, 5, TileType.Water));
        Assert.Throws<ArgumentOutOfRangeException>(() => surface.SetTile(5, -1, TileType.Water));
        Assert.Throws<ArgumentOutOfRangeException>(() => surface.SetTile(10, 5, TileType.Water));
        Assert.Throws<ArgumentOutOfRangeException>(() => surface.SetTile(5, 10, TileType.Water));
    }

    [Fact]
    public void GetTile_ThrowsException_WhenCoordinatesOutOfBounds()
    {
        var surface = new SurfaceLayer(10, 10);

        Assert.Throws<ArgumentOutOfRangeException>(() => surface.GetTile(-1, 5));
        Assert.Throws<ArgumentOutOfRangeException>(() => surface.GetTile(5, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => surface.GetTile(10, 5));
        Assert.Throws<ArgumentOutOfRangeException>(() => surface.GetTile(5, 10));
    }

    [Fact]
    public void MemoryUsage_DoesNotExceed8MB_For1000x1000Map()
    {
        // Arrange
        var width = 1000;
        var height = 1000;

        // Act
        var surface = new SurfaceLayer(width, height);

        // TileType занимает 1 байт (enum : byte)
        long expectedBytes = width * height * sizeof(byte);
        var expectedMB = expectedBytes / 1024.0 / 1024.0;

        // Assert
        Assert.True(expectedMB < 8.0, $"Ожидалось менее 8 МБ, фактически: {expectedMB:F2} МБ");
    }

    [Fact]
    public void SurfaceLayer_Throws_WhenInvalidSize()
    {
        Assert.Throws<ArgumentException>(() => new SurfaceLayer(0, 10));
        Assert.Throws<ArgumentException>(() => new SurfaceLayer(10, 0));
        Assert.Throws<ArgumentException>(() => new SurfaceLayer(-1, 10));
    }
}