using Map.Core.Models;

namespace Map.Core.Interfaces;

public interface IMapObjectRepository
{
    Task<bool> AddAsync(MapObject obj);
    
    Task<MapObject?> GetByIdAsync(string id);
    
    Task<bool> RemoveAsync(string id);

    Task<List<MapObject>> GetByCoordinatesAsync(int x, int y);

    Task<List<MapObject>> GetInAreaAsync(int x, int y, int width, int height);

    Task<bool> ExistsAsync(string id);

    Task<bool> UpdateAsync(MapObject obj);

    Task<long> GetCountAsync();

    Task ClearAsync();
}
