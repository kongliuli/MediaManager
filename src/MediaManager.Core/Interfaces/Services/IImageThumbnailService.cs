namespace MediaManager.Core.Interfaces.Services;

/// <summary>
/// 图片缩略图生成服务接口。
/// 将图片缩放到指定大小并保存。
/// </summary>
public interface IImageThumbnailService
{
    /// <summary>
    /// 生成图片缩略图
    /// </summary>
    /// <param name="imagePath">原始图片路径</param>
    /// <param name="outputPath">输出路径</param>
    /// <param name="maxWidth">最大宽度</param>
    /// <param name="maxHeight">最大高度</param>
    /// <param name="ct">取消令牌</param>
    Task GenerateAsync(string imagePath, string outputPath, int maxWidth = 200, int maxHeight = 200, CancellationToken ct = default);
}