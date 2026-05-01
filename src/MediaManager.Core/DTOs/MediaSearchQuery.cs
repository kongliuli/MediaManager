using MediaManager.Core.Enums;

namespace MediaManager.Core.DTOs;

/// <summary>
/// 媒体搜索查询参数 DTO。
/// 所有条件均为可选，未设置的条件不参与过滤。
/// </summary>
public class MediaSearchQuery
{
    public int? LibraryId { get; set; }
    public string? Keyword { get; set; }
    public MediaType? MediaType { get; set; }
    public List<int> TagIds { get; set; } = [];
    public double? MinDurationSeconds { get; set; }
    public double? MaxDurationSeconds { get; set; }
    public DateTime? DateAddedFrom { get; set; }
    public DateTime? DateAddedTo { get; set; }
    public SortField SortBy { get; set; } = SortField.DateAdded;
    public bool SortDescending { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
