using MemoryPack;

namespace Map.Network.Contracts;

[MemoryPackable]
public partial class ObjectDto
{
    public string Id { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Type { get; set; }
}