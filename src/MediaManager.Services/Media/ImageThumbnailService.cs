using MediaManager.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace MediaManager.Services.Media;

public class ImageThumbnailService : IImageThumbnailService
{
    private readonly ILogger<ImageThumbnailService> _logger;

    public ImageThumbnailService(ILogger<ImageThumbnailService> logger)
    {
        _logger = logger;
    }

    public async Task GenerateAsync(string imagePath, string outputPath, int maxWidth = 200, int maxHeight = 200, CancellationToken ct = default)
    {
        await Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                // 确保输出目录存在
                var outputDirectory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                using var originalImage = Image.FromFile(imagePath);
                
                // 计算缩放比例
                var scale = Math.Min((double)maxWidth / originalImage.Width, (double)maxHeight / originalImage.Height);
                
                if (scale >= 1)
                {
                    // 如果原始图片小于缩略图尺寸，直接复制
                    originalImage.Save(outputPath, ImageFormat.Png);
                    return;
                }

                var newWidth = (int)(originalImage.Width * scale);
                var newHeight = (int)(originalImage.Height * scale);

                using var thumbnailImage = new Bitmap(newWidth, newHeight);
                using var graphics = Graphics.FromImage(thumbnailImage);
                
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                
                graphics.DrawImage(originalImage, 0, 0, newWidth, newHeight);
                
                thumbnailImage.Save(outputPath, ImageFormat.Png);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "生成图片缩略图失败: {ImagePath}", imagePath);
            }
        }, ct);
    }
}