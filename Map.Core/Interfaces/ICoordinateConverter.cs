namespace Map.Core.Interfaces;

/// <summary>
/// ��������� ��� �������������� ������������� ��������� ����� � �������������� ���������� ��� Redis
/// </summary>
public interface ICoordinateConverter
{
    /// <summary>
    /// ����������� ������������� ���������� (x, y) � �������������� (�������, ������)
    /// </summary>
    /// <param name="x">X-���������� �� �����</param>
    /// <param name="y">Y-���������� �� �����</param>
    /// <returns>������ (longitude, latitude)</returns>
    (double Longitude, double Latitude) ToGeoCoordinates(int x, int y);

    /// <summary>
    /// ����������� �������������� ���������� ������� � ������������� ���������� �����
    /// </summary>
    /// <param name="longitude">�������</param>
    /// <param name="latitude">������</param>
    /// <returns>������ (x, y)</returns>
    (int X, int Y) FromGeoCoordinates(double longitude, double latitude);

    /// <summary>
    /// ��������� ������ ������ � ������ ��� ��������� ������� �����
    /// </summary>
    /// <param name="width">������ ������� � ������</param>
    /// <param name="height">������ ������� � ������</param>
    /// <returns>������ � ������</returns>
    double CalculateSearchRadius(int width, int height);
}
