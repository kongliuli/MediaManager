using MediaManager.Core.DTOs;

namespace MediaManager.Core.Interfaces.Services;

/// <summary>
/// 媒体元数据提取服务接口。
/// 通过 FFprobe 解析音视频文件的流信息和容器元数据。
/// </summary>
public interface IMetadataService
{
    Task<MetadataResult> ExtractAsync(string filePath, CancellationToken ct = default);
}
