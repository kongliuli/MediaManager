namespace MediaManager.Core.Interfaces.Services;

/// <summary>
/// 文件扫描服务接口。
/// 递归遍历目录，流式返回支持的媒体文件路径。
/// </summary>
public interface IFileScannerService
{
    IAsyncEnumerable<string> DiscoverFilesAsync(string directory, bool recursive = true, CancellationToken ct = default);
    bool IsSupportedFormat(string filePath);
}
