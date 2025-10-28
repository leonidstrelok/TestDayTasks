namespace Map.Core.Models;

public class Region
{
    public ushort Id { get; set; }
    public string Name { get; set; }
    public int TileCount { get; set; }
        
    public string Description { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}