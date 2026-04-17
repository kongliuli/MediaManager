using MediaManager.Core.DTOs;
using MediaManager.Core.Enums;
using MediaManager.Core.Interfaces.Repositories;
using MediaManager.Core.Interfaces.Services;
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
    IWaveformService waveformService,
    IMediaRepository mediaRepository)
{
    /// <summary>
    /// 执行完整扫描管道。
    /// 文件发现后并行处理（哈希 + 元数据），缩略图/波形图串行生成以控制 CPU 占用。
    /// </summary>
    public async Task RunAsync(string directory, string thumbnailDir, bool recursive = true, IProgress<ScanProgressReport>? progress = null, CancellationToken ct = default)
    {
        var report = new ScanProgressReport { Status = ScanStatus.Scanning };
        var fileList = new List<string>();

        // 阶段 1：收集所有文件路径
        await foreach (var path in scanner.DiscoverFilesAsync(directory, recursive, ct))
        {
            fileList.Add(path);
            report.TotalDiscovered = fileList.Count;
            // 每发现 10 个文件报告一次进度，避免频繁更新 UI
            if (fileList.Count % 10 == 0)
            {
                progress?.Report(report);
            }
        }

        // 确保报告最终的发现数量
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
                    Log.Information("开始处理文件: {FilePath}", filePath);
                    
                    // 1. 计算文件哈希（必须步骤）
                    var hash = await hashService.ComputeAsync(filePath, ct);
                    Log.Information("文件哈希计算完成: {Hash}", hash);

                    // 2. 尝试提取元数据（可选步骤，失败不影响落库）
                    MetadataResult? meta = null;
                    bool metadataExtracted = false;
                    try
                    {
                        meta = await metadataService.ExtractAsync(filePath, ct);
                        metadataExtracted = true;
                        Log.Information("元数据提取完成: {MediaType}", meta.MediaType);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "元数据提取失败: {FilePath}", filePath);
                        // 元数据提取失败，继续处理
                    }

                    // 3. 构建领域模型
                    MediaFile mediaFile;
                    if (metadataExtracted && meta != null)
                    {
                        mediaFile = meta.MediaType == MediaType.Audio
                            ? BuildAudioFile(filePath, hash, meta)
                            : BuildVideoFile(filePath, hash, meta);
                    }
                    else
                    {
                        // 元数据提取失败时，根据文件扩展名判断媒体类型
                        var extension = Path.GetExtension(filePath).ToLower();
                        var isAudio = IsAudioFile(extension);
                        var isVideo = IsVideoFile(extension);
                        
                        if (isAudio)
                        {
                            mediaFile = new AudioFile
                            {
                                Path = filePath,
                                FileName = Path.GetFileName(filePath),
                                FileSize = new FileInfo(filePath).Length,
                                Hash = hash,
                                DateAdded = DateTime.UtcNow,
                                LastModified = File.GetLastWriteTimeUtc(filePath),
                                MediaType = MediaType.Audio
                            };
                            Log.Information("使用基本信息构建音频文件: {FilePath}", filePath);
                        }
                        else if (isVideo)
                        {
                            mediaFile = new VideoFile
                            {
                                Path = filePath,
                                FileName = Path.GetFileName(filePath),
                                FileSize = new FileInfo(filePath).Length,
                                Hash = hash,
                                DateAdded = DateTime.UtcNow,
                                LastModified = File.GetLastWriteTimeUtc(filePath),
                                MediaType = MediaType.Video
                            };
                            Log.Information("使用基本信息构建视频文件: {FilePath}", filePath);
                        }
                        else
                        {
                            // 默认构建为音频文件
                            mediaFile = new AudioFile
                            {
                                Path = filePath,
                                FileName = Path.GetFileName(filePath),
                                FileSize = new FileInfo(filePath).Length,
                                Hash = hash,
                                DateAdded = DateTime.UtcNow,
                                LastModified = File.GetLastWriteTimeUtc(filePath),
                                MediaType = MediaType.Audio
                            };
                            Log.Information("使用基本信息构建默认媒体文件: {FilePath}", filePath);
                        }
                    }

                    // 4. 先落库，确保文件信息存储到数据库
                    await mediaRepository.UpsertAsync(mediaFile, ct);
                    Log.Information("文件信息已落库: {FilePath}", filePath);

                    // 5. 生成预览资源（非强制，失败不影响落库）
                    if (mediaFile is VideoFile video)
                    {
                        try
                        {
                            var thumbPath = Path.Combine(thumbnailDir, $"{hash}.png");
                            await thumbnailService.GenerateAsync(filePath, thumbPath, ct: ct);
                            video.ThumbnailPath = thumbPath;
                            Log.Information("视频缩略图生成完成: {ThumbPath}", thumbPath);
                            // 更新数据库中的缩略图路径
                            await mediaRepository.UpsertAsync(video, ct);
                        }
                        catch (Exception ex)
                        {
                            Log.Warning(ex, "视频缩略图生成失败: {FilePath}", filePath);
                            // 缩略图生成失败，继续处理
                        }
                    }
                    else if (mediaFile is AudioFile audio)
                    {
                        try
                        {
                            var waveformPath = Path.Combine(thumbnailDir, $"{hash}_waveform.png");
                            await waveformService.GenerateAsync(filePath, waveformPath, ct: ct);
                            audio.WaveformImagePath = waveformPath;
                            Log.Information("音频波形图生成完成: {WaveformPath}", waveformPath);
                            // 更新数据库中的波形图路径
                            await mediaRepository.UpsertAsync(audio, ct);
                        }
                        catch (Exception ex)
                        {
                            Log.Warning(ex, "音频波形图生成失败: {FilePath}", filePath);
                            // 波形图生成失败，继续处理
                        }
                    }

                    Log.Information("文件处理完成: {FilePath}", filePath);
                    return true; // 成功
                }
                catch (Exception ex)
                {
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

        report.Status = ScanStatus.Completed;
        progress?.Report(report);
    }

    private static AudioFile BuildAudioFile(string path, string hash, MetadataResult meta) => new()
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
        Genre = meta.Genre
    };

    private static VideoFile BuildVideoFile(string path, string hash, MetadataResult meta) => new()
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
        Title = meta.Title
    };

    private static bool IsAudioFile(string extension)
    {
        var audioExtensions = new[] { ".mp3", ".wav", ".flac", ".aac", ".ogg", ".wma", ".m4a" };
        return audioExtensions.Contains(extension);
    }

    private static bool IsVideoFile(string extension)
    {
        var videoExtensions = new[] { ".mp4", ".avi", ".mkv", ".wmv", ".mov", ".flv", ".webm", ".m4v" };
        return videoExtensions.Contains(extension);
    }
}
