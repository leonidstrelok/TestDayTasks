using Map.Core.Models;

namespace Map.Core.Interfaces;

public interface ICoordinateConverter
{
    (double Longitude, double Latitude) ToGeoCoordinates(int x, int y);
    (int X, int Y) FromGeoCoordinates(double longitude, double latitude);
    double CalculateSearchRadius(int width, int height);
    
    bool Contains(MapObject mapObject, int x, int y);
    bool Intersects(MapObject mapObject, int areaX, int areaY, int areaWidth, int areaHeight);
}
