using MediaManager.Core.Interfaces.Services;

namespace MediaManager.Services.Media;

public class CachedMediaEnhancementService : IMediaEnhancementService
{
    private readonly IMediaEnhancementService _innerService;
    private readonly ICacheService _cacheService;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

    public CachedMediaEnhancementService(IMediaEnhancementService innerService, ICacheService cacheService)
    {
        _innerService = innerService;
        _cacheService = cacheService;
    }

    public async Task<byte[]?> GenerateVideoThumbnailAsync(string filePath, TimeSpan? timestamp = null)
    {
        var cacheKey = GenerateCacheKey("thumbnail", filePath, timestamp?.ToString());
        var cached = await _cacheService.GetAsync<byte[]>(cacheKey);
        
        if (cached != null)
        {
            return cached;
        }

        var result = await _innerService.GenerateVideoThumbnailAsync(filePath, timestamp);
        if (result != null)
        {
            await _cacheService.SetAsync(cacheKey, result, _cacheDuration);
        }

        return result;
    }

    public async Task<byte[]?> GenerateAudioWaveformAsync(string filePath, int width = 400, int height = 100)
    {
        var cacheKey = GenerateCacheKey("waveform", filePath, width.ToString(), height.ToString());
        var cached = await _cacheService.GetAsync<byte[]>(cacheKey);
        
        if (cached != null)
        {
            return cached;
        }

        var result = await _innerService.GenerateAudioWaveformAsync(filePath, width, height);
        if (result != null)
        {
            await _cacheService.SetAsync(cacheKey, result, _cacheDuration);
        }

        return result;
    }

    public async Task<byte[]?> GenerateImagePreviewAsync(string filePath, int maxWidth = 800, int maxHeight = 800)
    {
        var cacheKey = GenerateCacheKey("preview", filePath, maxWidth.ToString(), maxHeight.ToString());
        var cached = await _cacheService.GetAsync<byte[]>(cacheKey);
        
        if (cached != null)
        {
            return cached;
        }

        var result = await _innerService.GenerateImagePreviewAsync(filePath, maxWidth, maxHeight);
        if (result != null)
        {
            await _cacheService.SetAsync(cacheKey, result, _cacheDuration);
        }

        return result;
    }

    public async Task<string> ExtractAudioFromVideoAsync(string videoPath, string outputPath)
    {
        return await _innerService.ExtractAudioFromVideoAsync(videoPath, outputPath);
    }

    public async Task<string> TranscodeVideoAsync(string inputPath, string outputPath, string codec = "h264", int quality = 80)
    {
        return await _innerService.TranscodeVideoAsync(inputPath, outputPath, codec, quality);
    }

    public async Task<byte[]?> GenerateBlurHashAsync(string filePath, int componentsX = 4, int componentsY = 3)
    {
        var cacheKey = GenerateCacheKey("blurhash", filePath, componentsX.ToString(), componentsY.ToString());
        var cached = await _cacheService.GetAsync<byte[]>(cacheKey);
        
        if (cached != null)
        {
            return cached;
        }

        var result = await _innerService.GenerateBlurHashAsync(filePath, componentsX, componentsY);
        if (result != null)
        {
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromHours(1));
        }

        return result;
    }

    private string GenerateCacheKey(params string[] parts)
    {
        return string.Join(":", parts.Select(p => p?.GetHashCode().ToString() ?? "null"));
    }
}
