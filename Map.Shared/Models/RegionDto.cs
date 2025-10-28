using MemoryPack;

namespace Map.Shared.Models;

[MemoryPackable]
public partial class RegionDto
{
    public ushort Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TileCount { get; set; }
}