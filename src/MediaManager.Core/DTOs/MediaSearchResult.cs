using MediaManager.Core.Models;

namespace MediaManager.Core.DTOs;

/// <summary>
/// 媒体搜索结果 DTO。
/// 包含分页信息和当前页的媒体文件列表。
/// </summary>
public class MediaSearchResult
{
    public IReadOnlyList<MediaFile> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
