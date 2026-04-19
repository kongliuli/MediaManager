using MediaManager.Core.Enums;
using System.Collections.ObjectModel;

namespace MediaManager.Core.Models;

/// <summary>
/// 媒体库模型 - 存储扫描路径和扫描状态
/// </summary>
public class Library
{
    public int Id { get; set; }
    
    /// <summary>媒体库名称</summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>扫描目录路径</summary>
    public string ScanPath { get; set; } = string.Empty;
    
    /// <summary>是否递归扫描</summary>
    public bool Recursive { get; set; } = true;
    
    /// <summary>是否包含音频</summary>
    public bool IncludeAudio { get; set; } = true;
    
    /// <summary>是否包含视频</summary>
    public bool IncludeVideo { get; set; } = true;
    
    /// <summary>是否包含图像</summary>
    public bool IncludeImages { get; set; } = false;
    
    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>最后扫描时间</summary>
    public DateTime? LastScannedAt { get; set; }
    
    /// <summary>扫描状态</summary>
    public LibraryStatus Status { get; set; } = LibraryStatus.Idle;
    
    /// <summary>媒体文件数量</summary>
    public int FileCount { get; set; }
    
    /// <summary>媒体文件大小（字节）</summary>
    public long TotalSize { get; set; }

    /// <summary>关联的媒体文件</summary>
    public virtual ICollection<MediaFile> MediaFiles { get; set; } = new ObservableCollection<MediaFile>();
}