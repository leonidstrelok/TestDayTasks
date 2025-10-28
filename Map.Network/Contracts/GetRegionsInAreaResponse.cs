using MemoryPack;

namespace Map.Network.Contracts;

[MemoryPackable]
public partial class GetRegionsInAreaResponse
{
    public List<RegionDto> Regions { get; set; } = new();
}