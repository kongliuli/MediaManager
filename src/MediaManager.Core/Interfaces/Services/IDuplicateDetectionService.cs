using MediaManager.Core.DTOs;
using MediaManager.Core.Models;

namespace MediaManager.Core.Interfaces.Services;

/// <summary>
/// 重复文件检测服务接口。
/// 基于 SHA256 哈希识别内容完全相同的文件。
/// </summary>
public interface IDuplicateDetectionService
{
    Task<IReadOnlyList<DuplicateGroup>> FindDuplicatesAsync(CancellationToken ct = default);
}
