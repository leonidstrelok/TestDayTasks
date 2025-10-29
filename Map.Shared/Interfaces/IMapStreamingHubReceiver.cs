using Map.Shared.Events;

namespace Map.Shared.Interfaces;

public interface IMapStreamingHubReceiver
{
    void OnObjectAdded(ObjectAddedEvent evt);
    void OnObjectUpdated(ObjectUpdatedEvent evt);
    void OnObjectDeleted(ObjectDeletedEvent evt);
}