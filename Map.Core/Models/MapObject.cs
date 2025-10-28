using MemoryPack;

namespace Map.Core.Models;

/// <summary>
/// Представляет объект на карте с координатами и размерами
/// </summary>
[MemoryPackable]
public partial class MapObject
{
    /// <summary>
    /// Уникальный идентификатор объекта
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// X-координата левого верхнего угла объекта
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Y-координата левого верхнего угла объекта
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// Ширина объекта в тайлах
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Высота объекта в тайлах
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Тип объекта (здание, юнит, ресурс и т.д.)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Дополнительные метаданные объекта
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Проверяет, содержится ли указанная точка внутри объекта
    /// </summary>
    public bool ContainsPoint(int x, int y)
    {
        return x >= X && x < X + Width && y >= Y && y < Y + Height;
    }

    /// <summary>
    /// Проверяет, пересекается ли объект с указанной областью
    /// </summary>
    public bool IntersectsArea(int x, int y, int width, int height)
    {
        return !(X + Width <= x || X >= x + width || Y + Height <= y || Y >= y + height);
    }

    /// <summary>
    /// Проверяет, полностью ли объект находится внутри указанной области
    /// </summary>
    public bool IsFullyWithinArea(int x, int y, int width, int height)
    {
        return X >= x && Y >= y && X + Width <= x + width && Y + Height <= y + height;
    }

    /// <summary>
    /// Получает центральную точку объекта
    /// </summary>
    public (int X, int Y) GetCenter()
    {
        return (X + Width / 2, Y + Height / 2);
    }
}
