using MemoryPack;

namespace Map.Core.Models;

[MemoryPackable]
public partial class Region
{
    public ushort Id { get; set; }
    public string Name { get; set; }
    public int TileCount { get; set; }
        
    public string Description { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}