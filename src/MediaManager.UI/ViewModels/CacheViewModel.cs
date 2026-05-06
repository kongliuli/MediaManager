using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaManager.Core.Interfaces.Services;
using System.Collections.ObjectModel;

namespace MediaManager.UI.ViewModels;

/// <summary>
/// 缓存管理视图模型
/// </summary>
public partial class CacheViewModel : ObservableObject
{
    private readonly ICacheService _cacheService;

    [ObservableProperty]
    private int _itemCount;

    [ObservableProperty]
    private long _memoryUsage;

    [ObservableProperty]
    private double _memoryUsageMB;

    [ObservableProperty]
    private long _hitCount;

    [ObservableProperty]
    private long _missCount;

    [ObservableProperty]
    private double _hitRate;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "就绪";

    public ObservableCollection<CacheItemDisplay> CacheItems { get; } = new();

    public CacheViewModel(ICacheService cacheService)
    {
        _cacheService = cacheService;
        
        // 初始化时加载数据
        _ = LoadInitialDataAsync();
    }

    private async Task LoadInitialDataAsync()
    {
        await RefreshAsync();
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsLoading = true;
        StatusMessage = "正在加载缓存统计...";

        try
        {
            var stats = await _cacheService.GetStatisticsAsync();
            
            ItemCount = stats.ItemCount;
            MemoryUsage = stats.MemoryUsage;
            MemoryUsageMB = stats.MemoryUsage / (1024.0 * 1024.0);
            HitCount = stats.HitCount;
            MissCount = stats.MissCount;
            HitRate = stats.HitRate * 100;

            StatusMessage = $"缓存统计已更新 - {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"加载失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ClearCacheAsync()
    {
        IsLoading = true;
        StatusMessage = "正在清空缓存...";

        try
        {
            await _cacheService.ClearAsync();
            await RefreshAsync();
            StatusMessage = "缓存已清空";
        }
        catch (Exception ex)
        {
            StatusMessage = $"清空失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

/// <summary>
/// 缓存项显示模型
/// </summary>
public class CacheItemDisplay
{
    public string Key { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
