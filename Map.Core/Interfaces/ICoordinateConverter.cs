namespace Map.Core.Interfaces;

/// <summary>
/// Интерфейс для преобразования прямоугольных координат карты в географические координаты для Redis
/// </summary>
public interface ICoordinateConverter
{
    /// <summary>
    /// Преобразует прямоугольные координаты (x, y) в географические (долгота, широта)
    /// </summary>
    /// <param name="x">X-координата на карте</param>
    /// <param name="y">Y-координата на карте</param>
    /// <returns>Кортеж (longitude, latitude)</returns>
    (double Longitude, double Latitude) ToGeoCoordinates(int x, int y);

    /// <summary>
    /// Преобразует географические координаты обратно в прямоугольные координаты карты
    /// </summary>
    /// <param name="longitude">Долгота</param>
    /// <param name="latitude">Широта</param>
    /// <returns>Кортеж (x, y)</returns>
    (int X, int Y) FromGeoCoordinates(double longitude, double latitude);

    /// <summary>
    /// Вычисляет радиус поиска в метрах для указанной области карты
    /// </summary>
    /// <param name="width">Ширина области в тайлах</param>
    /// <param name="height">Высота области в тайлах</param>
    /// <returns>Радиус в метрах</returns>
    double CalculateSearchRadius(int width, int height);
}
