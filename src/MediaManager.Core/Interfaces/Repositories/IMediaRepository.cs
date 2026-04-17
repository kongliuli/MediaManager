using MediaManager.Core.DTOs;
using MediaManager.Core.Models;

namespace MediaManager.Core.Interfaces.Repositories;

/// <summary>媒体文件仓储接口</summary>
public interface IMediaRepository
{
    Task<MediaFile?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<MediaFile?> GetByPathAsync(string path, CancellationToken ct = default);
    Task<MediaSearchResult> SearchAsync(MediaSearchQuery query, CancellationToken ct = default);

    /// <summary>插入或更新（按 Path 判断）</summary>
    Task UpsertAsync(MediaFile file, CancellationToken ct = default);
    Task DeleteAsync(long id, CancellationToken ct = default);
    Task DeleteRangeAsync(IEnumerable<long> ids, CancellationToken ct = default);

    /// <summary>获取所有文件的哈希列表，用于重复检测</summary>
    Task<IReadOnlyList<(string Hash, long Id)>> GetAllHashesAsync(CancellationToken ct = default);
}
