namespace MediaManager.Core.Interfaces.Services;

/// <summary>
/// 视频缩略图生成服务接口。
/// 从视频中提取指定时间点的帧，保存为 PNG 图像。
/// </summary>
public interface IThumbnailService
{
    /// <param name="atSecond">提取帧的时间点（秒），null 表示自动选取</param>
    Task GenerateAsync(string videoPath, string outputPath, double? atSecond = null, CancellationToken ct = default);
}
