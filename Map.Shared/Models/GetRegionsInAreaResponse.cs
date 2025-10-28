using MemoryPack;

namespace Map.Shared.Models;

[MemoryPackable]
public partial class GetRegionsInAreaResponse
{
    public List<RegionDto> Regions { get; set; } = new();
}