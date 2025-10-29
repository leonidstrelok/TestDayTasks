using MemoryPack;

namespace Map.Shared.Events;

[MemoryPackable]
public partial class ObjectDeletedEvent
{
    public string ObjectId { get; set; }
    public DateTime Timestamp { get; set; }
}