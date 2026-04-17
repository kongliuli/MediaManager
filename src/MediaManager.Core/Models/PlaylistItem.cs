namespace MediaManager.Core.Models;

/// <summary>播放列表条目（有序）</summary>
public class PlaylistItem
{
    public long Id { get; set; }
    public int PlaylistId { get; set; }
    public Playlist Playlist { get; set; } = null!;

    public long MediaFileId { get; set; }
    public MediaFile MediaFile { get; set; } = null!;

    /// <summary>排序序号（从 0 开始）</summary>
    public int SortOrder { get; set; }
}
