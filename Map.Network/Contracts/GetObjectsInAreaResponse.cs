using MemoryPack;

namespace Map.Network.Contracts;

[MemoryPackable]
public partial class GetObjectsInAreaResponse
{
    public List<ObjectDto> Objects { get; set; } = new();
}