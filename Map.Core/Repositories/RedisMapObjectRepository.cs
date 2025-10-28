using Map.Core.Interfaces;
using Map.Core.Models;
using MemoryPack;
using StackExchange.Redis;

namespace Map.Core.Repositories;

public class RedisMapObjectRepository : IMapObjectRepository
{
    private readonly IDatabase _database;
    private readonly ICoordinateConverter _converter;

    private const string GeoKey = "map:objects:geo";
    private const string HashKeyPrefix = "map:object:";
    private const string AllObjectsKey = "map:objects:all";

    public RedisMapObjectRepository(IConnectionMultiplexer redis, ICoordinateConverter converter)
    {
        _database = redis.GetDatabase() ?? throw new ArgumentNullException(nameof(redis));
        _converter = converter ?? throw new ArgumentNullException(nameof(converter));
    }

    public async Task<bool> AddAsync(MapObject obj)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        if (string.IsNullOrEmpty(obj.Id))
            throw new ArgumentException();

        try
        {
            var (centerX, centerY) = obj.GetCenter();
            var (longitude, latitude) = _converter.ToGeoCoordinates(centerX, centerY);

            var serialized = MemoryPackSerializer.Serialize(obj);

            var transaction = _database.CreateTransaction();

            var geoAddTask = transaction.GeoAddAsync(GeoKey, longitude, latitude, obj.Id);

            var hashSetTask = transaction.StringSetAsync(GetObjectKey(obj.Id), serialized);

            var setAddTask = transaction.SetAddAsync(AllObjectsKey, obj.Id);

            var committed = await transaction.ExecuteAsync();

            return committed && await geoAddTask && await hashSetTask && await setAddTask;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"{obj.Id}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<MapObject?> GetByIdAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("ID {0}", nameof(id));

        try
        {
            var data = await _database.StringGetAsync(GetObjectKey(id));

            if (!data.HasValue)
                return null;

            return MemoryPackSerializer.Deserialize<MapObject>((byte[])data!);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"{id}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<bool> RemoveAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("ID {0}", nameof(id));

        try
        {
            var transaction = _database.CreateTransaction();

            var geoRemoveTask = transaction.GeoRemoveAsync(GeoKey, id);

            var hashDeleteTask = transaction.KeyDeleteAsync(GetObjectKey(id));

            var setRemoveTask = transaction.SetRemoveAsync(AllObjectsKey, id);

            var committed = await transaction.ExecuteAsync();

            return committed && await geoRemoveTask && await hashDeleteTask && await setRemoveTask;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($" {id}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<List<MapObject>> GetByCoordinatesAsync(int x, int y)
    {
        try
        {
            var (longitude, latitude) = _converter.ToGeoCoordinates(x, y);

            var searchRadius = _converter.CalculateSearchRadius(2, 2);

            var results = await _database.GeoRadiusAsync(
                GeoKey,
                longitude,
                latitude,
                searchRadius,
                GeoUnit.Meters);

            var objects = new List<MapObject>();

            foreach (var result in results)
            {
                var obj = await GetByIdAsync(result.Member!);

                if (obj != null && obj.ContainsPoint(x, y))
                {
                    objects.Add(obj);
                }
            }

            return objects;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"({x}, {y})", ex);
        }
    }

    /// <inheritdoc />
    public async Task<List<MapObject>> GetInAreaAsync(int x, int y, int width, int height)
    {
        if (width < 0 || height < 0)
            throw new ArgumentException("The width or height cannot be less than zero");

        try
        {
            var centerX = x + width / 2;
            var centerY = y + height / 2;
            var (longitude, latitude) = _converter.ToGeoCoordinates(centerX, centerY);
            var searchRadius = _converter.CalculateSearchRadius(width, height);

            var results = await _database.GeoRadiusAsync(
                GeoKey,
                longitude,
                latitude,
                searchRadius,
                GeoUnit.Meters);

            var objects = new List<MapObject>();

            foreach (var result in results)
            {
                var obj = await GetByIdAsync(result.Member!);

                if (obj != null && obj.IntersectsArea(x, y, width, height))
                {
                    objects.Add(obj);
                }
            }

            return objects;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"({x}, {y}, {width}, {height})",
                ex);
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("The ID {0} cannot be empty", nameof(id));

        return await _database.KeyExistsAsync(GetObjectKey(id));
    }

    public async Task<bool> UpdateAsync(MapObject obj)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        if (string.IsNullOrEmpty(obj.Id))
            throw new ArgumentException("The ID {0} cannot be empty", nameof(obj));

        try
        {
            if (!await ExistsAsync(obj.Id))
                return false;

            await RemoveAsync(obj.Id);
            return await AddAsync(obj);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($" {obj.Id}", ex);
        }
    }

    public async Task<long> GetCountAsync()
    {
        return await _database.SetLengthAsync(AllObjectsKey);
    }

    public async Task ClearAsync()
    {
        try
        {
            var transaction = _database.CreateTransaction();

            var allIds = await _database.SetMembersAsync(AllObjectsKey);

            foreach (var id in allIds)
            {
                transaction.KeyDeleteAsync(GetObjectKey(id!), CommandFlags.FireAndForget);
            }

            transaction.KeyDeleteAsync(GeoKey, CommandFlags.FireAndForget);
            transaction.KeyDeleteAsync(AllObjectsKey, CommandFlags.FireAndForget);

            await transaction.ExecuteAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException();
        }
    }

    private static string GetObjectKey(string id) => $"{HashKeyPrefix}{id}";
}