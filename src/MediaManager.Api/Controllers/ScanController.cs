using Microsoft.AspNetCore.Mvc;
using MediaManager.Api.BackgroundServices;

namespace MediaManager.Api.Controllers;

/// <summary>
/// 扫描任务 API 控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ScanController : ControllerBase
{
    private readonly ScanTaskManager _taskManager;
    private readonly ScanBackgroundService _backgroundService;
    private readonly ILogger<ScanController> _logger;

    public ScanController(
        ScanTaskManager taskManager,
        ScanBackgroundService backgroundService,
        ILogger<ScanController> logger)
    {
        _taskManager = taskManager;
        _backgroundService = backgroundService;
        _logger = logger;
    }

    /// <summary>
    /// 创建新的扫描任务
    /// </summary>
    [HttpPost("start")]
    public IActionResult StartScan([FromBody] StartScanRequest request)
    {
        if (!Directory.Exists(request.LibraryPath))
        {
            return NotFound($"目录不存在: {request.LibraryPath}");
        }

        var task = _taskManager.CreateTask(request.LibraryPath, request.Recursive);
        _backgroundService.EnqueueTask(task.Id);

        return Ok(new { TaskId = task.Id, Message = "扫描任务已创建并加入队列" });
    }

    /// <summary>
    /// 获取所有扫描任务
    /// </summary>
    [HttpGet("tasks")]
    public IActionResult GetTasks()
    {
        var tasks = _taskManager.GetAllTasks();
        return Ok(tasks);
    }

    /// <summary>
    /// 获取指定任务详情
    /// </summary>
    [HttpGet("tasks/{taskId}")]
    public IActionResult GetTask(string taskId)
    {
        var task = _taskManager.GetTask(taskId);
        if (task == null)
        {
            return NotFound($"任务不存在: {taskId}");
        }
        return Ok(task);
    }

    /// <summary>
    /// 取消扫描任务
    /// </summary>
    [HttpPost("tasks/{taskId}/cancel")]
    public IActionResult CancelTask(string taskId)
    {
        var task = _taskManager.GetTask(taskId);
        if (task == null)
        {
            return NotFound($"任务不存在: {taskId}");
        }

        _taskManager.CancelTask(taskId);
        return Ok(new { Message = "取消请求已发送" });
    }
}

/// <summary>
/// 开始扫描请求
/// </summary>
public class StartScanRequest
{
    public string LibraryPath { get; set; } = string.Empty;
    public bool Recursive { get; set; } = true;
}
