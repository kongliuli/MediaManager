namespace MediaManager.Core.Interfaces.Services;

/// <summary>
/// 文件哈希计算服务接口。
/// 使用 SHA256 对文件内容进行哈希，返回十六进制字符串。
/// </summary>
public interface IHashService
{
    Task<string> ComputeAsync(string filePath, CancellationToken ct = default);
}
