using MagicOnion;

namespace Map.Network.Interfaces;

public interface IMapStreamingHub : IStreamingHub<IMapStreamingHub, IMapStreamingHubReceiver>
{
    Task SubscribeToMapUpdatesAsync();
    Task UnsubscribeFromMapUpdatesAsync();
}