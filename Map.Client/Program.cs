// See https://aka.ms/new-console-template for more information

using Grpc.Net.Client;
using MagicOnion.Client;
using Map.Shared.Interfaces;
using Map.Shared.Models;

Console.WriteLine("Hello, World!");

var channel = GrpcChannel.ForAddress("http://localhost:5000");

var mapClient = MagicOnionClient.Create<IMapService>(channel);

var objRequest = new GetObjectsInAreaRequest
{
    X1 = 0,
    Y1 = 0,
    X2 = 200,
    Y2 = 200
};

var objResponse = await mapClient.GetObjectsInAreaAsync(objRequest);

Console.WriteLine($"📦 Найдено объектов: {objResponse.Objects.Count}");