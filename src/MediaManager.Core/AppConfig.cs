namespace MediaManager.Core;

/// <summary>应用级配置，通过 DI 注入到需要的服务中</summary>
public class AppConfig
{
    public string ThumbnailDirectory { get; set; } = string.Empty;
    public string FfmpegPath { get; set; } = string.Empty;
    public string FfprobePath { get; set; } = string.Empty;
}
