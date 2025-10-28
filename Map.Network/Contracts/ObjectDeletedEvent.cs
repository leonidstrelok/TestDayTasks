using MessagePack;

namespace Map.Network.Contracts;

[MessagePackObject]
public partial class ObjectDeletedEvent
{
    [Key(0)]
    public string ObjectId { get; set; }
    [Key(1)]
    public DateTime Timestamp { get; set; }
}