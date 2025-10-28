using Map.Core.Models;

namespace Map.Core.Events;


public enum MapObjectEventType
{
    Created,
    Updated,
    Removed
}

public class MapObjectEventArgs : EventArgs
{
    public MapObjectEventType EventType { get; }
    
    public MapObject Object { get; }
    
    public MapObject? PreviousState { get; }
    
    public DateTime Timestamp { get; }

    public MapObjectEventArgs(MapObjectEventType eventType, MapObject obj, MapObject? previousState = null)
    {
        EventType = eventType;
        Object = obj ?? throw new ArgumentNullException(nameof(obj));
        PreviousState = previousState;
        Timestamp = DateTime.UtcNow;
    }
    
    public static MapObjectEventArgs Created(MapObject obj)
      => new(MapObjectEventType.Created, obj);
    
    public static MapObjectEventArgs Updated(MapObject obj, MapObject? previousState = null)
        => new(MapObjectEventType.Updated, obj, previousState);
    
    public static MapObjectEventArgs Removed(MapObject obj)
        => new(MapObjectEventType.Removed, obj);
}
