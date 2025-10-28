using MemoryPack;

namespace Map.Shared.Models;

[MemoryPackable]
public partial class GetObjectsInAreaResponse
{
    public List<ObjectDto> Objects { get; set; } = new();
}