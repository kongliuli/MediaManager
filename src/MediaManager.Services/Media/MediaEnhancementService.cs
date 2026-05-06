using MediaManager.Core.Interfaces.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace MediaManager.Services.Media;

/// <summary>
/// 媒体增强服务实现
/// </summary>
public class MediaEnhancementService : IMediaEnhancementService
{
    private readonly IThumbnailService _thumbnailService;
    private readonly IWaveformService _waveformService;
    private readonly IImageThumbnailService _imageThumbnailService;
    private readonly ICacheService _cacheService;

    public MediaEnhancementService(
        IThumbnailService thumbnailService,
        IWaveformService waveformService,
        IImageThumbnailService imageThumbnailService,
        ICacheService cacheService)
    {
        _thumbnailService = thumbnailService;
        _waveformService = waveformService;
        _imageThumbnailService = imageThumbnailService;
        _cacheService = cacheService;
    }

    public async Task<byte[]?> GenerateVideoThumbnailAsync(string filePath, TimeSpan? timestamp = null)
    {
        try
        {
            var cacheKey = $"thumb_{filePath.GetHashCode()}_{timestamp?.GetHashCode() ?? 0}";
            
            if (await _cacheService.ExistsAsync(cacheKey))
            {
                return await _cacheService.GetAsync<byte[]>(cacheKey);
            }

            var tempPath = Path.GetTempFileName() + ".png";
            
            if (timestamp.HasValue)
            {
                await _thumbnailService.GenerateWithTimeAsync(filePath, tempPath, timestamp.Value);
            }
            else
            {
                await _thumbnailService.GenerateAsync(filePath, tempPath);
            }

            if (File.Exists(tempPath))
            {
                var bytes = await File.ReadAllBytesAsync(tempPath);
                File.Delete(tempPath);
                
                await _cacheService.SetAsync(cacheKey, bytes, TimeSpan.FromMinutes(30));
                return bytes;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"生成视频缩略图失败: {ex.Message}");
        }
        return null;
    }

    public async Task<byte[]?> GenerateAudioWaveformAsync(string filePath, int width = 400, int height = 100)
    {
        try
        {
            var cacheKey = $"wave_{filePath.GetHashCode()}_{width}_{height}";
            
            if (await _cacheService.ExistsAsync(cacheKey))
            {
                return await _cacheService.GetAsync<byte[]>(cacheKey);
            }

            var tempPath = Path.GetTempFileName() + ".png";
            await _waveformService.GenerateAsync(filePath, tempPath, width, height);
            
            if (File.Exists(tempPath))
            {
                var bytes = await File.ReadAllBytesAsync(tempPath);
                File.Delete(tempPath);
                
                await _cacheService.SetAsync(cacheKey, bytes, TimeSpan.FromMinutes(30));
                return bytes;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"生成音频波形失败: {ex.Message}");
        }
        return null;
    }

