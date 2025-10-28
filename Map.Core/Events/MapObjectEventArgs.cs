using Map.Core.Models;

namespace Map.Core.Events;

/// <summary>
/// Тип события объекта карты
/// </summary>
public enum MapObjectEventType
{
    /// <summary>
    /// Объект создан
    /// </summary>
    Created,

    /// <summary>
    /// Объект изменен
    /// </summary>
    Updated,

    /// <summary>
    /// Объект удален
    /// </summary>
    Removed
}

/// <summary>
/// Аргументы события изменения объекта карты
/// </summary>
public class MapObjectEventArgs : EventArgs
{
    /// <summary>
    /// Тип события
    /// </summary>
    public MapObjectEventType EventType { get; }

    /// <summary>
    /// Объект, с которым произошло событие
    /// </summary>
    public MapObject Object { get; }

    /// <summary>
    /// Предыдущее состояние объекта (для события Updated)
    /// </summary>
    public MapObject? PreviousState { get; }

    /// <summary>
    /// Временная метка события
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
    /// Создает событие создания объекта
    /// </summary>
    public static MapObjectEventArgs Created(MapObject obj)
      => new(MapObjectEventType.Created, obj);

    /// <summary>
    /// Создает событие обновления объекта
    /// </summary>
    public static MapObjectEventArgs Updated(MapObject obj, MapObject? previousState = null)
        => new(MapObjectEventType.Updated, obj, previousState);

    /// <summary>
    /// Создает событие удаления объекта
    /// </summary>
    public static MapObjectEventArgs Removed(MapObject obj)
        => new(MapObjectEventType.Removed, obj);
}
