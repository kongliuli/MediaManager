namespace MediaManager.Core.Interfaces.Services;

/// <summary>
/// 缓存服务接口 - 提供内存缓存功能
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// 获取缓存值
    /// </summary>
    Task<T?> GetAsync<T>(string key);
    
    /// <summary>
    /// 设置缓存值
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    
    /// <summary>
    /// 尝试获取缓存值
    /// </summary>
    Task<bool> TryGetValueAsync<T>(string key, out T? value);
    
    /// <summary>
    /// 移除缓存项
    /// </summary>
    Task RemoveAsync(string key);
    
    /// <summary>
    /// 检查缓存是否存在
    /// </summary>
    Task<bool> ExistsAsync(string key);
    
    /// <summary>
    /// 清空所有缓存
    /// </summary>
    Task ClearAsync();
    
    /// <summary>
    /// 获取内存使用量（字节）
    /// </summary>
    Task<long> GetMemoryUsageAsync();
    
    /// <summary>
    /// 获取缓存项数量
    /// </summary>
    Task<int> GetItemCountAsync();
    
    /// <summary>
    /// 获取缓存统计信息
    /// </summary>
    Task<CacheStatistics> GetStatisticsAsync();
}

/// <summary>
/// 缓存统计信息
/// </summary>
public class CacheStatistics
{
    public int ItemCount { get; set; }
    public long MemoryUsage { get; set; }
    public long HitCount { get; set; }
    public long MissCount { get; set; }
    public double HitRate => HitCount + MissCount > 0 ? (double)HitCount / (HitCount + MissCount) : 0;
}
