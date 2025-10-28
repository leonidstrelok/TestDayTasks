using MemoryPack;

namespace Map.Network.Contracts;

[MemoryPackable]
public partial class RegionDto
{
    public ushort Id { get; set; }
    public string Name { get; set; }
    public int TileCount { get; set; }
}