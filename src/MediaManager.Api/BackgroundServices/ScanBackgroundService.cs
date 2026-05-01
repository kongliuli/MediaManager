using System.Collections.Concurrent;
using MediaManager.Api.Hubs;
using MediaManager.Core.Interfaces.Repositories;
using MediaManager.Core.Interfaces.Services;
using MediaManager.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace MediaManager.Api.BackgroundServices;

/// <summary>
/// 后台服务，负责异步执行扫描任务
/// </summary>
public class ScanBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ScanTaskManager _taskManager;
    private readonly ILogger<ScanBackgroundService> _logger;
    private readonly ConcurrentQueue<string> _taskQueue = new();

    public ScanBackgroundService(
        IServiceProvider serviceProvider,
        ScanTaskManager taskManager,
        ILogger<ScanBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _taskManager = taskManager;
        _logger = logger;
    }

    /// <summary>
    /// 将任务加入队列
    /// </summary>
    public void EnqueueTask(string taskId)
    {
        _taskQueue.Enqueue(taskId);
        _logger.LogInformation("Enqueued scan task {TaskId}", taskId);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Scan Background Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_taskQueue.TryDequeue(out var taskId))
            {
                await ProcessTaskAsync(taskId, stoppingToken);
            }
            else
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        _logger.LogInformation("Scan Background Service is stopping.");
    }

    private async Task ProcessTaskAsync(string taskId, CancellationToken stoppingToken)
    {
        var task = _taskManager.GetTask(taskId);
        if (task == null)
        {
            _logger.LogWarning("Task {TaskId} not found", taskId);
            return;
        }

        try
        {
            _taskManager.StartTask(taskId);

            using var scope = _serviceProvider.CreateScope();
            var fileScanner = scope.ServiceProvider.GetRequiredService<IFileScannerService>();
            var metadataService = scope.ServiceProvider.GetRequiredService<IMetadataService>();
            var mediaRepository = scope.ServiceProvider.GetRequiredService<IMediaRepository>();
            var thumbnailService = scope.ServiceProvider.GetRequiredService<IThumbnailService>();
            var hashService = scope.ServiceProvider.GetRequiredService<IHashService>();

            // 第一步：发现文件
            await _taskManager.UpdateProgressAsync(taskId, 0, 0, "", "正在扫描目录...");
            var files = (await fileScanner.DiscoverFilesAsync(task.LibraryPath, task.Recursive, task.CancellationTokenSource?.Token ?? stoppingToken)).ToList();
            
            var totalFiles = files.Count;
            var processed = 0;
            var newFiles = 0;
            var updatedFiles = 0;
            var skippedFiles = 0;
            var errors = new List<string>();

            await _taskManager.UpdateProgressAsync(taskId, 0, totalFiles, "", $"发现 {totalFiles} 个文件，开始处理...");

            // 第二步：逐个处理文件
            foreach (var filePath in files)
            {
                if (task.CancellationTokenSource?.IsCancellationRequested ?? stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Task {TaskId} was cancelled", taskId);
                    await _taskManager.FailTaskAsync(taskId, "任务被取消");
                    return;
                }

                try
                {
                    processed++;
                    var fileName = Path.GetFileName(filePath);
                    await _taskManager.UpdateProgressAsync(taskId, processed, totalFiles, fileName, $"正在处理: {fileName}");

                    // 检查文件是否已存在
                    var existingMedia = await mediaRepository.GetByPathAsync(filePath);
                    var fileInfo = new FileInfo(filePath);
                    
                    if (existingMedia != null)
                    {
                        // 检查文件是否有变化
                        if (existingMedia.LastModified >= fileInfo.LastWriteTimeUtc && 
                            existingMedia.FileSize == fileInfo.Length)
                        {
                            skippedFiles++;
                            continue;
                        }
                    }

                    // 计算哈希
                    var hash = await hashService.ComputeHashAsync(filePath);

                    // 提取元数据
                    var metadataResult = await metadataService.ExtractAsync(filePath);

                    // 创建媒体文件对象
                    MediaFile mediaFile;
                    switch (metadataResult.MediaType)
                    {
                        case MediaManager.Core.Enums.MediaType.Audio:
                            mediaFile = new AudioFile
                            {
                                Title = metadataResult.Title ?? Path.GetFileNameWithoutExtension(filePath),
                                Artist = metadataResult.Artist,
                                Album = metadataResult.Album,
                                Year = metadataResult.Year,
                                Genre = metadataResult.Genre,
                                TrackNumber = metadataResult.TrackNumber,
                                BitRate = metadataResult.BitRate,
                                SampleRate = metadataResult.SampleRate,
                                Channels = metadataResult.Channels
                            };
                            break;
                        case MediaManager.Core.Enums.MediaType.Video:
                            var videoFile = new VideoFile
                            {
                                Title = metadataResult.Title ?? Path.GetFileNameWithoutExtension(filePath),
                                Width = metadataResult.Width ?? 0,
                                Height = metadataResult.Height ?? 0,
                                FrameRate = metadataResult.FrameRate ?? 0,
                                VideoCodec = metadataResult.VideoCodec,
                                AudioCodec = metadataResult.AudioCodec,
                                BitRate = metadataResult.BitRate
                            };
                            mediaFile = videoFile;
                            break;
                        case MediaManager.Core.Enums.MediaType.Image:
                            var imageFile = new ImageFile
                            {
                                Width = metadataResult.Width ?? 0,
                                Height = metadataResult.Height ?? 0
                            };
                            mediaFile = imageFile;
                            break;
                        default:
                            mediaFile = new OtherFile();
                            break;
                    }

                    // 设置共同属性
                    mediaFile.Path = filePath;
                    mediaFile.FileName = fileName;
                    mediaFile.FileSize = fileInfo.Length;
                    mediaFile.DurationSeconds = metadataResult.DurationSeconds ?? 0;
                    mediaFile.Hash = hash;
                    mediaFile.LastModified = fileInfo.LastWriteTimeUtc;
                    mediaFile.MediaType = metadataResult.MediaType;

                    // 保存或更新
                    if (existingMedia == null)
                    {
                        mediaFile.Id = Guid.NewGuid();
                        mediaFile.DateAdded = DateTime.UtcNow;
                        await mediaRepository.AddAsync(mediaFile);
                        newFiles++;
                    }
                    else
                    {
                        mediaFile.Id = existingMedia.Id;
                        mediaFile.DateAdded = existingMedia.DateAdded;
                        mediaFile.Rating = existingMedia.Rating;
                        mediaFile.Notes = existingMedia.Notes;
                        await mediaRepository.UpdateAsync(mediaFile);
                        updatedFiles++;
                    }

                    // 生成缩略图（对于视频和图片）
                    if (metadataResult.MediaType == MediaManager.Core.Enums.MediaType.Video ||
                        metadataResult.MediaType == MediaManager.Core.Enums.MediaType.Image)
                    {
                        try
                        {
                            await thumbnailService.GenerateThumbnailAsync(filePath, mediaFile.Id.ToString());
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to generate thumbnail for {Path}", filePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing file {Path}", filePath);
                    errors.Add($"处理 {Path.GetFileName(filePath)} 时出错: {ex.Message}");
                }
            }

            // 任务完成
            await _taskManager.CompleteTaskAsync(taskId, newFiles, updatedFiles, skippedFiles, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing task {TaskId}", taskId);
            await _taskManager.FailTaskAsync(taskId, ex.Message);
        }
    }
}
