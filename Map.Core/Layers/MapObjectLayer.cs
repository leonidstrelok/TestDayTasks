using Map.Core.Events;
using Map.Core.Interfaces;
using Map.Core.Models;

namespace Map.Core.Layers;

/// <summary>
/// Слой объектов карты с поддержкой событий и пространственного поиска через Redis
/// </summary>
public class MapObjectLayer
{
    private readonly IMapObjectRepository _repository;

    /// <summary>
    /// Событие создания объекта
    /// </summary>
    public event EventHandler<MapObjectEventArgs>? ObjectCreated;

    /// <summary>
    /// Событие обновления объекта
    /// </summary>
    public event EventHandler<MapObjectEventArgs>? ObjectUpdated;

    /// <summary>
    /// Событие удаления объекта
    /// </summary>
    public event EventHandler<MapObjectEventArgs>? ObjectRemoved;

    /// <summary>
    /// Событие любого изменения объекта
    /// </summary>
    public event EventHandler<MapObjectEventArgs>? ObjectChanged;

    public MapObjectLayer(IMapObjectRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Добавляет объект на карту
    /// </summary>
    /// <param name="obj">Объект для добавления</param>
    /// <returns>True, если объект успешно добавлен</returns>
    /// <exception cref="ArgumentNullException">Если объект null</exception>
    /// <exception cref="InvalidOperationException">Если объект с таким ID уже существует</exception>
    public async Task<bool> AddObjectAsync(MapObject obj)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        if (await _repository.ExistsAsync(obj.Id))
            throw new InvalidOperationException($"Объект с ID {obj.Id} уже существует");

        var result = await _repository.AddAsync(obj);

        if (result)
        {
            OnObjectCreated(MapObjectEventArgs.Created(obj));
        }

        return result;
    }

    /// <summary>
    /// Получает объект по его идентификатору
    /// </summary>
    /// <param name="id">Идентификатор объекта</param>
    /// <returns>Объект или null, если не найден</returns>
    public async Task<MapObject?> GetObjectByIdAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("ID не может быть пустым", nameof(id));

        return await _repository.GetByIdAsync(id);
    }

    /// <summary>
    /// Удаляет объект с карты
    /// </summary>
    /// <param name="id">Идентификатор объекта</param>
    /// <returns>True, если объект успешно удален</returns>
    public async Task<bool> RemoveObjectAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("ID не может быть пустым", nameof(id));

        // Получаем объект перед удалением для события
        var obj = await _repository.GetByIdAsync(id);

        if (obj == null)
            return false;

        var result = await _repository.RemoveAsync(id);

        if (result)
        {
            OnObjectRemoved(MapObjectEventArgs.Removed(obj));
        }

        return result;
    }

    /// <summary>
    /// Получает объект по координатам точки
    /// </summary>
    /// <param name="x">X-координата</param>
    /// <param name="y">Y-координата</param>
    /// <returns>Список объектов, содержащих указанную точку</returns>
    public async Task<List<MapObject>> GetObjectsByCoordinatesAsync(int x, int y)
    {
        return await _repository.GetByCoordinatesAsync(x, y);
    }

    /// <summary>
    /// Получает все объекты в указанной области
    /// </summary>
    /// <param name="x">X-координата левого верхнего угла</param>
    /// <param name="y">Y-координата левого верхнего угла</param>
    /// <param name="width">Ширина области</param>
    /// <param name="height">Высота области</param>
    /// <returns>Список объектов в области</returns>
    public List<MapObject> GetObjectsInAreaAsync(int x, int y, int width, int height)
    {
        // Синхронная обертка для обратной совместимости с MapService
        return _repository.GetInAreaAsync(x, y, width, height).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Получает все объекты в указанной области (асинхронная версия)
    /// </summary>
    public async Task<List<MapObject>> GetObjectsInAreaTaskAsync(int x, int y, int width, int height)
    {
        if (width < 0 || height < 0)
            throw new ArgumentException("Размеры области должны быть неотрицательными");

        return await _repository.GetInAreaAsync(x, y, width, height);
    }

    /// <summary>
    /// Обновляет данные объекта
    /// </summary>
    /// <param name="obj">Объект с обновленными данными</param>
    /// <returns>True, если объект успешно обновлен</returns>
    public async Task<bool> UpdateObjectAsync(MapObject obj)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        // Получаем предыдущее состояние для события
        var previousState = await _repository.GetByIdAsync(obj.Id);

        if (previousState == null)
            throw new InvalidOperationException($"Объект с ID {obj.Id} не найден");

        var result = await _repository.UpdateAsync(obj);

        if (result)
        {
            OnObjectUpdated(MapObjectEventArgs.Updated(obj, previousState));
        }

        return result;
    }

    /// <summary>
    /// Проверяет, существует ли объект с указанным идентификатором
    /// </summary>
    /// <param name="id">Идентификатор объекта</param>
    /// <returns>True, если объект существует</returns>
    public async Task<bool> ObjectExistsAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("ID не может быть пустым", nameof(id));

        return await _repository.ExistsAsync(id);
    }

    /// <summary>
    /// Проверяет, входит ли объект в указанную область
    /// </summary>
    /// <param name="objectId">Идентификатор объекта</param>
    /// <param name="x">X-координата области</param>
    /// <param name="y">Y-координата области</param>
    /// <param name="width">Ширина области</param>
    /// <param name="height">Высота области</param>
    /// <returns>True, если объект входит в область</returns>
    public async Task<bool> IsObjectInAreaAsync(string objectId, int x, int y, int width, int height)
    {
        var obj = await _repository.GetByIdAsync(objectId);

        if (obj == null)
            return false;

        return obj.IntersectsArea(x, y, width, height);
    }

    /// <summary>
    /// Получает общее количество объектов на карте
    /// </summary>
    /// <returns>Количество объектов</returns>
    public async Task<long> GetObjectCountAsync()
    {
        return await _repository.GetCountAsync();
    }

    /// <summary>
    /// Удаляет все объекты с карты
    /// </summary>
    public async Task ClearAllObjectsAsync()
    {
        await _repository.ClearAsync();
    }

    #region Event Handlers

    protected virtual void OnObjectCreated(MapObjectEventArgs e)
    {
        ObjectCreated?.Invoke(this, e);
        ObjectChanged?.Invoke(this, e);
    }

    protected virtual void OnObjectUpdated(MapObjectEventArgs e)
    {
        ObjectUpdated?.Invoke(this, e);
        ObjectChanged?.Invoke(this, e);
    }

    protected virtual void OnObjectRemoved(MapObjectEventArgs e)
    {
        ObjectRemoved?.Invoke(this, e);
        ObjectChanged?.Invoke(this, e);
    }

    #endregion
}
