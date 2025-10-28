using Map.Core.Interfaces;
using Map.Core.Models;
using MemoryPack;
using StackExchange.Redis;

namespace Map.Core.Repositories;

/// <summary>
/// Реализация репозитория объектов карты на основе Redis
/// Использует GEOADD для пространственного индексирования и хэши для хранения данных
/// </summary>
public class RedisMapObjectRepository : IMapObjectRepository
{
    private readonly IDatabase _database;
    private readonly ICoordinateConverter _converter;

    // Ключи Redis
    private const string GeoKey = "map:objects:geo"; // Геопространственный индекс
    private const string HashKeyPrefix = "map:object:"; // Префикс для хэшей объектов
    private const string AllObjectsKey = "map:objects:all"; // Set всех ID объектов

    public RedisMapObjectRepository(IConnectionMultiplexer redis, ICoordinateConverter converter)
    {
        _database = redis.GetDatabase() ?? throw new ArgumentNullException(nameof(redis));
        _converter = converter ?? throw new ArgumentNullException(nameof(converter));
    }

    /// <inheritdoc />
    public async Task<bool> AddAsync(MapObject obj)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        if (string.IsNullOrEmpty(obj.Id))
            throw new ArgumentException("ID объекта не может быть пустым", nameof(obj));

        try
        {
            // Получаем географические координаты центра объекта
            var (centerX, centerY) = obj.GetCenter();
            var (longitude, latitude) = _converter.ToGeoCoordinates(centerX, centerY);

            // Сериализуем объект
            var serialized = MemoryPackSerializer.Serialize(obj);

            var transaction = _database.CreateTransaction();

            // 1. Добавляем объект в геопространственный индекс
            var geoAddTask = transaction.GeoAddAsync(GeoKey, longitude, latitude, obj.Id);

            // 2. Сохраняем сериализованные данные объекта
            var hashSetTask = transaction.StringSetAsync(GetObjectKey(obj.Id), serialized);

            // 3. Добавляем ID в множество всех объектов
            var setAddTask = transaction.SetAddAsync(AllObjectsKey, obj.Id);

            // Выполняем транзакцию
            var committed = await transaction.ExecuteAsync();

            return committed && await geoAddTask && await hashSetTask && await setAddTask;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Ошибка при добавлении объекта {obj.Id}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<MapObject?> GetByIdAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("ID не может быть пустым", nameof(id));

        try
        {
            var data = await _database.StringGetAsync(GetObjectKey(id));

            if (!data.HasValue)
                return null;

            return MemoryPackSerializer.Deserialize<MapObject>((byte[])data!);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Ошибка при получении объекта {id}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<bool> RemoveAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("ID не может быть пустым", nameof(id));

        try
        {
            var transaction = _database.CreateTransaction();

            // 1. Удаляем из геопространственного индекса
            var geoRemoveTask = transaction.GeoRemoveAsync(GeoKey, id);

            // 2. Удаляем данные объекта
            var hashDeleteTask = transaction.KeyDeleteAsync(GetObjectKey(id));

            // 3. Удаляем ID из множества всех объектов
            var setRemoveTask = transaction.SetRemoveAsync(AllObjectsKey, id);

            var committed = await transaction.ExecuteAsync();

            return committed && await geoRemoveTask && await hashDeleteTask && await setRemoveTask;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Ошибка при удалении объекта {id}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<List<MapObject>> GetByCoordinatesAsync(int x, int y)
    {
        try
        {
            // Преобразуем координаты в географические
            var (longitude, latitude) = _converter.ToGeoCoordinates(x, y);

            // Ищем объекты в небольшом радиусе вокруг точки (примерно 1 тайл)
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

                // Проверяем, действительно ли точка находится внутри объекта
                if (obj != null && obj.ContainsPoint(x, y))
                {
                    objects.Add(obj);
                }
            }

            return objects;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Ошибка при поиске объектов по координатам ({x}, {y})", ex);
        }
    }

    /// <inheritdoc />
    public async Task<List<MapObject>> GetInAreaAsync(int x, int y, int width, int height)
    {
        if (width < 0 || height < 0)
            throw new ArgumentException("Размеры области должны быть неотрицательными");

        try
        {
            // Получаем центр области и радиус поиска
            var centerX = x + width / 2;
            var centerY = y + height / 2;
            var (longitude, latitude) = _converter.ToGeoCoordinates(centerX, centerY);
            var searchRadius = _converter.CalculateSearchRadius(width, height);

            // Ищем все объекты в радиусе
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

                // Проверяем, пересекается ли объект с областью
                if (obj != null && obj.IntersectsArea(x, y, width, height))
                {
                    objects.Add(obj);
                }
            }

            return objects;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Ошибка при поиске объектов в области ({x}, {y}, {width}, {height})", ex);
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("ID не может быть пустым", nameof(id));

        return await _database.KeyExistsAsync(GetObjectKey(id));
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(MapObject obj)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        if (string.IsNullOrEmpty(obj.Id))
            throw new ArgumentException("ID объекта не может быть пустым", nameof(obj));

        try
        {
            // Проверяем существование объекта
            if (!await ExistsAsync(obj.Id))
                return false;

            // Удаляем старую версию и добавляем новую
            // Это необходимо, так как координаты могли измениться
            await RemoveAsync(obj.Id);
            return await AddAsync(obj);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Ошибка при обновлении объекта {obj.Id}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<long> GetCountAsync()
    {
        return await _database.SetLengthAsync(AllObjectsKey);
    }

    /// <inheritdoc />
    public async Task ClearAsync()
    {
        try
        {
            var transaction = _database.CreateTransaction();

            // Получаем все ID объектов
            var allIds = await _database.SetMembersAsync(AllObjectsKey);

            // Удаляем все хэши объектов
            foreach (var id in allIds)
            {
                transaction.KeyDeleteAsync(GetObjectKey(id!), CommandFlags.FireAndForget);
            }

            // Удаляем геопространственный индекс и множество ID
            transaction.KeyDeleteAsync(GeoKey, CommandFlags.FireAndForget);
            transaction.KeyDeleteAsync(AllObjectsKey, CommandFlags.FireAndForget);

            await transaction.ExecuteAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Ошибка при очистке всех объектов", ex);
        }
    }

    /// <summary>
    /// Формирует ключ для хранения данных объекта
    /// </summary>
    private static string GetObjectKey(string id) => $"{HashKeyPrefix}{id}";
}
