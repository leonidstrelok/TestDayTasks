using Map.Core.Services;

namespace Map.Core.Tests.Services;

public class CoordinateConverterTests
{
    private const int MapWidth = 1000;
    private const int MapHeight = 1000;

    [Fact]
    public void Constructor_WithValidDimensions_CreatesConverter()
    {
        // Act
        var converter = new CoordinateConverter(MapWidth, MapHeight);

        // Assert
        Assert.NotNull(converter);
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(100, 0)]
    [InlineData(-1, 100)]
    [InlineData(100, -1)]
    public void Constructor_WithInvalidDimensions_ThrowsArgumentException(int width, int height)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new CoordinateConverter(width, height));
    }

    [Fact]
    public void ToGeoCoordinates_WithValidCoordinates_ReturnsGeoCoordinates()
    {
        // Arrange
        var converter = new CoordinateConverter(MapWidth, MapHeight);
        var x = 100;
        var y = 200;

        // Act
        var (longitude, latitude) = converter.ToGeoCoordinates(x, y);

        // Assert
        Assert.True(longitude >= -180 && longitude <= 180);
        Assert.True(latitude >= -90 && latitude <= 90);
    }

    [Fact]
    public void ToGeoCoordinates_WithOrigin_ReturnsBaseCoordinates()
    {
        // Arrange
        var converter = new CoordinateConverter(MapWidth, MapHeight);

        // Act
        var (longitude, latitude) = converter.ToGeoCoordinates(0, 0);

        // Assert
        Assert.Equal(0.0, longitude);
        Assert.Equal(0.0, latitude);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(1001, 0)]
    [InlineData(0, 1001)]
    public void ToGeoCoordinates_WithOutOfBoundsCoordinates_ThrowsArgumentOutOfRangeException(int x, int y)
    {
        // Arrange
        var converter = new CoordinateConverter(MapWidth, MapHeight);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => converter.ToGeoCoordinates(x, y));
    }

    [Fact]
    public void FromGeoCoordinates_WithValidGeoCoordinates_ReturnsMapCoordinates()
    {
        // Arrange
        var converter = new CoordinateConverter(MapWidth, MapHeight);
        var longitude = 0.01;
        var latitude = 0.02;

        // Act
        var (x, y) = converter.FromGeoCoordinates(longitude, latitude);

        // Assert
        Assert.InRange(x, 0, MapWidth - 1);
        Assert.InRange(y, 0, MapHeight - 1);
    }

    [Fact]
    public void ToGeoCoordinates_AndBack_ReturnsOriginalCoordinates()
    {
        // Arrange
        var converter = new CoordinateConverter(MapWidth, MapHeight);
        var originalX = 100;
        var originalY = 200;

        // Act
        var (longitude, latitude) = converter.ToGeoCoordinates(originalX, originalY);
        var (x, y) = converter.FromGeoCoordinates(longitude, latitude);

        // Assert
        Assert.Equal(originalX, x);
        Assert.Equal(originalY, y);
    }

    [Fact]
    public void FromGeoCoordinates_WithOutOfBoundsGeoCoordinates_ClampsToMapBounds()
    {
        // Arrange
        var converter = new CoordinateConverter(MapWidth, MapHeight);
        var longitude = 1000.0; // ќчень большое значение
        var latitude = 1000.0;

        // Act
        var (x, y) = converter.FromGeoCoordinates(longitude, latitude);

        // Assert
        Assert.InRange(x, 0, MapWidth - 1);
        Assert.InRange(y, 0, MapHeight - 1);
    }

    [Fact]
    public void CalculateSearchRadius_WithValidDimensions_ReturnsPositiveRadius()
    {
        // Arrange
        var converter = new CoordinateConverter(MapWidth, MapHeight);
        var width = 10;
        var height = 10;

        // Act
        var radius = converter.CalculateSearchRadius(width, height);

        // Assert
        Assert.True(radius > 0);
    }

    [Theory]
    [InlineData(10, 10)]
    [InlineData(100, 100)]
    [InlineData(50, 100)]
    [InlineData(100, 50)]
    public void CalculateSearchRadius_WithDifferentDimensions_ReturnsAppropriateRadius(int width, int height)
    {
        // Arrange
        var converter = new CoordinateConverter(MapWidth, MapHeight);

        // Act
        var radius = converter.CalculateSearchRadius(width, height);

        // Assert
        Assert.True(radius > 0);

        // –адиус должен увеличиватьс€ с увеличением размеров
        var smallerRadius = converter.CalculateSearchRadius(width / 2, height / 2);
        Assert.True(radius > smallerRadius);
    }

    [Theory]
    [InlineData(-1, 10)]
    [InlineData(10, -1)]
    public void CalculateSearchRadius_WithNegativeDimensions_ThrowsArgumentException(int width, int height)
    {
        // Arrange
        var converter = new CoordinateConverter(MapWidth, MapHeight);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => converter.CalculateSearchRadius(width, height));
    }

    [Fact]
    public void GetAreaCenter_WithValidArea_ReturnsCenterGeoCoordinates()
    {
        // Arrange
        var converter = new CoordinateConverter(MapWidth, MapHeight);
        var x = 10;
        var y = 10;
        var width = 20;
        var height = 20;

        // Act
        var (longitude, latitude) = converter.GetAreaCenter(x, y, width, height);

        // Assert
        var expectedCenter = converter.ToGeoCoordinates(x + width / 2, y + height / 2);
        Assert.Equal(expectedCenter.Longitude, longitude);
        Assert.Equal(expectedCenter.Latitude, latitude);
    }

    [Fact]
    public void ToGeoCoordinates_MultiplePoints_AreUnique()
    {
        // Arrange
        var converter = new CoordinateConverter(MapWidth, MapHeight);
        var coordinates = new HashSet<(double, double)>();

        // Act
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                var geo = converter.ToGeoCoordinates(x, y);
                coordinates.Add(geo);
            }
        }

        // Assert - все координаты должны быть уникальными
        Assert.Equal(100, coordinates.Count);
    }

    [Fact]
    public void ToGeoCoordinates_AdjacentTiles_HaveConsistentDistances()
    {
        // Arrange
        var converter = new CoordinateConverter(MapWidth, MapHeight);

        var (lon1, lat1) = converter.ToGeoCoordinates(0, 0);
        var (lon2, lat2) = converter.ToGeoCoordinates(1, 0);
        var (lon3, lat3) = converter.ToGeoCoordinates(0, 1);

        // Act
        var horizontalDistance = Math.Abs(lon2 - lon1);
        var verticalDistance = Math.Abs(lat3 - lat1);

        // Assert - рассто€ни€ должны быть одинаковыми дл€ соседних тайлов
        Assert.Equal(horizontalDistance, verticalDistance, 10);
    }
}
