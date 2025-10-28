using Map.Core.Models;

namespace Map.Core.Interfaces;

/// <summary>
/// ��������� ����������� ��� ������ � ��������� ����� � Redis
/// </summary>
public interface IMapObjectRepository
{
    /// <summary>
    /// ��������� ������ �� �����
    /// </summary>
    /// <param name="obj">������ ��� ����������</param>
    /// <returns>True, ���� ������ ������� ��������</returns>
    Task<bool> AddAsync(MapObject obj);

    /// <summary>
    /// �������� ������ �� ��� ��������������
    /// </summary>
    /// <param name="id">������������� �������</param>
    /// <returns>������ ��� null, ���� �� ������</returns>
    Task<MapObject?> GetByIdAsync(string id);

    /// <summary>
    /// ������� ������ � �����
    /// </summary>
    /// <param name="id">������������� �������</param>
    /// <returns>True, ���� ������ ������� ������</returns>
    Task<bool> RemoveAsync(string id);

    /// <summary>
    /// �������� ������ �� ����������� �����
    /// </summary>
    /// <param name="x">X-����������</param>
    /// <param name="y">Y-����������</param>
    /// <returns>������ ��������, ���������� ��������� �����</returns>
    Task<List<MapObject>> GetByCoordinatesAsync(int x, int y);

    /// <summary>
    /// �������� ��� ������� � ��������� �������
    /// </summary>
    /// <param name="x">X-���������� ������ �������� ����</param>
    /// <param name="y">Y-���������� ������ �������� ����</param>
    /// <param name="width">������ �������</param>
    /// <param name="height">������ �������</param>
    /// <returns>������ �������� � �������</returns>
    Task<List<MapObject>> GetInAreaAsync(int x, int y, int width, int height);

    /// <summary>
    /// ���������, ���������� �� ������ � ��������� ���������������
    /// </summary>
    /// <param name="id">������������� �������</param>
    /// <returns>True, ���� ������ ����������</returns>
    Task<bool> ExistsAsync(string id);

    /// <summary>
    /// ��������� ������ �������
    /// </summary>
    /// <param name="obj">������ � ������������ �������</param>
    /// <returns>True, ���� ������ ������� ��������</returns>
    Task<bool> UpdateAsync(MapObject obj);

    /// <summary>
    /// �������� ����� ���������� �������� �� �����
    /// </summary>
    /// <returns>���������� ��������</returns>
    Task<long> GetCountAsync();

    /// <summary>
    /// ������� ��� ������� � �����
    /// </summary>
    Task ClearAsync();
}
