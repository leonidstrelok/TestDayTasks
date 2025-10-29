using Map.Core.Interfaces;
using Map.Core.Layers;
using Map.Core.Repositories;
using Map.Core.Services;
using Map.Network.Events;
using Map.Network.Seeds;
using Map.Shared.Extensions;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddMagicOnion(options =>
{
    options.MessageSerializer = new MemoryPackMagicOnionSerializerProvider();
});

// Регистрируем зависимости
builder.Services.AddSingleton<SurfaceLayer>(sp =>
{
    var layer = new SurfaceLayer(1000, 1000);
    // Инициализация карты
    return layer;
});

builder.Services.AddSingleton<RegionLayer>(sp =>
{
    var layer = new RegionLayer(1000, 1000);
    layer.GenerateRegions(50, new Random());
    return layer;
});
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis"))
);
builder.Services.AddSingleton<IMapObjectRepository, RedisMapObjectRepository>();
builder.Services.AddSingleton<DataSeeder>();
builder.Services.AddSingleton<ICoordinateConverter>(sp => new CoordinateConverter(1000, 1000));
builder.Services.AddSingleton<MapObjectLayer>();
builder.Services.AddSingleton<MapEventBroadcaster>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync(mapWidth: 500, mapHeight: 500, regionCount: 8);
}

app.MapMagicOnionService();

app.Run();