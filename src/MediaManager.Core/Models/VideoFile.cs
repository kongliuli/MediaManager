namespace MediaManager.Core.Models;

/// <summary>视频文件，继承自 MediaFile</summary>
public class VideoFile : MediaFile
{
    public int Width { get; set; }
    public int Height { get; set; }
    public double FrameRate { get; set; }
    public string VideoCodec { get; set; } = string.Empty;
    public string AudioCodec { get; set; } = string.Empty;
    public int BitRate { get; set; }

    /// <summary>视频标题（来自容器元数据）</summary>
    public string? Title { get; set; }

    /// <summary>缩略图缓存路径</summary>
    public string? ThumbnailPath { get; set; }

    /// <summary>分辨率字符串，如 "1920×1080"</summary>
    public string Resolution => $"{Width}×{Height}";
}
