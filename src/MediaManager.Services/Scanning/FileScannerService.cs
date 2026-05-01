using MediaManager.Core.Interfaces.Services;
using System.IO;
using System.Runtime.CompilerServices;

namespace MediaManager.Services.Scanning;

/// <summary>
/// 文件扫描服务。
/// 递归遍历目录，流式返回支持的媒体文件路径。
/// </summary>
public class FileScannerService : IFileScannerService
{
    // 支持的音频文件扩展名
    private readonly HashSet<string> _supportedAudioExtensions = new()
    {
        ".mp3", ".wav", ".flac", ".aac", ".ogg", ".wma", ".m4a", ".opus"
    };

    // 支持的视频文件扩展名
    private readonly HashSet<string> _supportedVideoExtensions = new()
    {
        ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv", ".webm", ".m4v"
    };

    // 支持的图像文件扩展名
    private readonly HashSet<string> _supportedImageExtensions = new()
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp", ".svg"
    };

    /// <summary>
    /// 递归遍历目录，流式返回支持的媒体文件路径。
    /// </summary>
    /// <param name="directory">要扫描的目录路径</param>
    /// <param name="recursive">是否递归扫描子目录</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>支持的媒体文件路径的异步流</returns>
    public async IAsyncEnumerable<string> DiscoverFilesAsync(string directory, bool recursive = true, [EnumeratorCancellation] CancellationToken ct = default)
    {
        // 默认包含所有类型
        await foreach (var filePath in DiscoverFilesAsync(directory, true, true, true, recursive, ct))
        {
            yield return filePath;
        }
    }

    /// <summary>
    /// 带过滤选项的文件扫描
    /// </summary>
    public async IAsyncEnumerable<string> DiscoverFilesAsync(string directory, bool includeAudio, bool includeVideo, bool includeImages,
                                                             bool recursive = true, [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (!Directory.Exists(directory))
        {
            yield break;
        }

        // 扫描当前目录
        foreach (var filePath in Directory.EnumerateFiles(directory))
        {
            ct.ThrowIfCancellationRequested();

            if (IsSupportedFormat(filePath, includeAudio, includeVideo, includeImages))
            {
                yield return filePath;
            }
        }

        // 递归扫描子目录
        if (recursive)
        {
            foreach (var subDirectory in Directory.EnumerateDirectories(directory))
            {
                ct.ThrowIfCancellationRequested();

                await foreach (var filePath in DiscoverFilesAsync(subDirectory, includeAudio, includeVideo, includeImages, true, ct))
                {
                    ct.ThrowIfCancellationRequested();
                    yield return filePath;
                }
            }
        }
    }

    /// <summary>
    /// 检查文件是否为支持的媒体格式（带过滤选项）
    /// </summary>
    private bool IsSupportedFormat(string filePath, bool includeAudio, bool includeVideo, bool includeImages)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return (includeAudio && _supportedAudioExtensions.Contains(extension)) ||
               (includeVideo && _supportedVideoExtensions.Contains(extension)) ||
               (includeImages && _supportedImageExtensions.Contains(extension));
    }

    /// <summary>
    /// 检查文件是否为支持的媒体格式。
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>如果文件是支持的媒体格式，返回 true；否则返回 false</returns>
    public bool IsSupportedFormat(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return _supportedAudioExtensions.Contains(extension) || 
               _supportedVideoExtensions.Contains(extension) ||
               _supportedImageExtensions.Contains(extension);
    }

    /// <summary>
    /// 检查文件是否为音频格式。
    /// </summary>
    public bool IsAudioFile(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return _supportedAudioExtensions.Contains(extension);
    }

    /// <summary>
    /// 检查文件是否为视频格式。
    /// </summary>
    public bool IsVideoFile(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return _supportedVideoExtensions.Contains(extension);
    }

    /// <summary>
    /// 检查文件是否为图像格式。
    /// </summary>
    public bool IsImageFile(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return _supportedImageExtensions.Contains(extension);
    }
}