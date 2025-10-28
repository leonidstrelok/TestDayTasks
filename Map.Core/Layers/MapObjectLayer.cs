using Map.Core.Events;
using Map.Core.Interfaces;
using Map.Core.Models;

namespace Map.Core.Layers;

public class MapObjectLayer
{
    private readonly IMapObjectRepository _repository;
    
    public event EventHandler<MapObjectEventArgs>? ObjectCreated;
    
    public event EventHandler<MapObjectEventArgs>? ObjectUpdated;

    public event EventHandler<MapObjectEventArgs>? ObjectRemoved;

    public event EventHandler<MapObjectEventArgs>? ObjectChanged;

    public MapObjectLayer(IMapObjectRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }


    public async Task<bool> AddObjectAsync(MapObject obj)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        if (await _repository.ExistsAsync(obj.Id))
            throw new InvalidOperationException($"������ � ID {obj.Id} ��� ����������");

        var result = await _repository.AddAsync(obj);

        if (result)
        {
            OnObjectCreated(MapObjectEventArgs.Created(obj));
        }

        return result;
    }


    public async Task<MapObject?> GetObjectByIdAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("ID �� ����� ���� ������", nameof(id));

        return await _repository.GetByIdAsync(id);
    }


    public async Task<bool> RemoveObjectAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("ID With name: {0} not found", nameof(id));

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

    public async Task<List<MapObject>> GetObjectsByCoordinatesAsync(int x, int y)
    {
        return await _repository.GetByCoordinatesAsync(x, y);
    }

    public List<MapObject> GetObjectsInAreaAsync(int x, int y, int width, int height)
    {
        return _repository.GetInAreaAsync(x, y, width, height).GetAwaiter().GetResult();
    }

    public async Task<List<MapObject>> GetObjectsInAreaTaskAsync(int x, int y, int width, int height)
    {
        if (width < 0 || height < 0)
            throw new ArgumentException("The width or height cannot be less than zero");

        return await _repository.GetInAreaAsync(x, y, width, height);
    }

    public async Task<bool> UpdateObjectAsync(MapObject obj)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        var previousState = await _repository.GetByIdAsync(obj.Id);

        if (previousState == null)
            throw new InvalidOperationException();

        var result = await _repository.UpdateAsync(obj);

        if (result)
        {
            OnObjectUpdated(MapObjectEventArgs.Updated(obj, previousState));
        }

        return result;
    }

    public async Task<bool> ObjectExistsAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("The ID {0} cannot be empty", nameof(id));

        return await _repository.ExistsAsync(id);
    }

    public async Task<bool> IsObjectInAreaAsync(string objectId, int x, int y, int width, int height)
    {
        var obj = await _repository.GetByIdAsync(objectId);

        if (obj == null)
            return false;

        return obj.IntersectsArea(x, y, width, height);
    }

    public async Task<long> GetObjectCountAsync()
    {
        return await _repository.GetCountAsync();
    }

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
