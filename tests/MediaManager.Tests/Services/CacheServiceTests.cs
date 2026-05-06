using MediaManager.Core.Interfaces.Services;
using MediaManager.Services.Library;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace MediaManager.Tests.Services;

/// <summary>
/// 缓存服务单元测试
/// </summary>
public class CacheServiceTests : IDisposable
{
    private readonly CacheService _cacheService;
    private readonly MemoryCache _memoryCache;

    public CacheServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _cacheService = new CacheService(_memoryCache);
    }

    [Fact]
    public async Task SetAndGetAsync_ShouldReturnCachedValue()
    {
        // Arrange
        var key = "test_key";
        var value = "test_value";

        // Act
        await _cacheService.SetAsync(key, value);
        var result = await _cacheService.GetAsync<string>(key);

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task GetAsync_NonExistingKey_ShouldReturnNull()
    {
        // Arrange
        var key = "non_existing_key";

        // Act
        var result = await _cacheService.GetAsync<string>(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ExistsAsync_ExistingKey_ShouldReturnTrue()
    {
        // Arrange
        var key = "existing_key";
        await _cacheService.SetAsync(key, "value");

        // Act
        var exists = await _cacheService.ExistsAsync(key);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_NonExistingKey_ShouldReturnFalse()
    {
        // Arrange
        var key = "non_existing_key";

        // Act
        var exists = await _cacheService.ExistsAsync(key);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveCachedItem()
    {
        // Arrange
        var key = "key_to_remove";
        await _cacheService.SetAsync(key, "value");

        // Act
        await _cacheService.RemoveAsync(key);
        var exists = await _cacheService.ExistsAsync(key);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task ClearAsync_ShouldRemoveAllItems()
    {
        // Arrange
        await _cacheService.SetAsync("key1", "value1");
        await _cacheService.SetAsync("key2", "value2");
        await _cacheService.SetAsync("key3", "value3");

        // Act
        await _cacheService.ClearAsync();
        var count = await _cacheService.GetItemCountAsync();

        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task GetItemCountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        await _cacheService.SetAsync("key1", "value1");
        await _cacheService.SetAsync("key2", "value2");

        // Act
        var count = await _cacheService.GetItemCountAsync();

        // Assert
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task GetMemoryUsageAsync_ShouldReturnNonNegativeValue()
    {
        // Arrange
        await _cacheService.SetAsync("key", new byte[1024]);

        // Act
        var usage = await _cacheService.GetMemoryUsageAsync();

        // Assert
        Assert.True(usage >= 0);
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldReturnValidStatistics()
    {
        // Arrange
        await _cacheService.SetAsync("key1", "value1");
        await _cacheService.SetAsync("key2", "value2");
        await _cacheService.GetAsync<string>("key1"); // Hit
        await _cacheService.GetAsync<string>("non_existing"); // Miss

        // Act
        var stats = await _cacheService.GetStatisticsAsync();

        // Assert
        Assert.Equal(2, stats.ItemCount);
        Assert.Equal(1, stats.HitCount);
        Assert.Equal(1, stats.MissCount);
        Assert.True(stats.HitRate > 0);
    }

    [Fact]
    public async Task SetAsync_WithExpiration_ShouldExpire()
    {
        // Arrange
        var key = "expiring_key";
        var value = "value";
        var expiration = TimeSpan.FromMilliseconds(100);

        // Act
        await _cacheService.SetAsync(key, value, expiration);
        await Task.Delay(150);
        var exists = await _cacheService.ExistsAsync(key);

        // Assert
        Assert.False(exists);
    }

    public void Dispose()
    {
        _memoryCache?.Dispose();
    }
}
