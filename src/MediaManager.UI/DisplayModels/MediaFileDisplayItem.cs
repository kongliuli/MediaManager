using CommunityToolkit.Mvvm.ComponentModel;
using MediaManager.Core.Enums;
using MediaManager.Core.Models;
using System.Windows.Media.Imaging;

namespace MediaManager.UI.DisplayModels;

/// <summary>
/// MediaFile 的 WPF 显示包装器。
/// 将领域模型与 WPF 专属属性（IsSelected、BitmapSource）分离，
/// 避免污染 Core 层模型。
/// </summary>
public partial class MediaFileDisplayItem(MediaFile source) : ObservableObject
{
    /// <summary>原始领域模型引用</summary>
    public MediaFile Source { get; } = source;

    // 转发常用属性，避免 View 直接绑定到 Source
    public string FileName => Source.FileName;
    public string Path => Source.Path;
    public long FileSize => Source.FileSize;
    public double DurationSeconds => Source.DurationSeconds;
    public DateTime DateAdded => Source.DateAdded;
    public string Hash => Source.Hash;
    public DateTime LastModified => Source.LastModified;
    public int? Width => Source.Width;
    public int? Height => Source.Height;
    public MediaType MediaType => Source.MediaType;

    /// <summary>是否在列表中被选中（UI 状态，不属于领域模型）</summary>
    [ObservableProperty] private bool _isSelected;

    /// <summary>
    /// 缩略图/封面图的 WPF 图像源（懒加载）。
    /// 视频取 ThumbnailPath，音频取 AlbumArtPath，图片取 ThumbnailPath。
    /// </summary>
    [ObservableProperty] private BitmapSource? _thumbnail;

    /// <summary>格式化时长字符串，如 "1:23:45"</summary>
    public string DurationFormatted
    {
        get
        {
            var ts = TimeSpan.FromSeconds(DurationSeconds);
            return ts.TotalHours >= 1
                ? $"{(int)ts.TotalHours}:{ts.Minutes:D2}:{ts.Seconds:D2}"
                : $"{ts.Minutes}:{ts.Seconds:D2}";
        }
    }

    /// <summary>格式化文件大小，如 "1.2 GB"</summary>
    public string FileSizeFormatted => Source.FileSize switch
    {
        >= 1_073_741_824 => $"{Source.FileSize / 1_073_741_824.0:F1} GB",
        >= 1_048_576     => $"{Source.FileSize / 1_048_576.0:F1} MB",
        >= 1_024         => $"{Source.FileSize / 1_024.0:F1} KB",
        _                => $"{Source.FileSize} B"
    };

    /// <summary>分辨率字符串，如 "1920×1080"</summary>
    public string Resolution => Width.HasValue && Height.HasValue 
        ? $"{Width}×{Height}" 
        : "未知";

    /// <summary>哈希值短格式（前8位）</summary>
    public string HashShort => Hash.Length >= 8 ? Hash.Substring(0, 8) + "..." : Hash;

    /// <summary>完整哈希值</summary>
    public string HashFull => Hash;
}
