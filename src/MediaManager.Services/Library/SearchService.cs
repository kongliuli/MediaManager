using MediaManager.Core.DTOs;
using MediaManager.Core.Interfaces.Repositories;
using MediaManager.Core.Interfaces.Services;

namespace MediaManager.Services.Library;

/// <summary>
/// 媒体搜索服务。
/// 通过 IMediaRepository 接口执行搜索操作，支持多维度组合过滤和分页。
/// </summary>
public class SearchService(IMediaRepository mediaRepository) : ISearchService
{
    public async Task<MediaSearchResult> SearchAsync(MediaSearchQuery query, CancellationToken ct = default)
    {
        return await mediaRepository.SearchAsync(query, ct);
    }
}
