using MemoryPack;

namespace Map.Network.Contracts;

[MemoryPackable]
public partial class ObjectDeletedEvent
{
    public string ObjectId { get; set; }
    public DateTime Timestamp { get; set; }
}