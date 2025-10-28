using Map.Core.Interfaces;

namespace Map.Core.Services;

/// <summary>
/// Преобразует прямоугольные координаты карты в географические координаты для Redis
/// Использует масштабирование для сохранения точности и обеспечения уникальности координат
/// </summary>
public class CoordinateConverter : ICoordinateConverter
{
    private const double BaseLongitude = 0.0;
    private const double BaseLatitude = 0.0;

    // Масштабный коэффициент: 1 тайл = 0.0001 градуса (примерно 11 метров на экваторе)
    // Это обеспечивает уникальность координат и приемлемую точность для Redis GEORADIUS
    private const double ScaleFactor = 0.0001;

    // Приблизительное количество метров на градус широты (константа)
    private const double MetersPerDegree = 111320.0;

    private readonly int _mapWidth;
    private readonly int _mapHeight;

    /// <summary>
    /// Создает конвертер для карты заданного размера
    /// </summary>
    /// <param name="mapWidth">Ширина карты в тайлах</param>
    /// <param name="mapHeight">Высота карты в тайлах</param>
    public CoordinateConverter(int mapWidth, int mapHeight)
    {
        if (mapWidth <= 0 || mapHeight <= 0)
            throw new ArgumentException("Размеры карты должны быть положительными");

        _mapWidth = mapWidth;
        _mapHeight = mapHeight;
    }

    /// <inheritdoc />
    public (double Longitude, double Latitude) ToGeoCoordinates(int x, int y)
    {
        if (x < 0 || x >= _mapWidth || y < 0 || y >= _mapHeight)
            throw new ArgumentOutOfRangeException($"Координаты ({x}, {y}) выходят за границы карты ({_mapWidth}, {_mapHeight})");

        // Преобразуем прямоугольные координаты в географические
        // X -> Longitude (долгота), Y -> Latitude (широта)
        var longitude = BaseLongitude + (x * ScaleFactor);
        var latitude = BaseLatitude + (y * ScaleFactor);

        return (longitude, latitude);
    }

    /// <inheritdoc />
    public (int X, int Y) FromGeoCoordinates(double longitude, double latitude)
    {
        // Обратное преобразование из географических координат в прямоугольные
        var x = (int)Math.Round((longitude - BaseLongitude) / ScaleFactor);
        var y = (int)Math.Round((latitude - BaseLatitude) / ScaleFactor);

        // Ограничиваем координаты границами карты
        x = Math.Max(0, Math.Min(_mapWidth - 1, x));
        y = Math.Max(0, Math.Min(_mapHeight - 1, y));

        return (x, y);
    }

    /// <inheritdoc />
    public double CalculateSearchRadius(int width, int height)
    {
        if (width < 0 || height < 0)
            throw new ArgumentException("Размеры области должны быть неотрицательными");

        // Вычисляем диагональ области в градусах
        var widthDegrees = width * ScaleFactor;
        var heightDegrees = height * ScaleFactor;
        var diagonalDegrees = Math.Sqrt(widthDegrees * widthDegrees + heightDegrees * heightDegrees);

        // Преобразуем в метры (используем константу для упрощения)
        // Для более точного расчета можно учитывать широту, но для игровой карты это избыточно
        var radiusMeters = diagonalDegrees * MetersPerDegree / 2.0;

        // Добавляем небольшой запас для компенсации погрешностей округления
        return radiusMeters * 1.1;
    }

    /// <summary>
    /// Получает географические координаты центра области
    /// </summary>
    public (double Longitude, double Latitude) GetAreaCenter(int x, int y, int width, int height)
    {
        var centerX = x + width / 2;
        var centerY = y + height / 2;
        return ToGeoCoordinates(centerX, centerY);
    }
}
