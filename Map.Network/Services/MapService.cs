using MagicOnion;
using MagicOnion.Server;
using Map.Core.Layers;
using Map.Core.Models;
using Map.Shared.Interfaces;
using Map.Shared.Models;

namespace Map.Network.Services;

public class MapService : ServiceBase<IMapService>, IMapService
{
    private readonly MapObjectLayer _objectLayer;
    private readonly RegionLayer _regionLayer;
    private readonly object _lock = new();

    public MapService(MapObjectLayer objectLayer, RegionLayer regionLayer)
    {
        _objectLayer = objectLayer ?? throw new ArgumentNullException(nameof(objectLayer));
        _regionLayer = regionLayer ?? throw new ArgumentNullException(nameof(regionLayer));
    }

    public async UnaryResult<GetObjectsInAreaResponse> GetObjectsInAreaAsync(GetObjectsInAreaRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // Вычисляем размеры области
        var x = Math.Min(request.X1, request.X2);
        var y = Math.Min(request.Y1, request.Y2);
        var width = Math.Abs(request.X2 - request.X1);
        var height = Math.Abs(request.Y2 - request.Y1);

        // Получаем объекты (потокобезопасно)
        List<MapObject> objects;
        lock (_lock)
        {
            objects = _objectLayer.GetObjectsInAreaAsync(x, y, width, height);
        }

        // Конвертируем в DTO
        var response = new GetObjectsInAreaResponse();
        foreach (var obj in objects)
        {
            response.Objects.Add(new ObjectDto
            {
                Id = obj.Id,
                X = obj.X,
                Y = obj.Y,
                Width = obj.Width,
                Height = obj.Height,
                Type = obj.Type
            });
        }
        
        return response;
    }

    public UnaryResult<GetRegionsInAreaResponse> GetRegionsInAreaAsync(GetRegionsInAreaRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // Вычисляем размеры области
        var x = Math.Min(request.X1, request.X2);
        var y = Math.Min(request.Y1, request.Y2);
        var width = Math.Abs(request.X2 - request.X1);
        var height = Math.Abs(request.Y2 - request.Y1);

        // Получаем регионы (потокобезопасно - только чтение)
        List<Region> regions;
        lock (_lock)
        {
            regions = _regionLayer.GetRegionMetadataInArea(x, y, width, height);
        }

        // Конвертируем в DTO
        var response = new GetRegionsInAreaResponse();
        foreach (var region in regions)
        {
            response.Regions.Add(new RegionDto
            {
                Id = region.Id,
                Name = region.Name,
                TileCount = region.TileCount
            });
        }

        return UnaryResult.FromResult(response);
    }
}