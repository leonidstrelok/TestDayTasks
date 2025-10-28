using Map.Network.Contracts;

namespace Map.Network.Interfaces;

public interface IMapStreamingHubReceiver
{
    void OnObjectAdded(ObjectAddedEvent evt);
    void OnObjectUpdated(ObjectUpdatedEvent evt);
    void OnObjectDeleted(ObjectDeletedEvent evt);
}