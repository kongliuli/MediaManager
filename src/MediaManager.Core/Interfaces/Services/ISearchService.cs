using MediaManager.Core.DTOs;

namespace MediaManager.Core.Interfaces.Services;

/// <summary>
/// 媒体搜索服务接口。
/// 支持多维度组合过滤和分页。
/// </summary>
public interface ISearchService
{
    Task<MediaSearchResult> SearchAsync(MediaSearchQuery query, CancellationToken ct = default);
}
