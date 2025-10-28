using MagicOnion.Server.Hubs;
using Map.Core.Layers;
using Map.Network.Contracts;
using Map.Network.Events;
using Map.Network.Interfaces;

namespace Map.Network.Services;

public class MapStreamingHub : StreamingHubBase<IMapStreamingHub, IMapStreamingHubReceiver>, IMapStreamingHub
{
    private readonly MapObjectLayer _objectLayer;
    private readonly MapEventBroadcaster _broadcaster;
    private bool _isSubscribed = false;

    public MapStreamingHub(MapObjectLayer objectLayer, MapEventBroadcaster broadcaster)
    {
        _objectLayer = objectLayer ?? throw new ArgumentNullException(nameof(objectLayer));
        _broadcaster = broadcaster ?? throw new ArgumentNullException(nameof(broadcaster));
    }

    public async Task SubscribeToMapUpdatesAsync()
    {
        if (_isSubscribed)
            return;

        _isSubscribed = true;
        await _broadcaster.SubscribeAsync(this);
    }

    public async Task UnsubscribeFromMapUpdatesAsync()
    {
        if (!_isSubscribed)
            return;

        _isSubscribed = false;
        await _broadcaster.UnsubscribeAsync(this);
    }

    protected override async ValueTask OnDisconnected()
    {
        if (_isSubscribed)
        {
            await _broadcaster.UnsubscribeAsync(this);
        }
    }

    // Методы для отправки событий клиенту (вызываются из Broadcaster)
    internal void BroadcastObjectAdded(ObjectAddedEvent evt)
    {
        // TODO: Implement proper MagicOnion broadcast when client interface is defined
        // For now, this method is called by the broadcaster
    }

    internal void BroadcastObjectUpdated(ObjectUpdatedEvent evt)
    {
        // TODO: Implement proper MagicOnion broadcast when client interface is defined
        // For now, this method is called by the broadcaster
    }

    internal void BroadcastObjectDeleted(ObjectDeletedEvent evt)
    {
        // TODO: Implement proper MagicOnion broadcast when client interface is defined
        // For now, this method is called by the broadcaster
    }
}