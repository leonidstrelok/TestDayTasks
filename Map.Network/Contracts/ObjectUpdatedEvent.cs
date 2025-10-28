using MemoryPack;

namespace Map.Network.Contracts;

[MemoryPackable]
public partial class ObjectUpdatedEvent
{
    public ObjectDto Object { get; set; }
    public DateTime Timestamp { get; set; }
}