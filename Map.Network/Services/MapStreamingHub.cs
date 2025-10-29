using MagicOnion.Server.Hubs;
using Map.Core.Layers;
using Map.Network.Events;
using Map.Shared.Events;
using Map.Shared.Interfaces;

namespace Map.Network.Services;

public class MapStreamingHub : StreamingHubBase<IMapStreamingHub, IMapStreamingHubReceiver>, IMapStreamingHub
{
    private readonly MapObjectLayer _objectLayer;
    private readonly MapEventBroadcaster _broadcaster;
    private IGroup<IMapStreamingHubReceiver>? _group;
    private bool _isSubscribed;

    public MapStreamingHub(MapObjectLayer objectLayer, MapEventBroadcaster broadcaster)
    {
        _objectLayer = objectLayer ?? throw new ArgumentNullException(nameof(objectLayer));
        _broadcaster = broadcaster ?? throw new ArgumentNullException(nameof(broadcaster));
    }

    public async Task SubscribeToMapUpdatesAsync()
    {
        if (_isSubscribed)
            return;

        _group = await Group.AddAsync("map-updates");
        _isSubscribed = true;
        await _broadcaster.SubscribeAsync(this);
    }

    public async Task UnsubscribeFromMapUpdatesAsync()
    {
        if (!_isSubscribed)
            return;

        _isSubscribed = false;
        await _group.RemoveAsync(Context);
        await _broadcaster.UnsubscribeAsync(this);
    }

    protected override async ValueTask OnDisconnected()
    {
        if (_isSubscribed)
        {
            await _group.RemoveAsync(Context);
            await _broadcaster.UnsubscribeAsync(this);
        }
    }

    // Теперь рассылка через Clients
    public void BroadcastObjectAdded(ObjectAddedEvent evt)
    {
        _group.All.OnObjectAdded(evt);
    }

    public void BroadcastObjectUpdated(ObjectUpdatedEvent evt)
    {
        _group.All.OnObjectUpdated(evt);
    }

    public void BroadcastObjectDeleted(ObjectDeletedEvent evt)
    {
        _group.All.OnObjectDeleted(evt);
    }
}