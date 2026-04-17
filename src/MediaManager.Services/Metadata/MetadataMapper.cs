using MediaManager.Core.DTOs;
using MediaManager.Core.Enums;

namespace MediaManager.Services.Metadata;

/// <summary>
/// 将 FFprobe JSON 输出映射到 MetadataResult DTO。
/// 独立为静态类，便于单元测试。
/// </summary>
public static class MetadataMapper
{
    /// <summary>
    /// 从 FFprobe JSON 字符串解析元数据。
    /// </summary>
    /// <param name="ffprobeJson">ffprobe -print_format json 的输出</param>
    /// <param name="fileSize">文件大小（字节），从 FileInfo 获取</param>
    public static MetadataResult Map(string ffprobeJson, long fileSize)
    {
        // 实现要点：
        // 1. 使用 System.Text.Json 解析 JSON
        // 2. 遍历 streams 数组，找到 codec_type == "video" 和 "audio" 的流
        // 3. 从 format.tags 提取 title, artist, album 等 ID3 标签
        // 4. 根据是否存在视频流判断 MediaType
        throw new NotImplementedException();
    }
}