    public async Task<byte[]?> GenerateImagePreviewAsync(string filePath, int maxWidth = 800, int maxHeight = 800)
    {
        try
        {
            var cacheKey = $"preview_{filePath.GetHashCode()}_{maxWidth}_{maxHeight}";
            
            if (await _cacheService.ExistsAsync(cacheKey))
            {
                return await _cacheService.GetAsync<byte[]>(cacheKey);
            }

            using var image = await Image.LoadAsync(filePath);
            
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(maxWidth, maxHeight),
                Mode = ResizeMode.Max
            }));

            using var stream = new MemoryStream();
            await image.SaveAsync(stream, new PngEncoder());
            var bytes = stream.ToArray();
            
            await _cacheService.SetAsync(cacheKey, bytes, TimeSpan.FromMinutes(30));
            return bytes;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"生成图像预览失败: {ex.Message}");
        }
        return null;
    }

    public async Task<string?> GenerateBlurHashAsync(string filePath, int componentsX = 4, int componentsY = 3)
    {
        try
        {
            var cacheKey = $"blurhash_{filePath.GetHashCode()}_{componentsX}_{componentsY}";
            
            if (await _cacheService.ExistsAsync(cacheKey))
            {
                return await _cacheService.GetAsync<string>(cacheKey);
            }

            using var image = await Image.LoadAsync<Rgba32>(filePath);
            var blurHash = EncodeBlurHash(image, componentsX, componentsY);
            
            await _cacheService.SetAsync(cacheKey, blurHash, TimeSpan.FromHours(1));
            return blurHash;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"生成BlurHash失败: {ex.Message}");
        }
        return null;
    }

    public async Task<string?> GetOrCreateThumbnailAsync(string filePath, string cacheDirectory)
    {
        try
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath) + "_thumb.jpg";
            var cachePath = Path.Combine(cacheDirectory, fileName);
            
            if (File.Exists(cachePath))
            {
                return cachePath;
            }

            Directory.CreateDirectory(cacheDirectory);
            await _thumbnailService.GenerateAsync(filePath, cachePath);
            
            return File.Exists(cachePath) ? cachePath : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"获取或创建缩略图失败: {ex.Message}");
            return null;
        }
    }

    private string EncodeBlurHash(Image<Rgba32> image, int componentsX, int componentsY)
    {
        var width = image.Width;
        var height = image.Height;
        
        var factors = new float[componentsX * componentsY * 3];
        
        for (var y = 0; y < componentsY; y++)
        {
            for (var x = 0; x < componentsX; x++)
            {
                var r = 0f;
                var g = 0f;
                var b = 0f;
                
                for (var py = 0; py < height; py++)
                {
                    for (var px = 0; px < width; px++)
                    {
                        var color = image[px, py];
                        var basis = (float)(Math.Cos(Math.PI * x * px / width) * Math.Cos(Math.PI * y * py / height));
                        r += color.R * basis;
                        g += color.G * basis;
                        b += color.B * basis;
                    }
                }
                
                var scale = (x == 0 && y == 0) ? 1f : 2f;
                var factor = scale / (width * height);
                
                factors[(y * componentsX + x) * 3 + 0] = r * factor;
                factors[(y * componentsX + x) * 3 + 1] = g * factor;
                factors[(y * componentsX + x) * 3 + 2] = b * factor;
            }
        }
        
        return EncodeBlurHashBase83(factors, componentsX, componentsY);
    }

    private string EncodeBlurHashBase83(float[] factors, int componentsX, int componentsY)
    {
        const string alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz#$%*+,-.:;=?@[]^_{|}~";
        
        var hash = new char[1 + 2 + 4 * factors.Length / 2];
        var sizeFlag = (componentsX - 1) + (componentsY - 1) * 9;
        
        hash[0] = alphabet[sizeFlag];
        hash[1] = alphabet[8];
        hash[2] = alphabet[8];
        
        var value = LinearTosRGB(factors[0]);
        var quantR = (int)Math.Floor(Math.Clamp(value / (256f / 18f), 0, 17));
        var quantG = (int)Math.Floor(Math.Clamp(LinearTosRGB(factors[1]) / (256f / 18f), 0, 17));
        var quantB = (int)Math.Floor(Math.Clamp(LinearTosRGB(factors[2]) / (256f / 18f), 0, 17));
        
        hash[3] = alphabet[quantR * 19 * 19 + quantG * 19 + quantB];
        
        for (var i = 1; i < factors.Length / 3; i++)
        {
            var r = LinearTosRGB(factors[i * 3 + 0]);
            var g = LinearTosRGB(factors[i * 3 + 1]);
            var b = LinearTosRGB(factors[i * 3 + 2]);
            
            var quantR2 = (int)Math.Floor(Math.Clamp(r / (256f / 9f), 0, 8));
            var quantG2 = (int)Math.Floor(Math.Clamp(g / (256f / 9f), 0, 8));
            var quantB2 = (int)Math.Floor(Math.Clamp(b / (256f / 9f), 0, 8));
            
            hash[4 + i * 2] = alphabet[quantR2 * 9 * 9 + quantG2 * 9 + quantB2];
        }
        
        return new string(hash).TrimEnd('\0');
    }

    private float LinearTosRGB(float value)
    {
        return value <= 0.0031308f ? value * 12.92f : (float)(1.055 * Math.Pow(value, 1 / 2.4) - 0.055);
    }
}
