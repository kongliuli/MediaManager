using System.Collections.Concurrent;
using MediaManager.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace MediaManager.Api;

/// <summary>
/// 扫描任务状态
/// </summary>
public enum ScanTaskStatus
{
    Pending,
    Running,
    Paused,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
/// 扫描任务信息
/// </summary>
public class ScanTask
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string LibraryPath { get; set; } = string.Empty;
    public bool Recursive { get; set; } = true;
    public ScanTaskStatus Status { get; set; } = ScanTaskStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int ProcessedCount { get; set; }
    public int TotalCount { get; set; }
    public string CurrentFile { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    public CancellationTokenSource? CancellationTokenSource { get; set; }
}

/// <summary>
/// 扫描任务管理器，负责管理和协调扫描任务
/// </summary>
public class ScanTaskManager
{
    private readonly ConcurrentDictionary<string, ScanTask> _tasks = new();
    private readonly IHubContext<ScanProgressHub> _hubContext;
    private readonly ILogger<ScanTaskManager> _logger;

    public ScanTaskManager(
        IHubContext<ScanProgressHub> hubContext,
        ILogger<ScanTaskManager> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// 创建新的扫描任务
    /// </summary>
    public ScanTask CreateTask(string libraryPath, bool recursive = true)
    {
        var task = new ScanTask
        {
            LibraryPath = libraryPath,
            Recursive = recursive,
            Status = ScanTaskStatus.Pending,
            CancellationTokenSource = new CancellationTokenSource()
        };

        _tasks[task.Id] = task;
        _logger.LogInformation("Created new scan task {TaskId} for path {Path}", task.Id, libraryPath);

        return task;
    }

    /// <summary>
    /// 获取任务
    /// </summary>
    public ScanTask? GetTask(string taskId)
    {
        return _tasks.TryGetValue(taskId, out var task) ? task : null;
    }

    /// <summary>
    /// 获取所有任务
    /// </summary>
    public List<ScanTask> GetAllTasks()
    {
        return _tasks.Values.OrderByDescending(t => t.CreatedAt).ToList();
    }

    /// <summary>
    /// 开始任务
    /// </summary>
    public void StartTask(string taskId)
    {
        if (_tasks.TryGetValue(taskId, out var task))
        {
            task.Status = ScanTaskStatus.Running;
            task.StartedAt = DateTime.UtcNow;
            _logger.LogInformation("Started scan task {TaskId}", taskId);
        }
    }

    /// <summary>
    /// 更新任务进度
    /// </summary>
    public async Task UpdateProgressAsync(string taskId, int processed, int total, string currentFile, string message = "")
    {
        if (_tasks.TryGetValue(taskId, out var task))
        {
            task.ProcessedCount = processed;
            task.TotalCount = total;
            task.CurrentFile = currentFile;

            var update = new ScanProgressUpdate
            {
                TaskId = taskId,
                Status = task.Status.ToString(),
                ProgressPercent = total > 0 ? (double)processed / total * 100 : 0,
                CurrentFile = currentFile,
                ProcessedCount = processed,
                TotalCount = total,
                Message = message
            };

            await _hubContext.Clients.All.SendAsync("ReceiveProgressUpdate", update);
        }
    }

    /// <summary>
    /// 完成任务
    /// </summary>
    public async Task CompleteTaskAsync(string taskId, int newFiles, int updatedFiles, int skippedFiles, List<string>? errors = null)
    {
        if (_tasks.TryGetValue(taskId, out var task))
        {
            task.Status = ScanTaskStatus.Completed;
            task.CompletedAt = DateTime.UtcNow;
            if (errors != null)
            {
                task.Errors.AddRange(errors);
            }

            var result = new ScanCompletedResult
            {
                TaskId = taskId,
                Success = errors == null || errors.Count == 0,
                TotalFiles = task.ProcessedCount,
                NewFiles = newFiles,
                UpdatedFiles = updatedFiles,
                SkippedFiles = skippedFiles,
                Duration = task.StartedAt.HasValue ? DateTime.UtcNow - task.StartedAt.Value : TimeSpan.Zero,
                Message = errors == null || errors.Count == 0 ? "扫描完成" : $"扫描完成，但有 {errors.Count} 个错误",
                Errors = errors
            };

            await _hubContext.Clients.All.SendAsync("ReceiveScanCompleted", result);
            _logger.LogInformation("Completed scan task {TaskId}", taskId);
        }
    }

    /// <summary>
    /// 任务失败
    /// </summary>
    public async Task FailTaskAsync(string taskId, string error)
    {
        if (_tasks.TryGetValue(taskId, out var task))
        {
            task.Status = ScanTaskStatus.Failed;
            task.CompletedAt = DateTime.UtcNow;
            task.Errors.Add(error);

            var result = new ScanCompletedResult
            {
                TaskId = taskId,
                Success = false,
                TotalFiles = task.ProcessedCount,
                NewFiles = 0,
                UpdatedFiles = 0,
                SkippedFiles = 0,
                Duration = task.StartedAt.HasValue ? DateTime.UtcNow - task.StartedAt.Value : TimeSpan.Zero,
                Message = error,
                Errors = new List<string> { error }
            };

            await _hubContext.Clients.All.SendAsync("ReceiveScanCompleted", result);
            _logger.LogError("Scan task {TaskId} failed: {Error}", taskId, error);
        }
    }

    /// <summary>
    /// 取消任务
    /// </summary>
    public void CancelTask(string taskId)
    {
        if (_tasks.TryGetValue(taskId, out var task))
        {
            task.CancellationTokenSource?.Cancel();
            task.Status = ScanTaskStatus.Cancelled;
            task.CompletedAt = DateTime.UtcNow;
            _logger.LogInformation("Cancelled scan task {TaskId}", taskId);
        }
    }
}
