using MediaManager.Core.Enums;

namespace MediaManager.Core.Models;

/// <summary>
/// 媒体文件基类（抽象）。
/// AudioFile 和 VideoFile 通过 TPH 继承此类，共用 MediaFiles 表。
/// </summary>
public abstract class MediaFile
{
    public long Id { get; set; }

    /// <summary>文件完整路径（唯一键）</summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>文件名（含扩展名）</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>文件大小（字节）</summary>
    public long FileSize { get; set; }

    /// <summary>时长（秒）</summary>
    public double DurationSeconds { get; set; }

    /// <summary>SHA256 哈希（用于重复检测）</summary>
    public string Hash { get; set; } = string.Empty;

    /// <summary>导入时间（UTC）</summary>
    public DateTime DateAdded { get; set; }

    /// <summary>文件最后修改时间（UTC）</summary>
    public DateTime LastModified { get; set; }

    /// <summary>TPH 鉴别器列</summary>
    public MediaType MediaType { get; set; }

    /// <summary>用户评分（0-5，null 表示未评分）</summary>
    public int? Rating { get; set; }

    /// <summary>用户备注</summary>
    public string? Notes { get; set; }

    /// <summary>所属媒体库 ID（可为 null，表示未关联媒体库）</summary>
    public int? LibraryId { get; set; }

    /// <summary>媒体子类型（可为 null，表示未识别或未设置）</summary>
    public string? SubType { get; set; }

    // 共享属性：视频和图像都有尺寸信息
    public int? Width { get; set; }
    public int? Height { get; set; }

    // 导航属性
    public ICollection<MediaTag> MediaTags { get; set; } = [];
    public ICollection<PlaylistItem> PlaylistItems { get; set; } = [];

    /// <summary>所属媒体库</summary>
    public virtual Library? Library { get; set; }
}
