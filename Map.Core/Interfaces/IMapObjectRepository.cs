using Map.Core.Models;

namespace Map.Core.Interfaces;

/// <summary>
/// Интерфейс репозитория для работы с объектами карты в Redis
/// </summary>
public interface IMapObjectRepository
{
    /// <summary>
    /// Добавляет объект на карту
    /// </summary>
    /// <param name="obj">Объект для добавления</param>
    /// <returns>True, если объект успешно добавлен</returns>
    Task<bool> AddAsync(MapObject obj);

    /// <summary>
    /// Получает объект по его идентификатору
    /// </summary>
    /// <param name="id">Идентификатор объекта</param>
    /// <returns>Объект или null, если не найден</returns>
    Task<MapObject?> GetByIdAsync(string id);

    /// <summary>
    /// Удаляет объект с карты
    /// </summary>
    /// <param name="id">Идентификатор объекта</param>
    /// <returns>True, если объект успешно удален</returns>
    Task<bool> RemoveAsync(string id);

    /// <summary>
    /// Получает объект по координатам точки
    /// </summary>
    /// <param name="x">X-координата</param>
    /// <param name="y">Y-координата</param>
    /// <returns>Список объектов, содержащих указанную точку</returns>
    Task<List<MapObject>> GetByCoordinatesAsync(int x, int y);

    /// <summary>
    /// Получает все объекты в указанной области
    /// </summary>
    /// <param name="x">X-координата левого верхнего угла</param>
    /// <param name="y">Y-координата левого верхнего угла</param>
    /// <param name="width">Ширина области</param>
    /// <param name="height">Высота области</param>
    /// <returns>Список объектов в области</returns>
    Task<List<MapObject>> GetInAreaAsync(int x, int y, int width, int height);

    /// <summary>
    /// Проверяет, существует ли объект с указанным идентификатором
    /// </summary>
    /// <param name="id">Идентификатор объекта</param>
    /// <returns>True, если объект существует</returns>
    Task<bool> ExistsAsync(string id);

    /// <summary>
    /// Обновляет данные объекта
    /// </summary>
    /// <param name="obj">Объект с обновленными данными</param>
    /// <returns>True, если объект успешно обновлен</returns>
    Task<bool> UpdateAsync(MapObject obj);

    /// <summary>
    /// Получает общее количество объектов на карте
    /// </summary>
    /// <returns>Количество объектов</returns>
    Task<long> GetCountAsync();

    /// <summary>
    /// Удаляет все объекты с карты
    /// </summary>
    Task ClearAsync();
}
