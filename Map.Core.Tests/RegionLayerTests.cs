using Map.Core.Layers;

namespace Map.Core.Tests;

public class RegionLayerTests
{
    [Fact]
    public void Constructor_CreatesLayerWithCorrectSize()
    {
        var layer = new RegionLayer(100, 200);

        Assert.Equal(100, layer.Width);
        Assert.Equal(200, layer.Height);
    }

    [Fact]
    public void GenerateRegions_CreatesCorrectNumberOfRegions()
    {
        var layer = new RegionLayer(100, 100);
        layer.GenerateRegions(10, new Random(42));

        Assert.Equal(10, layer.Regions.Count);
    }

    [Fact]
    public void GenerateRegions_AllTilesBelongToSomeRegion()
    {
        var layer = new RegionLayer(50, 50);
        layer.GenerateRegions(5, new Random(42));

        for (var y = 0; y < layer.Height; y++)
        {
            for (var x = 0; x < layer.Width; x++)
            {
                var regionId = layer.GetRegionId(x, y);
                Assert.NotEqual(0, regionId);
                Assert.True(regionId <= 5);
            }
        }
    }

    [Fact]
    public void GenerateRegions_ExactAreaDistribution()
    {
        var layer = new RegionLayer(100, 100);
        layer.GenerateRegions(10);

        var totalTiles = 100 * 100;
        var expectedMin = totalTiles / 10;
        var expectedMax = expectedMin + 1; 

        foreach (var region in layer.Regions.Values)
        {
            Assert.InRange(region.TileCount, expectedMin, expectedMax);
        }
    }

    [Fact]
    public void GetRegionId_WorksCorrectly()
    {
        var layer = new RegionLayer(10, 10);
        layer.GenerateRegions(2, new Random(42));

        var regionId = layer.GetRegionId(5, 5);
        Assert.InRange(regionId, (ushort)1, (ushort)2);
    }

    [Fact]
    public void GetRegionId_ThrowsOnOutOfBounds()
    {
        var layer = new RegionLayer(10, 10);

        Assert.Throws<ArgumentOutOfRangeException>(() => layer.GetRegionId(-1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => layer.GetRegionId(10, 0));
    }

    [Fact]
    public void GetRegionIdSafe_ReturnsDefaultOnOutOfBounds()
    {
        var layer = new RegionLayer(10, 10);

        Assert.Equal(0, layer.GetRegionIdSafe(-1, 0));
        Assert.Equal(99, layer.GetRegionIdSafe(-1, 0, 99));
    }

    [Fact]
    public void GetRegionMetadata_ReturnsCorrectData()
    {
        var layer = new RegionLayer(10, 10);
        layer.GenerateRegions(2, new Random(42));

        var region = layer.GetRegionMetadata(1);

        Assert.NotNull(region);
        Assert.Equal(1, region.Id);
        Assert.NotNull(region.Name);
        Assert.True(region.TileCount > 0);
    }

    [Fact]
    public void TileBelongsToRegion_WorksCorrectly()
    {
        var layer = new RegionLayer(10, 10);
        layer.GenerateRegions(2, new Random(42));

        var regionId = layer.GetRegionId(5, 5);
        Assert.True(layer.TileBelongsToRegion(5, 5, regionId));

        var otherRegionId = (ushort)(regionId == 1 ? 2 : 1);
        Assert.False(layer.TileBelongsToRegion(5, 5, otherRegionId));
    }

    [Fact]
    public void GetRegionsInArea_ReturnsCorrectRegions()
    {
        var layer = new RegionLayer(100, 100);
        layer.GenerateRegions(10, new Random(42));

        var regionIds = layer.GetRegionsInArea(10, 10, 20, 20);

        Assert.NotEmpty(regionIds);
        Assert.All(regionIds, id => Assert.InRange(id, (ushort)1, (ushort)10));
    }

    [Fact]
    public void GetRegionsInArea_HandlesOutOfBounds()
    {
        var layer = new RegionLayer(10, 10);
        layer.GenerateRegions(2, new Random(42));

        var regionIds = layer.GetRegionsInArea(-5, -5, 20, 20);

        Assert.NotEmpty(regionIds);
    }

    [Fact]
    public void GetRegionMetadataInArea_ReturnsFullInfo()
    {
        var layer = new RegionLayer(50, 50);
        layer.GenerateRegions(5, new Random(42));

        var regions = layer.GetRegionMetadataInArea(0, 0, 50, 50);

        Assert.Equal(5, regions.Count);
        Assert.All(regions, r => Assert.NotNull(r.Name));
    }

    [Fact]
    public void SetRegionName_UpdatesName()
    {
        var layer = new RegionLayer(10, 10);
        layer.GenerateRegions(2, new Random(42));

        layer.SetRegionName(1, "Королевство Севера");

        var region = layer.GetRegionMetadata(1);
        Assert.Equal("Королевство Севера", region.Name);
    }

    [Fact]
    public void MemoryUsage_IsReasonable()
    {
        var layer = new RegionLayer(1000, 1000);
        layer.GenerateRegions(50, new Random(42));

        var memoryUsage = layer.GetMemoryUsage();

        Assert.True(memoryUsage < 8 * 1024 * 1024);
    }

    [Fact]
    public void GenerateRegions_IsIdempotent()
    {
        var layer = new RegionLayer(50, 50);

        layer.GenerateRegions(5, new Random(42));
        var firstState = new ushort[50, 50];
        for (var y = 0; y < 50; y++)
        for (var x = 0; x < 50; x++)
            firstState[y, x] = layer.GetRegionId(x, y);

        layer.GenerateRegions(5, new Random(42));
        var secondState = new ushort[50, 50];
        for (var y = 0; y < 50; y++)
        for (var x = 0; x < 50; x++)
            secondState[y, x] = layer.GetRegionId(x, y);

        Assert.Equal(firstState, secondState);
    }
}