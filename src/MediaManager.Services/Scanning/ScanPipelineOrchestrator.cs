using CommunityToolkit.Mvvm.Messaging;
using MediaManager.Core.DTOs;
using MediaManager.Core.Enums;
using MediaManager.Core.Interfaces.Repositories;
using MediaManager.Core.Interfaces.Services;
using MediaManager.Core.Messages;
using MediaManager.Core.Models;
using Serilog;
using System;
using System.IO;

namespace MediaManager.Services.Scanning;

/// <summary>
/// 扫描管道协调器。
/// 驱动完整的文件摄入流程：发现 → 哈希 → 元数据 → 缩略图/波形图 → 持久化。
/// 通过 IProgress&lt;ScanProgressReport&gt; 向 UI 层推送进度。
/// </summary>
public class ScanPipelineOrchestrator(
    IFileScannerService scanner,
    IHashService hashService,
    IMetadataService metadataService,
    IThumbnailService thumbnailService,
    IImageThumbnailService imageThumbnailService,
    IWaveformService waveformService,
    IMediaRepository mediaRepository)
{
    private IProgress<ScanLogMessage>? _logProgress;
    private bool IsScanning;

    /// <summary>
    /// 执行完整扫描管道（默认包含所有类型）。
    /// 文件发现后并行处理（哈希 + 元数据），缩略图/波形图串行生成以控制 CPU 占用。
    /// </summary>
    public async Task RunAsync(string directory, string thumbnailDir, bool recursive = true, IProgress<ScanProgressReport>? progress = null, CancellationToken ct = default)
    {
        await RunAsync(directory, thumbnailDir, true, true, false, recursive, progress, null, ct, null);
    }

    /// <summary>
    /// 执行完整扫描管道（带过滤选项）。
    /// 文件发现后并行处理（哈希 + 元数据），缩略图/波形图串行生成以控制 CPU 占用。
    /// </summary>
    public async Task RunAsync(string directory, string thumbnailDir, bool includeAudio, bool includeVideo, 
                               bool includeImages, bool recursive = true, IProgress<ScanProgressReport>? progress = null,
                               IProgress<ScanLogMessage>? logProgress = null, CancellationToken ct = default,
                               int? libraryId = null)
    {
        IsScanning = true;
        ScanProgressReport report = new ScanProgressReport { Status = ScanStatus.Idle };
        try
        {
            _logProgress = logProgress;
            
            report = new ScanProgressReport { Status = ScanStatus.Scanning };
            var fileList = new List<string>();

            // 阶段 1：发现文件
            report.CurrentStep = ScanStep.DiscoveringFiles;
            report.StepDescription = "正在发现媒体文件...";
            LogStep(ScanStep.DiscoveringFiles, "开始发现文件", LogMessageType.StepStart);
            
            await foreach (var path in scanner.DiscoverFilesAsync(directory, includeAudio, includeVideo, includeImages, recursive, ct))
            {
                fileList.Add(path);
                report.TotalDiscovered = fileList.Count;
                
                // 记录发现的文件
                var ext = Path.GetExtension(path).ToLower();
                LogMessage(LogMessageType.Info, $"发现文件: {Path.GetFileName(path)}", path, ext, GetMediaTypeFromExtension(ext));
                
                // 每发现 10 个文件报告一次进度，避免频繁更新 UI
                if (fileList.Count % 10 == 0)
                {
                    progress?.Report(report);
                }
            }

            LogStep(ScanStep.DiscoveringFiles, $"文件发现完成，共发现 {fileList.Count} 个文件", LogMessageType.StepComplete);
            progress?.Report(report);

            // 阶段 2：批量处理文件
            const int batchSize = 5;
            var batches = fileList.Select((file, index) => new { file, index })
                .GroupBy(x => x.index / batchSize)
                .Select(g => g.Select(x => x.file).ToList())
                .ToList();

            foreach (var batch in batches)
            {
                ct.ThrowIfCancellationRequested();

                // 并行处理批次中的文件
                var tasks = batch.Select(async filePath =>
                {
                    try
                    {
                        var extension = Path.GetExtension(filePath).ToLower();
                        var mediaType = GetMediaTypeFromExtension(extension);
                        
                        LogMessage(LogMessageType.Info, $"开始处理: {Path.GetFileName(filePath)}", filePath, extension, mediaType);

                        // 1. 计算文件哈希（必须步骤）
                        report.CurrentStep = ScanStep.CalculatingHash;
                        report.StepDescription = $"计算哈希: {Path.GetFileName(filePath)}";
                        LogStep(ScanStep.CalculatingHash, $"计算哈希: {Path.GetFileName(filePath)}", LogMessageType.StepStart);
                        
                        string hash;
                        try
                        {
                            hash = await hashService.ComputeAsync(filePath, ct);
                            report.HashSuccess++;
                            LogStep(ScanStep.CalculatingHash, $"哈希计算成功: {hash.Substring(0, 8)}...", LogMessageType.StepComplete);
                        }
                        catch (Exception ex)
                        {
                            report.HashFailed++;
                            LogStep(ScanStep.CalculatingHash, $"哈希计算失败: {ex.Message}", LogMessageType.StepFailed);
                            throw;
                        }

                        // 2. 尝试提取元数据（可选步骤，失败不影响落库）
                        report.CurrentStep = ScanStep.ExtractingMetadata;
                        report.StepDescription = $"提取元数据: {Path.GetFileName(filePath)}";
                        LogStep(ScanStep.ExtractingMetadata, $"提取元数据: {Path.GetFileName(filePath)}", LogMessageType.StepStart);
                        
                        MetadataResult? meta = null;
                        bool metadataExtracted = false;
                        try
                        {
                            meta = await metadataService.ExtractAsync(filePath, ct);
                            metadataExtracted = true;
                            report.MetadataSuccess++;
                            LogStep(ScanStep.ExtractingMetadata, $"元数据提取成功: {meta.MediaType}", LogMessageType.StepComplete);
                        }
                        catch (Exception ex)
                        {
                            report.MetadataFailed++;
                            LogStep(ScanStep.ExtractingMetadata, $"元数据提取失败: {ex.Message}", LogMessageType.StepFailed);
                            // 元数据提取失败，继续处理
                        }

                        // 3. 构建领域模型
                        MediaFile mediaFile;
                        
                        if (metadataExtracted && meta != null)
                        {
                            mediaFile = meta.MediaType switch
                            {
                                MediaType.Audio => BuildAudioFile(filePath, hash, meta, libraryId),
                                MediaType.Video => BuildVideoFile(filePath, hash, meta, libraryId),
                                MediaType.Image => BuildImageFile(filePath, hash, meta, libraryId),
                                _ => BuildAudioFile(filePath, hash, meta, libraryId)
                            };
                        }
                        else
                        {
                            // 元数据提取失败时，根据文件扩展名判断媒体类型
                            if (IsAudioFile(extension))
                            {
                                mediaFile = new AudioFile
                                {
                                    Path = filePath,
                                    FileName = Path.GetFileName(filePath),
                                    FileSize = new FileInfo(filePath).Length,
                                    Hash = hash,
                                    DateAdded = DateTime.UtcNow,
                                    LastModified = File.GetLastWriteTimeUtc(filePath),
                                    MediaType = MediaType.Audio,
                                    LibraryId = libraryId
                                };
                            }
                            else if (IsVideoFile(extension))
                            {
                                mediaFile = new VideoFile
                                {
                                    Path = filePath,
                                    FileName = Path.GetFileName(filePath),
                                    FileSize = new FileInfo(filePath).Length,
                                    Hash = hash,
                                    DateAdded = DateTime.UtcNow,
                                    LastModified = File.GetLastWriteTimeUtc(filePath),
                                    MediaType = MediaType.Video,
                                    LibraryId = libraryId
                                };
                            }
                            else if (IsImageFile(extension))
                            {
                                mediaFile = new ImageFile
                                {
                                    Path = filePath,
                                    FileName = Path.GetFileName(filePath),
                                    FileSize = new FileInfo(filePath).Length,
                                    Hash = hash,
                                    DateAdded = DateTime.UtcNow,
                                    LastModified = File.GetLastWriteTimeUtc(filePath),
                                    MediaType = MediaType.Image,
                                    Format = extension.TrimStart('.').ToUpper(),
                                    LibraryId = libraryId
                                };
                            }
                            else
                            {
                                // 非定义的其他类型保存为 Other
                                mediaFile = new OtherFile
                                {
                                    Path = filePath,
                                    FileName = Path.GetFileName(filePath),
                                    FileSize = new FileInfo(filePath).Length,
                                    Hash = hash,
                                    DateAdded = DateTime.UtcNow,
                                    LastModified = File.GetLastWriteTimeUtc(filePath),
                                    MediaType = MediaType.Other,
                                    Extension = extension.TrimStart('.').ToUpper(),
                                    LibraryId = libraryId
                                };
                            }
                        }

                        // 4. 先落库，确保文件信息存储到数据库
                        report.CurrentStep = ScanStep.SavingToDatabase;
                        report.StepDescription = $"保存到数据库: {Path.GetFileName(filePath)}";
                        LogStep(ScanStep.SavingToDatabase, $"保存到数据库: {Path.GetFileName(filePath)}", LogMessageType.StepStart);
                        
                        try
                        {
                            await mediaRepository.UpsertAsync(mediaFile, ct);
                            report.DatabaseSuccess++;
                            LogStep(ScanStep.SavingToDatabase, "入库成功", LogMessageType.StepComplete);
                        }
                        catch (Exception ex)
                        {
                            report.DatabaseFailed++;
                            LogStep(ScanStep.SavingToDatabase, $"入库失败: {ex.Message}", LogMessageType.StepFailed);
                            throw;
                        }

                        // 5. 生成预览资源（非强制，失败不影响落库）
                        if (mediaFile is VideoFile video)
                        {
                            report.CurrentStep = ScanStep.GeneratingThumbnail;
                            report.StepDescription = $"生成缩略图: {Path.GetFileName(filePath)}";
                            LogStep(ScanStep.GeneratingThumbnail, $"生成视频缩略图: {Path.GetFileName(filePath)}", LogMessageType.StepStart);
                            
                            try
                            {
                                var thumbPath = Path.Combine(thumbnailDir, $"{hash}.png");
                                await thumbnailService.GenerateAsync(filePath, thumbPath, ct: ct);
                                video.ThumbnailPath = thumbPath;
                                await mediaRepository.UpsertAsync(video, ct);
                                report.ThumbnailSuccess++;
                                LogStep(ScanStep.GeneratingThumbnail, "缩略图生成成功", LogMessageType.StepComplete);
                            }
                            catch (Exception ex)
                            {
                                report.ThumbnailFailed++;
                                LogStep(ScanStep.GeneratingThumbnail, $"缩略图生成失败: {ex.Message}", LogMessageType.StepFailed);
                            }
                        }
                        else if (mediaFile is AudioFile audio)
                        {
                            report.CurrentStep = ScanStep.GeneratingWaveform;
                            report.StepDescription = $"生成波形图: {Path.GetFileName(filePath)}";
                            LogStep(ScanStep.GeneratingWaveform, $"生成音频波形图: {Path.GetFileName(filePath)}", LogMessageType.StepStart);
                            
                            try
                            {
                                var waveformPath = Path.Combine(thumbnailDir, $"{hash}_waveform.png");
                                await waveformService.GenerateAsync(filePath, waveformPath, ct: ct);
                                audio.WaveformImagePath = waveformPath;
                                await mediaRepository.UpsertAsync(audio, ct);
                                report.WaveformSuccess++;
                                LogStep(ScanStep.GeneratingWaveform, "波形图生成成功", LogMessageType.StepComplete);
                            }
                            catch (Exception ex)
                            {
                                report.WaveformFailed++;
                                LogStep(ScanStep.GeneratingWaveform, $"波形图生成失败: {ex.Message}", LogMessageType.StepFailed);
                            }
                        }
                        else if (mediaFile is ImageFile image)
                        {
                            report.CurrentStep = ScanStep.GeneratingThumbnail;
                            report.StepDescription = $"生成缩略图: {Path.GetFileName(filePath)}";
                            LogStep(ScanStep.GeneratingThumbnail, $"生成图片缩略图: {Path.GetFileName(filePath)}", LogMessageType.StepStart);
                            
                            try
                            {
                                var thumbPath = Path.Combine(thumbnailDir, $"{hash}_thumb.png");
                                await imageThumbnailService.GenerateAsync(filePath, thumbPath, 200, 200, ct);
                                image.ThumbnailPath = thumbPath;
                                await mediaRepository.UpsertAsync(image, ct);
                                report.ThumbnailSuccess++;
                                LogStep(ScanStep.GeneratingThumbnail, "缩略图生成成功", LogMessageType.StepComplete);
                            }
                            catch (Exception ex)
                            {
                                report.ThumbnailFailed++;
                                LogStep(ScanStep.GeneratingThumbnail, $"缩略图生成失败: {ex.Message}", LogMessageType.StepFailed);
                            }
                        }

                        LogMessage(LogMessageType.Success, $"文件处理完成: {Path.GetFileName(filePath)}", filePath, extension, mediaType);
                        return true; // 成功
                    }
                    catch (Exception ex)
                    {
                        LogMessage(LogMessageType.Error, $"文件处理失败: {ex.Message}", filePath);
                        Log.Error(ex, "文件处理失败: {FilePath}", filePath);
                        return false; // 失败
                    }
                }).ToList();

                // 等待批次处理完成
                var results = await Task.WhenAll(tasks);

                // 更新进度
                report.Processed += results.Count(r => r);
                report.Errors += results.Count(r => !r);
                report.CurrentFile = batch.LastOrDefault() != null ? Path.GetFileName(batch.Last()) : string.Empty;
                progress?.Report(report);
            }

            report.CurrentStep = ScanStep.Completed;
            report.StepDescription = "扫描完成";
            report.Status = ScanStatus.Completed;
            
            LogStep(ScanStep.Completed, 
                $"扫描完成 - 总计: {report.TotalDiscovered}, 成功: {report.Processed}, 错误: {report.Errors}" +
                $", 哈希成功: {report.HashSuccess}, 元数据成功: {report.MetadataSuccess}, " +
                $"入库成功: {report.DatabaseSuccess}, 缩略图成功: {report.ThumbnailSuccess}, 波形图成功: {report.WaveformSuccess}", 
                LogMessageType.StepComplete);
            
            progress?.Report(report);
        }
        finally
        {
            IsScanning = false;
            WeakReferenceMessenger.Default.Send(new ScanCompletedMessage(report));
        }
    }

    private void LogMessage(LogMessageType type, string message, string? filePath = null, 
                           string? fileExtension = null, MediaType? mediaType = null)
    {
        _logProgress?.Report(new ScanLogMessage
        {
            Type = type,
            Message = message,
            FilePath = filePath,
            FileExtension = fileExtension,
            MediaType = mediaType
        });
    }

    private void LogStep(ScanStep step, string message, LogMessageType type)
    {
        _logProgress?.Report(new ScanLogMessage
        {
            Type = type,
            Message = message,
            Step = step
        });
    }

    private MediaType GetMediaTypeFromExtension(string extension)
    {
        if (IsAudioFile(extension)) return MediaType.Audio;
        if (IsVideoFile(extension)) return MediaType.Video;
        if (IsImageFile(extension)) return MediaType.Image;
        return MediaType.Other;
    }

    private static AudioFile BuildAudioFile(string path, string hash, MetadataResult meta, int? libraryId) => new()
    {
        Path = path,
        FileName = Path.GetFileName(path),
        FileSize = meta.FileSize,
        DurationSeconds = meta.DurationSeconds,
        Hash = hash,
        DateAdded = DateTime.UtcNow,
        LastModified = File.GetLastWriteTimeUtc(path),
        MediaType = MediaType.Audio,
        BitRate = meta.BitRate,
        SampleRate = meta.SampleRate,
        Channels = meta.Channels,
        Codec = meta.AudioCodec ?? string.Empty,
        Title = meta.Title,
        Artist = meta.Artist,
        Album = meta.Album,
        Year = meta.Year,
        Genre = meta.Genre,
        LibraryId = libraryId
    };

    private static VideoFile BuildVideoFile(string path, string hash, MetadataResult meta, int? libraryId) => new()
    {
        Path = path,
        FileName = Path.GetFileName(path),
        FileSize = meta.FileSize,
        DurationSeconds = meta.DurationSeconds,
        Hash = hash,
        DateAdded = DateTime.UtcNow,
        LastModified = File.GetLastWriteTimeUtc(path),
        MediaType = MediaType.Video,
        Width = meta.Width,
        Height = meta.Height,
        FrameRate = meta.FrameRate,
        VideoCodec = meta.VideoCodec ?? string.Empty,
        AudioCodec = meta.AudioCodec ?? string.Empty,
        BitRate = meta.VideoBitRate,
        Title = meta.Title,
        LibraryId = libraryId
    };

    private static ImageFile BuildImageFile(string path, string hash, MetadataResult meta, int? libraryId) => new()
    {
        Path = path,
        FileName = Path.GetFileName(path),
        FileSize = meta.FileSize,
        Hash = hash,
        DateAdded = DateTime.UtcNow,
        LastModified = File.GetLastWriteTimeUtc(path),
        MediaType = MediaType.Image,
        Width = meta.Width,
        Height = meta.Height,
        Format = Path.GetExtension(path).TrimStart('.').ToUpper(),
        LibraryId = libraryId
    };

    private static bool IsAudioFile(string extension)
    {
        var audioExtensions = new[] { ".mp3", ".wav", ".flac", ".aac", ".ogg", ".wma", ".m4a", ".opus" };
        return audioExtensions.Contains(extension);
    }

    private static bool IsVideoFile(string extension)
    {
        var videoExtensions = new[] { ".mp4", ".avi", ".mkv", ".wmv", ".mov", ".flv", ".webm", ".m4v" };
        return videoExtensions.Contains(extension);
    }

    private static bool IsImageFile(string extension)
    {
        var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp", ".svg" };
        return imageExtensions.Contains(extension);
    }
}
