using MemoryPack;

namespace Map.Core.Models;

/// <summary>
/// ������������ ������ �� ����� � ������������ � ���������
/// </summary>
[MemoryPackable]
public partial class MapObject
{
    /// <summary>
    /// ���������� ������������� �������
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// X-���������� ������ �������� ���� �������
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Y-���������� ������ �������� ���� �������
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// ������ ������� � ������
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// ������ ������� � ������
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// ��� ������� (������, ����, ������ � �.�.)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// �������������� ���������� �������
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// ���������, ���������� �� ��������� ����� ������ �������
    /// </summary>
    public bool ContainsPoint(int x, int y)
    {
        return x >= X && x < X + Width && y >= Y && y < Y + Height;
    }

    /// <summary>
    /// ���������, ������������ �� ������ � ��������� ��������
    /// </summary>
    public bool IntersectsArea(int x, int y, int width, int height)
    {
        return !(X + Width <= x || X >= x + width || Y + Height <= y || Y >= y + height);
    }

    /// <summary>
    /// ���������, ��������� �� ������ ��������� ������ ��������� �������
    /// </summary>
    public bool IsFullyWithinArea(int x, int y, int width, int height)
    {
        return X >= x && Y >= y && X + Width <= x + width && Y + Height <= y + height;
    }

    /// <summary>
    /// �������� ����������� ����� �������
    /// </summary>
    public (int X, int Y) GetCenter()
    {
        return (X + Width / 2, Y + Height / 2);
    }
}
