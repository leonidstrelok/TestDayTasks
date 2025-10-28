using Map.Core.Events;
using Map.Core.Layers;
using Map.Core.Models;
using Map.Network.Contracts;
using Map.Network.Services;

namespace Map.Network.Events;

public class MapEventBroadcaster
{
    private readonly List<MapStreamingHub> _subscribers = new();
    private readonly object _subscribersLock = new();
    private readonly MapObjectLayer _objectLayer;

    public MapEventBroadcaster(MapObjectLayer objectLayer)
    {
        _objectLayer = objectLayer ?? throw new ArgumentNullException(nameof(objectLayer));

        // Подписываемся на события от ObjectLayer
        _objectLayer.ObjectChanged += (sender, e) => OnObjectEvent(e);
    }

    public Task SubscribeAsync(MapStreamingHub hub)
    {
        lock (_subscribersLock)
        {
            if (!_subscribers.Contains(hub))
            {
                _subscribers.Add(hub);
            }
        }

        return Task.CompletedTask;
    }

    public Task UnsubscribeAsync(MapStreamingHub hub)
    {
        lock (_subscribersLock)
        {
            _subscribers.Remove(hub);
        }

        return Task.CompletedTask;
    }

    private void OnObjectEvent(MapObjectEventArgs evt)
    {
        List<MapStreamingHub> subscribers;

        lock (_subscribersLock)
        {
            subscribers = new List<MapStreamingHub>(_subscribers);
        }

        // Конвертируем событие и рассылаем
        switch (evt.EventType)
        {
            case MapObjectEventType.Created:
                var addedEvent = new ObjectAddedEvent
                {
                    Object = MapObjectToDto(evt.Object),
                    Timestamp = evt.Timestamp
                };
                foreach (var sub in subscribers)
                {
                    try
                    {
                        sub.BroadcastObjectAdded(addedEvent);
                    }
                    catch
                    {
                        // Логирование ошибок
                    }
                }
                break;

            case MapObjectEventType.Updated:
                var updatedEvent = new ObjectUpdatedEvent
                {
                    Object = MapObjectToDto(evt.Object),
                    Timestamp = evt.Timestamp
                };
                foreach (var sub in subscribers)
                {
                    try
                    {
                        sub.BroadcastObjectUpdated(updatedEvent);
                    }
                    catch
                    {
                        // Логирование ошибок
                    }
                }
                break;

            case MapObjectEventType.Removed:
                var deletedEvent = new ObjectDeletedEvent
                {
                    ObjectId = evt.Object.Id,
                    Timestamp = evt.Timestamp
                };
                foreach (var sub in subscribers)
                {
                    try
                    {
                        sub.BroadcastObjectDeleted(deletedEvent);
                    }
                    catch
                    {
                        // Логирование ошибок
                    }
                }
                break;
        }
    }

    private ObjectDto MapObjectToDto(MapObject obj)
    {
        return new ObjectDto
        {
            Id = obj.Id,
            X = obj.X,
            Y = obj.Y,
            Width = obj.Width,
            Height = obj.Height,
            Type = obj.Type
        };
    }
}