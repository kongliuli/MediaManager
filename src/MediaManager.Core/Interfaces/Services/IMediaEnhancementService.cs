namespace MediaManager.Core.Interfaces.Services;

public interface IMediaEnhancementService
{
    Task<byte[]?> GenerateVideoThumbnailAsync(string filePath, TimeSpan? timestamp = null);
    
    Task<byte[]?> GenerateAudioWaveformAsync(string filePath, int width = 400, int height = 100);
    
    Task<byte[]?> GenerateImagePreviewAsync(string filePath, int maxWidth = 800, int maxHeight = 800);
    
    Task<string> ExtractAudioFromVideoAsync(string videoPath, string outputPath);
    
    Task<string> TranscodeVideoAsync(string inputPath, string outputPath, string codec = "h264", int quality = 80);
    
    Task<byte[]?> GenerateBlurHashAsync(string filePath, int componentsX = 4, int componentsY = 3);
}
