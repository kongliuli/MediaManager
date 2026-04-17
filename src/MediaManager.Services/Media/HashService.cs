using MediaManager.Core.Interfaces.Services;
using System;
using System.IO;
using System.Security.Cryptography;

namespace MediaManager.Services.Media;

/// <summary>
/// SHA256 哈希计算服务。
/// 使用流式读取，支持大文件而不占用大量内存。
/// </summary>
public class HashService : IHashService
{
    public async Task<string> ComputeAsync(string filePath, CancellationToken ct = default)
    {
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read,
            FileShare.Read, bufferSize: 81920, useAsync: true);

        var hashBytes = await SHA256.HashDataAsync(stream, ct);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
