using MemoryPack;

namespace Map.Shared.Models;

[MemoryPackable]
public partial class ObjectDto
{
    public string Id { get; set; } = string.Empty;
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Type { get; set; } = string.Empty;
}