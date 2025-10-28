using Map.Shared.Models;
using MemoryPack;

namespace Map.Network.Contracts;

[MemoryPackable]
public partial class ObjectAddedEvent
{
    public ObjectDto Object { get; set; }
    public DateTime Timestamp { get; set; }
}