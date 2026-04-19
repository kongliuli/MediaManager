using MediaManager.Core.Models;

namespace MediaManager.Core.Interfaces.Repositories;

/// <summary>
/// 媒体库仓储接口
/// </summary>
public interface ILibraryRepository
{
    /// <summary>获取所有媒体库</summary>
    Task<List<Library>> GetAllAsync(CancellationToken ct = default);
    
    /// <summary>根据ID获取媒体库</summary>
    Task<Library?> GetByIdAsync(int id, CancellationToken ct = default);
    
    /// <summary>根据路径获取媒体库</summary>
    Task<Library?> GetByPathAsync(string path, CancellationToken ct = default);
    
    /// <summary>创建媒体库</summary>
    Task<Library> CreateAsync(Library library, CancellationToken ct = default);
    
    /// <summary>更新媒体库</summary>
    Task<Library> UpdateAsync(Library library, CancellationToken ct = default);
    
    /// <summary>删除媒体库</summary>
    Task DeleteAsync(int id, CancellationToken ct = default);
    
    /// <summary>检查路径是否已存在媒体库</summary>
    Task<bool> ExistsByPathAsync(string path, CancellationToken ct = default);
}