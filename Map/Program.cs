var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMagicOnion();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();