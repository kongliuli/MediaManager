using Microsoft.AspNetCore.SignalR;

namespace MediaManager.Api.Hubs;

/// <summary>
/// SignalR Hub 用于实时推送扫描进度
/// </summary>
public class ScanProgressHub : Hub
{
    /// <summary>
    /// 广播扫描进度更新
    /// </summary>
    public async Task BroadcastProgressUpdate(ScanProgressUpdate update)
    {
        await Clients.All.SendAsync("ReceiveProgressUpdate", update);
    }

    /// <summary>
    /// 广播扫描完成
    /// </summary>
    public async Task BroadcastScanCompleted(ScanCompletedResult result)
    {
        await Clients.All.SendAsync("ReceiveScanCompleted", result);
    }
}

/// <summary>
/// 扫描进度更新数据
/// </summary>
public class ScanProgressUpdate
{
    public string TaskId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double ProgressPercent { get; set; }
    public string CurrentFile { get; set; } = string.Empty;
    public int ProcessedCount { get; set; }
    public int TotalCount { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// 扫描完成结果
/// </summary>
public class ScanCompletedResult
{
    public string TaskId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int TotalFiles { get; set; }
    public int NewFiles { get; set; }
    public int UpdatedFiles { get; set; }
    public int SkippedFiles { get; set; }
    public TimeSpan Duration { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string>? Errors { get; set; }
}
