using Map.Core.Models;

namespace Map.Core.Events;

/// <summary>
/// ��� ������� ������� �����
/// </summary>
public enum MapObjectEventType
{
    /// <summary>
    /// ������ ������
    /// </summary>
    Created,

    /// <summary>
    /// ������ �������
    /// </summary>
    Updated,

    /// <summary>
    /// ������ ������
    /// </summary>
    Removed
}

/// <summary>
/// ��������� ������� ��������� ������� �����
/// </summary>
public class MapObjectEventArgs : EventArgs
{
    /// <summary>
    /// ��� �������
    /// </summary>
    public MapObjectEventType EventType { get; }

    /// <summary>
    /// ������, � ������� ��������� �������
    /// </summary>
    public MapObject Object { get; }

    /// <summary>
    /// ���������� ��������� ������� (��� ������� Updated)
    /// </summary>
    public MapObject? PreviousState { get; }

    /// <summary>
    /// ��������� ����� �������
    /// </summary>
    public DateTime Timestamp { get; }

    public MapObjectEventArgs(MapObjectEventType eventType, MapObject obj, MapObject? previousState = null)
    {
        EventType = eventType;
        Object = obj ?? throw new ArgumentNullException(nameof(obj));
        PreviousState = previousState;
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// ������� ������� �������� �������
    /// </summary>
    public static MapObjectEventArgs Created(MapObject obj)
      => new(MapObjectEventType.Created, obj);

    /// <summary>
    /// ������� ������� ���������� �������
    /// </summary>
    public static MapObjectEventArgs Updated(MapObject obj, MapObject? previousState = null)
        => new(MapObjectEventType.Updated, obj, previousState);

    /// <summary>
    /// ������� ������� �������� �������
    /// </summary>
    public static MapObjectEventArgs Removed(MapObject obj)
        => new(MapObjectEventType.Removed, obj);
}
