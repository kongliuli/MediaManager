using MediaManager.Core.Enums;

namespace MediaManager.Core.DTOs;

/// <summary>
/// 扫描进度报告 DTO。
/// 通过 IProgress&lt;ScanProgressReport&gt; 从后台线程推送到 UI。
/// </summary>
public class ScanProgressReport
{
    public ScanStatus Status { get; set; } = ScanStatus.Idle;
    public int TotalDiscovered { get; set; }
    public int Processed { get; set; }
    public int Errors { get; set; }
    public string? CurrentFile { get; set; }

    /// <summary>进度百分比（0-100）</summary>
    public double ProgressPercent => TotalDiscovered == 0 ? 0
        : Math.Min(100.0 * Processed / TotalDiscovered, 100);
}
