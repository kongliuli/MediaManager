namespace MediaManager.Core.Interfaces.Services;

/// <summary>
/// 媒体增强服务接口 - 提供缩略图、波形、预览等增强功能
/// </summary>
public interface IMediaEnhancementService
{
    /// <summary>
    /// 生成视频缩略图
    /// </summary>
    Task<byte[]?> GenerateVideoThumbnailAsync(string filePath, TimeSpan? timestamp = null);
    
    /// <summary>
    /// 生成音频波形图
    /// </summary>
    Task<byte[]?> GenerateAudioWaveformAsync(string filePath, int width = 400, int height = 100);
    
    /// <summary>
    /// 生成图片预览
    /// </summary>
    Task<byte[]?> GenerateImagePreviewAsync(string filePath, int maxWidth = 800, int maxHeight = 800);
    
    /// <summary>
    /// 生成 BlurHash 编码（用于图片占位符）
    /// </summary>
    Task<string?> GenerateBlurHashAsync(string filePath, int componentsX = 4, int componentsY = 3);
    
    /// <summary>
    /// 获取或生成缩略图（带缓存）
    /// </summary>
    Task<string?> GetOrCreateThumbnailAsync(string filePath, string cacheDirectory);
}
