using MediaManager.Core.Enums;

namespace MediaManager.Core.DTOs;

/// <summary>
/// 扫描日志消息 DTO
/// </summary>
public class ScanLogMessage
{
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public LogMessageType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public string? FileExtension { get; set; }
    public MediaType? MediaType { get; set; }
    public ScanStep? Step { get; set; }

    public string FormattedMessage => $"[{Timestamp:HH:mm:ss}] [{Type}] {Message}";
}