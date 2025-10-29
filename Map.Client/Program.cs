
using Grpc.Net.Client;
using MagicOnion.Client;
using Map.Shared.Events;
using Map.Shared.Extensions;
using Map.Shared.Interfaces;
using Map.Shared.Models;


var channel = GrpcChannel.ForAddress("http://localhost:5000");

// Streaming Hub для real-time обновлений
var hubClient = await StreamingHubClient.ConnectAsync<IMapStreamingHub, IMapStreamingHubReceiver>(
    channel,
    new MapReceiver(),
    serializerProvider: new MemoryPackMagicOnionSerializerProvider()
);
await hubClient.SubscribeToMapUpdatesAsync();

var mapClient = MagicOnionClient.Create<IMapService>(channel, new MemoryPackMagicOnionSerializerProvider());

// Получение объектов
var request = new GetObjectsInAreaRequest
{
    X1 = 0, Y1 = 0,
    X2 = 100, Y2 = 100
};
var response = await mapClient.GetObjectsInAreaAsync(request);

foreach (var obj in response.Objects)
{
    Console.WriteLine($"Объект {obj.Id} на ({obj.X}, {obj.Y})");
}

var regionsRequest = new GetRegionsInAreaRequest
{
    X1 = 0, Y1 = 0,
    X2 = 500, Y2 = 500
};
var regionsResponse = await mapClient.GetRegionsInAreaAsync(regionsRequest);

Console.ReadLine();

Console.WriteLine($"📦 Найдено объектов: {response.Objects.Count}");

class MapReceiver : IMapStreamingHubReceiver
{
    public void OnObjectAdded(ObjectAddedEvent evt)
    {
        Console.WriteLine($"[Event] Добавлен объект: {evt.Object.Id}");
    }

    public void OnObjectUpdated(ObjectUpdatedEvent evt)
    {
        Console.WriteLine($"[Event] Обновлён объект: {evt.Object.Id}");
    }

    public void OnObjectDeleted(ObjectDeletedEvent evt)
    {
        Console.WriteLine($"[Event] Удалён объект: {evt.ObjectId}");
    }
}