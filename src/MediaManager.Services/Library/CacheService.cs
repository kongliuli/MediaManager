using MediaManager.Core.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace MediaManager.Services.Library;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ConcurrentDictionary<string, CacheItemInfo> _cacheInfo;
    private readonly long _maxMemoryUsage;
    private long _currentMemoryUsage;

    private class CacheItemInfo
    {
        public long Size { get; set; }
        public DateTime CreatedAt { get; set; }
        public TimeSpan? Expiration { get; set; }
    }

    public CacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
        _cacheInfo = new ConcurrentDictionary<string, CacheItemInfo>();
        _maxMemoryUsage = 256 * 1024 * 1024;
        _currentMemoryUsage = 0;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var result = _memoryCache.Get<T>(key);
        
        if (result != null)
        {
            UpdateAccessTime(key);
        }
        
        await Task.CompletedTask;
        return result;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var size = EstimateSize(value);
        
        if (size > _maxMemoryUsage / 2)
        {
            await Task.CompletedTask;
            return;
        }

        EnsureCapacity(size);

        var cacheEntryOptions = new MemoryCacheEntryOptions();
        
        if (expiration.HasValue)
        {
            cacheEntryOptions.AbsoluteExpirationRelativeToNow = expiration.Value;
        }
        else
        {
            cacheEntryOptions.SlidingExpiration = TimeSpan.FromMinutes(30);
        }

        cacheEntryOptions.RegisterPostEvictionCallback((cacheKey, cacheValue, reason, state) =>
        {
            if (_cacheInfo.TryRemove(cacheKey.ToString(), out var info))
            {
                Interlocked.Add(ref _currentMemoryUsage, -info.Size);
            }
        });

        _memoryCache.Set(key, value, cacheEntryOptions);

        _cacheInfo[key] = new CacheItemInfo
        {
            Size = size,
            CreatedAt = DateTime.UtcNow,
            Expiration = expiration
        };

        Interlocked.Add(ref _currentMemoryUsage, size);
        
        await Task.CompletedTask;
    }

    public async Task<bool> TryGetValueAsync<T>(string key, out T? value)
    {
        var result = _memoryCache.TryGetValue(key, out T? foundValue);
        value = foundValue;
        
        if (result)
        {
            UpdateAccessTime(key);
        }
        
        await Task.CompletedTask;
        return result;
    }

    public async Task RemoveAsync(string key)
    {
        _memoryCache.Remove(key);
        if (_cacheInfo.TryRemove(key, out var info))
        {
            Interlocked.Add(ref _currentMemoryUsage, -info.Size);
        }
        await Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(string key)
    {
        await Task.CompletedTask;
        return _cacheInfo.ContainsKey(key) && _memoryCache.TryGetValue(key, out _);
    }

    public async Task ClearAsync()
    {
        _memoryCache.Dispose();
        _cacheInfo.Clear();
        Interlocked.Exchange(ref _currentMemoryUsage, 0);
        await Task.CompletedTask;
    }

    public async Task<long> GetMemoryUsage()
    {
        await Task.CompletedTask;
        return _currentMemoryUsage;
    }

    public async Task<int> GetItemCount()
    {
        await Task.CompletedTask;
        return _cacheInfo.Count;
    }

    private void EnsureCapacity(long requiredSize)
    {
        while (_currentMemoryUsage + requiredSize > _maxMemoryUsage)
        {
            var oldestKey = _cacheInfo.OrderBy(kv => kv.Value.CreatedAt).FirstOrDefault().Key;
            if (!string.IsNullOrEmpty(oldestKey))
            {
                RemoveAsync(oldestKey).Wait();
            }
            else
            {
                break;
            }
        }
    }

    private long EstimateSize(object? value)
    {
        if (value == null) return 0;
        
        if (value is byte[] bytes)
        {
            return bytes.LongLength;
        }
        
        if (value is string str)
        {
            return str.Length * 2L;
        }
        
        var type = value.GetType();
        if (type.IsPrimitive)
        {
            return type switch
            {
                Type t when t == typeof(int) => 4,
                Type t when t == typeof(long) => 8,
                Type t when t == typeof(bool) => 1,
                _ => 8
            };
        }
        
        return 1024;
    }

    private void UpdateAccessTime(string key)
    {
        if (_cacheInfo.TryGetValue(key, out var info))
        {
            info.CreatedAt = DateTime.UtcNow;
        }
    }
}
