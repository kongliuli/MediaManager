namespace MediaManager.Core.Models;

/// <summary>媒体文件与标签的多对多关联表</summary>
public class MediaTag
{
    public long MediaFileId { get; set; }
    public MediaFile MediaFile { get; set; } = null!;

    public int TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
