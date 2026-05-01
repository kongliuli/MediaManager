using MediaManager.Core.Enums;

namespace MediaManager.Core.Models;

/// <summary>
/// 扩展的音频文件，添加完整元数据支持
/// </summary>
public class ExtendedAudioFile : AudioFile
{
    /// <summary>
    /// 音频特有的扩展元数据
    /// </summary>
    public AudioMetadata? AudioMetadata { get; set; }
}

/// <summary>
/// 扩展的视频文件，添加完整元数据支持
/// </summary>
public class ExtendedVideoFile : VideoFile
{
    /// <summary>
    /// 视频特有的扩展元数据
    /// </summary>
    public VideoMetadata? VideoMetadata { get; set; }
}

/// <summary>
/// 扩展的图像文件，添加完整元数据支持
/// </summary>
public class ExtendedImageFile : ImageFile
{
    /// <summary>
    /// 图像特有的扩展元数据
    /// </summary>
    public ImageMetadata? ImageMetadata { get; set; }
}

/// <summary>
/// 扩展的文档文件，添加完整元数据支持
/// </summary>
public class ExtendedDocumentFile : DocumentFile
{
    /// <summary>
    /// 文档特有的扩展元数据
    /// </summary>
    public DocumentMetadata? DocumentMetadata { get; set; }
}
