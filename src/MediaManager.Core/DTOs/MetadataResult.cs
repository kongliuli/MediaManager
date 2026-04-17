using MediaManager.Core.Enums;

namespace MediaManager.Core.DTOs;

/// <summary>
/// FFprobe 元数据提取结果 DTO。
/// 由 FfprobeMetadataService 填充，传递给 ScanPipelineOrchestrator 构建领域模型。
/// </summary>
public class MetadataResult
{
    public MediaType MediaType { get; set; }
    public long FileSize { get; set; }
    public double DurationSeconds { get; set; }

    // 音频流
    public int BitRate { get; set; }
    public int SampleRate { get; set; }
    public int Channels { get; set; }
    public string? AudioCodec { get; set; }

    // 视频流
    public int Width { get; set; }
    public int Height { get; set; }
    public double FrameRate { get; set; }
    public string? VideoCodec { get; set; }
    public int VideoBitRate { get; set; }

    // 容器标签
    public string? Title { get; set; }
    public string? Artist { get; set; }
    public string? Album { get; set; }
    public int? Year { get; set; }
    public string? Genre { get; set; }
}
