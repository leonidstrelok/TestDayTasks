using Map.Core.Interfaces;
using Map.Core.Models;

namespace Map.Core.Services;

public class CoordinateConverter : ICoordinateConverter
{
    private const double BaseLongitude = 0.0;
    private const double BaseLatitude = 0.0;

    private const double ScaleFactor = 0.0001;

    private const double MetersPerDegree = 111320.0;

    private readonly int _mapWidth;
    private readonly int _mapHeight;

    public CoordinateConverter(int mapWidth, int mapHeight)
    {
        if (mapWidth <= 0 || mapHeight <= 0)
            throw new ArgumentException();

        _mapWidth = mapWidth;
        _mapHeight = mapHeight;
    }

    /// <inheritdoc />
    public (double Longitude, double Latitude) ToGeoCoordinates(int x, int y)
    {
        if (x < 0 || x >= _mapWidth || y < 0 || y >= _mapHeight)
            throw new ArgumentOutOfRangeException($"({x}, {y})  ({_mapWidth}, {_mapHeight})");

        var longitude = BaseLongitude + (x * ScaleFactor);
        var latitude = BaseLatitude + (y * ScaleFactor);

        return (longitude, latitude);
    }

    public (int X, int Y) FromGeoCoordinates(double longitude, double latitude)
    {
        var x = (int)Math.Round((longitude - BaseLongitude) / ScaleFactor);
        var y = (int)Math.Round((latitude - BaseLatitude) / ScaleFactor);

        x = Math.Max(0, Math.Min(_mapWidth - 1, x));
        y = Math.Max(0, Math.Min(_mapHeight - 1, y));

        return (x, y);
    }

    public double CalculateSearchRadius(int width, int height)
    {
        if (width < 0 || height < 0)
            throw new ArgumentException();

        var widthDegrees = width * ScaleFactor;
        var heightDegrees = height * ScaleFactor;
        var diagonalDegrees = Math.Sqrt(widthDegrees * widthDegrees + heightDegrees * heightDegrees);

        var radiusMeters = diagonalDegrees * MetersPerDegree / 2.0;

        return radiusMeters * 1.1;
    }

    public bool Contains(MapObject mapObject, int x, int y)
    {
        if (mapObject == null)
            throw new ArgumentNullException(nameof(mapObject));

        return x >= mapObject.X &&
               x < mapObject.X + mapObject.Width &&
               y >= mapObject.Y &&
               y < mapObject.Y + mapObject.Height;
    }

    public bool Intersects(MapObject mapObject, int areaX, int areaY, int areaWidth, int areaHeight)
    {
        if (mapObject == null)
            throw new ArgumentNullException(nameof(mapObject));

        if (areaWidth <= 0 || areaHeight <= 0)
            throw new ArgumentException("Area dimensions must be positive");

        var areaRight = areaX + areaWidth;
        var areaBottom = areaY + areaHeight;

        var objectRight = mapObject.X + mapObject.Width;
        var objectBottom = mapObject.Y + mapObject.Height;

        var intersectsX = mapObject.X < areaRight && objectRight > areaX;
        var intersectsY = mapObject.Y < areaBottom && objectBottom > areaY;

        return intersectsX && intersectsY;
    }

    public (double Longitude, double Latitude) GetAreaCenter(int x, int y, int width, int height)
    {
        var centerX = x + width / 2;
        var centerY = y + height / 2;
        return ToGeoCoordinates(centerX, centerY);
    }
}