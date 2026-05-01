namespace MediaManager.Core.Models;

/// <summary>图像文件，继承自 MediaFile</summary>
public class ImageFile : MediaFile
{
    public string Format { get; set; } = string.Empty;
    
    /// <summary>缩略图路径</summary>
    public string? ThumbnailPath { get; set; }

    /// <summary>分辨率字符串，如 "1920×1080"</summary>
    public string Resolution => $"{Width}×{Height}";
}