using Map.Core.Layers;
using Map.Core.Models;

namespace Map.Core.Tests;

public class SurfaceLayerTests
{
    [Fact]
    public void Constructor_CreatesMapWithCorrectSize()
    {
        var layer = new SurfaceLayer(100, 200);

        Assert.Equal(100, layer.Width);
        Assert.Equal(200, layer.Height);
    }

    [Fact]
    public void Constructor_ThrowsOnInvalidSize()
    {
        Assert.Throws<ArgumentException>(() => new SurfaceLayer(0, 100));
        Assert.Throws<ArgumentException>(() => new SurfaceLayer(100, -1));
    }

    [Fact]
    public void GetSetTile_WorksCorrectly()
    {
        var layer = new SurfaceLayer(10, 10);

        layer.SetTile(5, 5, TileType.Mountain);

        Assert.Equal(TileType.Mountain, layer.GetTile(5, 5));
        Assert.Equal(TileType.Plain, layer.GetTile(0, 0));
    }

    [Fact]
    public void GetTile_ThrowsOnOutOfBounds()
    {
        var layer = new SurfaceLayer(10, 10);

        Assert.Throws<ArgumentOutOfRangeException>(() => layer.GetTile(-1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => layer.GetTile(0, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => layer.GetTile(10, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => layer.GetTile(0, 10));
    }

    [Fact]
    public void GetTileSafe_ReturnsDefaultOnOutOfBounds()
    {
        var layer = new SurfaceLayer(10, 10);

        Assert.Equal(TileType.Plain, layer.GetTileSafe(-1, 0));
        Assert.Equal(TileType.Mountain, layer.GetTileSafe(-1, 0, TileType.Mountain));
    }

    [Fact]
    public void FillArea_FillsCorrectly()
    {
        var layer = new SurfaceLayer(10, 10);

        layer.FillArea(2, 2, 3, 3, TileType.Mountain);

        for (int y = 2; y < 5; y++)
        {
            for (int x = 2; x < 5; x++)
            {
                Assert.Equal(TileType.Mountain, layer.GetTile(x, y));
            }
        }

        Assert.Equal(TileType.Plain, layer.GetTile(0, 0));
        Assert.Equal(TileType.Plain, layer.GetTile(9, 9));
    }

    [Fact]
    public void FillArea_ClipsToMapBounds()
    {
        var layer = new SurfaceLayer(10, 10);

        layer.FillArea(-5, -5, 20, 20, TileType.Mountain);

        Assert.Equal(TileType.Mountain, layer.GetTile(0, 0));
        Assert.Equal(TileType.Mountain, layer.GetTile(9, 9));
    }

    [Fact]
    public void CanPlaceObjectInArea_ReturnsTrueForPlains()
    {
        var layer = new SurfaceLayer(10, 10);

        Assert.True(layer.CanPlaceObjectInArea(0, 0, 5, 5));
    }

    [Fact]
    public void CanPlaceObjectInArea_ReturnsFalseForMountains()
    {
        var layer = new SurfaceLayer(10, 10);
        layer.SetTile(5, 5, TileType.Mountain);

        Assert.False(layer.CanPlaceObjectInArea(4, 4, 3, 3));
    }

    [Fact]
    public void CanPlaceObjectInArea_ReturnsFalseForOutOfBounds()
    {
        var layer = new SurfaceLayer(10, 10);

        Assert.False(layer.CanPlaceObjectInArea(-1, 0, 5, 5));
        Assert.False(layer.CanPlaceObjectInArea(8, 8, 5, 5));
    }

    [Fact]
    public void FromTiles_CreatesCorrectMap()
    {
        var tiles = new[] { TileType.Plain, TileType.Mountain, TileType.Plain, TileType.Mountain };
        var layer = SurfaceLayer.FromTiles(2, 2, tiles);

        Assert.Equal(TileType.Plain, layer.GetTile(0, 0));
        Assert.Equal(TileType.Mountain, layer.GetTile(1, 0));
        Assert.Equal(TileType.Plain, layer.GetTile(0, 1));
        Assert.Equal(TileType.Mountain, layer.GetTile(1, 1));
    }

    [Fact]
    public void FromArray_CreatesCorrectMap()
    {
        var tiles = new TileType[2, 3]
        {
            { TileType.Plain, TileType.Mountain, TileType.Plain },
            { TileType.Mountain, TileType.Plain, TileType.Mountain }
        };

        var layer = SurfaceLayer.FromArray(tiles);

        Assert.Equal(3, layer.Width);
        Assert.Equal(2, layer.Height);
        Assert.Equal(TileType.Mountain, layer.GetTile(1, 0));
    }

    [Fact]
    public void MemoryUsage_IsWithinLimit()
    {
        var layer = new SurfaceLayer(1000, 1000);
        var memoryUsage = layer.GetMemoryUsage();

        Assert.True(memoryUsage < 8 * 1024 * 1024);
    }

    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(999, 999, true)]
    [InlineData(-1, 0, false)]
    [InlineData(0, -1, false)]
    [InlineData(1000, 0, false)]
    [InlineData(0, 1000, false)]
    public void IsInBounds_WorksCorrectly(int x, int y, bool expected)
    {
        var layer = new SurfaceLayer(1000, 1000);
        Assert.Equal(expected, layer.IsInBounds(x, y));
    }
}