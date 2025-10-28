using Map.Core.Layers;

namespace Map.Core.Tests.Layers;

public class RegionLayerTests
{
    [Fact]
    public void RegionLayer_CreatesWithCorrectSize()
    {
        var layer = new RegionLayer(100, 50);

        Assert.Equal(100, layer.Width);
        Assert.Equal(50, layer.Height);
        Assert.NotNull(layer.Regions);
        Assert.Empty(layer.Regions);
    }

    [Fact]
    public void GenerateRegions_CreatesExpectedRegions_AndAllTilesAssigned()
    {
        var layer = new RegionLayer(100, 100);
        layer.GenerateRegions(10);

        Assert.Equal(10, layer.Regions.Count);

        // Проверяем, что все тайлы принадлежат региону > 0
        for (int i = 0; i < 100; i++)
        for (int j = 0; j < 100; j++)
            Assert.InRange(layer.GetRegionId(i, j), (ushort)1, (ushort)10);
    }

    [Fact]
    public void GetRegionId_Throws_OnOutOfBounds()
    {
        var layer = new RegionLayer(10, 10);
        layer.GenerateRegions(2);

        Assert.Throws<ArgumentOutOfRangeException>(() => layer.GetRegionId(-1, 5));
        Assert.Throws<ArgumentOutOfRangeException>(() => layer.GetRegionId(10, 5));
    }

    [Fact]
    public void GetRegionIdSafe_ReturnsDefault_OnOutOfBounds()
    {
        var layer = new RegionLayer(10, 10);
        layer.GenerateRegions(2);

        var id = layer.GetRegionIdSafe(-1, 5, 999);
        Assert.Equal((ushort)999, id);
    }

    [Fact]
    public void GetRegionMetadata_ReturnsCorrectData()
    {
        var layer = new RegionLayer(50, 50);
        layer.GenerateRegions(5);

        var region = layer.GetRegionMetadata(3);
        Assert.NotNull(region);
        Assert.Equal((ushort)3, region!.Id);
        Assert.StartsWith("Регион", region.Name);
    }

    [Fact]
    public void GetRegionMetadataInArea_ReturnsOnlyIntersectingRegions()
    {
        var layer = new RegionLayer(100, 100);
        layer.GenerateRegions(10);

        var regions = layer.GetRegionMetadataInArea(0, 0, 30, 100);

        Assert.NotEmpty(regions);
        Assert.All(regions, r => Assert.NotNull(r));
    }

    [Fact]
    public void TileBelongsToRegion_WorksCorrectly()
    {
        var layer = new RegionLayer(20, 20);
        layer.GenerateRegions(4);

        var regionId = layer.GetRegionId(5, 5);
        Assert.True(layer.TileBelongsToRegion(5, 5, regionId));
        Assert.False(layer.TileBelongsToRegion(5, 5, (ushort)(regionId + 1)));
    }

    [Fact]
    public void AllRegions_AreEqualInSize_OrDifferByAtMostOneTile()
    {
        var layer = new RegionLayer(50, 50);
        layer.GenerateRegions(7);

        var counts = layer.Regions.Values.Select(r => r.TileCount).ToList();
        int min = counts.Min();
        int max = counts.Max();
        Assert.True(max - min <= 1, $"Разница между регионами не должна превышать 1 тайл (min={min}, max={max})");
    }

    [Fact]
    public void SetRegionName_ChangesName()
    {
        var layer = new RegionLayer(20, 20);
        layer.GenerateRegions(2);

        layer.SetRegionName(1, "CustomName");

        var region = layer.GetRegionMetadata(1);
        Assert.NotNull(region);
        Assert.Equal("CustomName", region!.Name);
    }

    [Fact]
    public void GetRegionsInArea_ReturnsCorrectIds()
    {
        var layer = new RegionLayer(50, 50);
        layer.GenerateRegions(5);

        var ids = layer.GetRegionsInArea(0, 0, 25, 50);
        Assert.NotEmpty(ids);
        Assert.All(ids, id => Assert.InRange(id, (ushort)1, (ushort)5));
    }

    [Fact]
    public void GetMemoryUsage_IsUnderLimit()
    {
        var layer = new RegionLayer(500, 500);
        layer.GenerateRegions(10);

        var memory = layer.GetMemoryUsage();
        Assert.True(memory < 8 * 1024 * 1024,
            $"Использование памяти слишком большое: {memory / 1024.0 / 1024.0:F2} МБ");
    }

    [Fact]
    public void GetRegionIdAndMetadata_WorkFast()
    {
        var layer = new RegionLayer(500, 500);
        layer.GenerateRegions(10);

        var start = DateTime.UtcNow;

        for (int i = 0; i < 100_000; i++)
        {
            int x = i % 500;
            int y = (i / 500) % 500;
            ushort id = layer.GetRegionId(x, y);
            var meta = layer.GetRegionMetadata(id);
            Assert.NotNull(meta);
        }

        var elapsed = (DateTime.UtcNow - start).TotalMilliseconds;
        Assert.True(elapsed < 300, $"Операции выполнялись слишком медленно: {elapsed:F2} мс");
    }
}