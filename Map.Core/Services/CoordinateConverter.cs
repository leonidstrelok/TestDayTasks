using Map.Core.Interfaces;

namespace Map.Core.Services;

/// <summary>
/// ����������� ������������� ���������� ����� � �������������� ���������� ��� Redis
/// ���������� ��������������� ��� ���������� �������� � ����������� ������������ ���������
/// </summary>
public class CoordinateConverter : ICoordinateConverter
{
    private const double BaseLongitude = 0.0;
    private const double BaseLatitude = 0.0;

    // ���������� �����������: 1 ���� = 0.0001 ������� (�������� 11 ������ �� ��������)
    // ��� ������������ ������������ ��������� � ���������� �������� ��� Redis GEORADIUS
    private const double ScaleFactor = 0.0001;

    // ��������������� ���������� ������ �� ������ ������ (���������)
    private const double MetersPerDegree = 111320.0;

    private readonly int _mapWidth;
    private readonly int _mapHeight;

    /// <summary>
    /// ������� ��������� ��� ����� ��������� �������
    /// </summary>
    /// <param name="mapWidth">������ ����� � ������</param>
    /// <param name="mapHeight">������ ����� � ������</param>
    public CoordinateConverter(int mapWidth, int mapHeight)
    {
        if (mapWidth <= 0 || mapHeight <= 0)
            throw new ArgumentException("������� ����� ������ ���� ��������������");

        _mapWidth = mapWidth;
        _mapHeight = mapHeight;
    }

    /// <inheritdoc />
    public (double Longitude, double Latitude) ToGeoCoordinates(int x, int y)
    {
        if (x < 0 || x >= _mapWidth || y < 0 || y >= _mapHeight)
            throw new ArgumentOutOfRangeException($"���������� ({x}, {y}) ������� �� ������� ����� ({_mapWidth}, {_mapHeight})");

        // ����������� ������������� ���������� � ��������������
        // X -> Longitude (�������), Y -> Latitude (������)
        var longitude = BaseLongitude + (x * ScaleFactor);
        var latitude = BaseLatitude + (y * ScaleFactor);

        return (longitude, latitude);
    }

    /// <inheritdoc />
    public (int X, int Y) FromGeoCoordinates(double longitude, double latitude)
    {
        // �������� �������������� �� �������������� ��������� � �������������
        var x = (int)Math.Round((longitude - BaseLongitude) / ScaleFactor);
        var y = (int)Math.Round((latitude - BaseLatitude) / ScaleFactor);

        // ������������ ���������� ��������� �����
        x = Math.Max(0, Math.Min(_mapWidth - 1, x));
        y = Math.Max(0, Math.Min(_mapHeight - 1, y));

        return (x, y);
    }

    /// <inheritdoc />
    public double CalculateSearchRadius(int width, int height)
    {
        if (width < 0 || height < 0)
            throw new ArgumentException("������� ������� ������ ���� ����������������");

        // ��������� ��������� ������� � ��������
        var widthDegrees = width * ScaleFactor;
        var heightDegrees = height * ScaleFactor;
        var diagonalDegrees = Math.Sqrt(widthDegrees * widthDegrees + heightDegrees * heightDegrees);

        // ����������� � ����� (���������� ��������� ��� ���������)
        // ��� ����� ������� ������� ����� ��������� ������, �� ��� ������� ����� ��� ���������
        var radiusMeters = diagonalDegrees * MetersPerDegree / 2.0;

        // ��������� ��������� ����� ��� ����������� ������������ ����������
        return radiusMeters * 1.1;
    }

    /// <summary>
    /// �������� �������������� ���������� ������ �������
    /// </summary>
    public (double Longitude, double Latitude) GetAreaCenter(int x, int y, int width, int height)
    {
        var centerX = x + width / 2;
        var centerY = y + height / 2;
        return ToGeoCoordinates(centerX, centerY);
    }
}
