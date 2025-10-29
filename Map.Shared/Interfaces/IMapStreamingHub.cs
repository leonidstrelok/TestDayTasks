using MagicOnion;

namespace Map.Shared.Interfaces;

public interface IMapStreamingHub : IStreamingHub<IMapStreamingHub, IMapStreamingHubReceiver>
{
    Task SubscribeToMapUpdatesAsync();
    Task UnsubscribeFromMapUpdatesAsync();
}