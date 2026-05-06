using MediaManager.Core.Interfaces.Services;
using MediaManager.Services.Media;
using Moq;
using Xunit;

namespace MediaManager.Tests.Services;

/// <summary>
/// 媒体增强服务单元测试
/// </summary>
public class MediaEnhancementServiceTests
{
    private readonly Mock<IThumbnailService> _mockThumbnailService;
    private readonly Mock<IWaveformService> _mockWaveformService;
    private readonly Mock<IImageThumbnailService> _mockImageThumbnailService;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly MediaEnhancementService _service;

    public MediaEnhancementServiceTests()
    {
        _mockThumbnailService = new Mock<IThumbnailService>();
        _mockWaveformService = new Mock<IWaveformService>();
        _mockImageThumbnailService = new Mock<IImageThumbnailService>();
        _mockCacheService = new Mock<ICacheService>();
        
        _service = new MediaEnhancementService(
            _mockThumbnailService.Object,
            _mockWaveformService.Object,
            _mockImageThumbnailService.Object,
            _mockCacheService.Object
        );
    }

    [Fact]
    public async Task GenerateVideoThumbnailAsync_WhenCacheHit_ShouldReturnCachedValue()
    {
        // Arrange
        var filePath = "test.mp4";
        var cachedBytes = new byte[] { 1, 2, 3 };
        _mockCacheService
            .Setup(x => x.ExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);
        _mockCacheService
            .Setup(x => x.GetAsync<byte[]>(It.IsAny<string>()))
            .ReturnsAsync(cachedBytes);

        // Act
        var result = await _service.GenerateVideoThumbnailAsync(filePath);

        // Assert
        Assert.Equal(cachedBytes, result);
        _mockThumbnailService.Verify(x => x.GenerateAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GenerateAudioWaveformAsync_WhenCacheHit_ShouldReturnCachedValue()
    {
        // Arrange
        var filePath = "test.mp3";
        var cachedBytes = new byte[] { 4, 5, 6 };
        _mockCacheService
            .Setup(x => x.ExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);
        _mockCacheService
            .Setup(x => x.GetAsync<byte[]>(It.IsAny<string>()))
            .ReturnsAsync(cachedBytes);

        // Act
        var result = await _service.GenerateAudioWaveformAsync(filePath);

        // Assert
        Assert.Equal(cachedBytes, result);
        _mockWaveformService.Verify(x => x.GenerateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GenerateBlurHashAsync_WhenCacheHit_ShouldReturnCachedValue()
    {
        // Arrange
        var filePath = "test.jpg";
        var cachedHash = "LKO2U~V%2Tw=w]~RBVZRi};RPxuw";
        _mockCacheService
            .Setup(x => x.ExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);
        _mockCacheService
            .Setup(x => x.GetAsync<string>(It.IsAny<string>()))
            .ReturnsAsync(cachedHash);

        // Act
        var result = await _service.GenerateBlurHashAsync(filePath);

        // Assert
        Assert.Equal(cachedHash, result);
    }

    [Fact]
    public async Task GenerateVideoThumbnailAsync_WhenFileNotExists_ShouldReturnNull()
    {
        // Arrange
        var filePath = "non_existing.mp4";
        _mockCacheService
            .Setup(x => x.ExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.GenerateVideoThumbnailAsync(filePath);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GenerateAudioWaveformAsync_WithCustomDimensions_ShouldPassCorrectParameters()
    {
        // Arrange
        var filePath = "test.mp3";
        var width = 800;
        var height = 200;
        
        _mockCacheService
            .Setup(x => x.ExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act - Will fail because file doesn't exist, but we can verify the call
        await _service.GenerateAudioWaveformAsync(filePath, width, height);

        // Assert - Verify the service was called with correct parameters
        _mockWaveformService.Verify(
            x => x.GenerateAsync(
                filePath,
                It.IsAny<string>(),
                width,
                height
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task GetOrCreateThumbnailAsync_WhenCacheExists_ShouldReturnCachedPath()
    {
        // Arrange
        var filePath = "test.mp4";
        var cacheDirectory = "cache";
        var expectedCachePath = Path.Combine(cacheDirectory, "test_thumb.jpg");
        
        // Create a temporary file to simulate cached thumbnail
        Directory.CreateDirectory(cacheDirectory);
        await File.WriteAllTextAsync(expectedCachePath, "test");

        try
        {
            // Act
            var result = await _service.GetOrCreateThumbnailAsync(filePath, cacheDirectory);

            // Assert
            Assert.Equal(expectedCachePath, result);
        }
        finally
        {
            // Cleanup
            if (File.Exists(expectedCachePath))
                File.Delete(expectedCachePath);
            if (Directory.Exists(cacheDirectory))
                Directory.Delete(cacheDirectory);
        }
    }
}
