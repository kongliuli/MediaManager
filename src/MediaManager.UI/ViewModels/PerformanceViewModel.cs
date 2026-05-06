using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaManager.Core.Interfaces.Services;
using System.Diagnostics;

namespace MediaManager.UI.ViewModels;

/// <summary>
/// 性能监控视图模型
/// </summary>
public partial class PerformanceViewModel : ObservableObject
{
    private readonly ICacheService _cacheService;
    private readonly IMediaEnhancementService _enhancementService;
    private System.Timers.Timer? _monitoringTimer;

    [ObservableProperty]
    private double _cpuUsage;

    [ObservableProperty]
    private double _memoryUsageMB;

    [ObservableProperty]
    private double _availableMemoryMB;

    [ObservableProperty]
    private int _processCount;

    [ObservableProperty]
    private int _threadCount;

    [ObservableProperty]
    private TimeSpan _uptime;

    [ObservableProperty]
    private int _cacheItemCount;

    [ObservableProperty]
    private double _cacheMemoryMB;

    [ObservableProperty]
    private double _cacheHitRate;

    [ObservableProperty]
    private long _cacheHits;

    [ObservableProperty]
    private long _cacheMisses;

    [ObservableProperty]
    private bool _isMonitoring;

    [ObservableProperty]
    private string _monitoringStatus = "未启动";

    private readonly Process _currentProcess;
    private readonly DateTime _startTime;

    public PerformanceViewModel(ICacheService cacheService, IMediaEnhancementService enhancementService)
    {
        _cacheService = cacheService;
        _enhancementService = enhancementService;
        _currentProcess = Process.GetCurrentProcess();
        _startTime = DateTime.Now;
    }

    [RelayCommand]
    private void StartMonitoring()
    {
        if (IsMonitoring) return;

        IsMonitoring = true;
        MonitoringStatus = "监控中...";

        _monitoringTimer = new System.Timers.Timer(1000); // 每秒更新
        _monitoringTimer.Elapsed += async (s, e) => await UpdateMetricsAsync();
        _monitoringTimer.Start();

        UpdateMetricsAsync().Wait();
    }

    [RelayCommand]
    private void StopMonitoring()
    {
        if (!IsMonitoring) return;

        IsMonitoring = false;
        MonitoringStatus = "已停止";

        _monitoringTimer?.Stop();
        _monitoringTimer?.Dispose();
        _monitoringTimer = null;
    }

    private async Task UpdateMetricsAsync()
    {
        try
        {
            // 更新进程指标
            _currentProcess.Refresh();
            
            MemoryUsageMB = _currentProcess.WorkingSet64 / (1024.0 * 1024.0);
            ThreadCount = _currentProcess.Threads.Count;
            ProcessCount = Process.GetProcesses().Length;
            Uptime = DateTime.Now - _startTime;

            // 更新缓存指标
            var cacheStats = await _cacheService.GetStatisticsAsync();
            CacheItemCount = cacheStats.ItemCount;
            CacheMemoryMB = cacheStats.MemoryUsage / (1024.0 * 1024.0);
            CacheHitRate = cacheStats.HitRate * 100;
            CacheHits = cacheStats.HitCount;
            CacheMisses = cacheStats.MissCount;

            // CPU 使用率（简化计算）
            CpuUsage = GetCpuUsage();

            // 可用内存
            var memCounter = new PerformanceCounter("Memory", "Available MBytes");
            AvailableMemoryMB = memCounter.NextValue();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"更新性能指标失败: {ex.Message}");
        }
    }

    private double _lastCpuTime;
    private DateTime _lastCheckTime = DateTime.MinValue;

    private double GetCpuUsage()
    {
        try
        {
            var currentTime = DateTime.Now;
            var currentCpuTime = _currentProcess.TotalProcessorTime.TotalMilliseconds;

            if (_lastCheckTime != DateTime.MinValue)
            {
                var cpuUsage = (currentCpuTime - _lastCpuTime) / 
                              (currentTime - _lastCheckTime).TotalMilliseconds * 100;
                _lastCpuTime = currentCpuTime;
                _lastCheckTime = currentTime;
                return Math.Min(cpuUsage, 100);
            }

            _lastCpuTime = currentCpuTime;
            _lastCheckTime = currentTime;
            return 0;
        }
        catch
        {
            return 0;
        }
    }

    [RelayCommand]
    private async Task ClearCacheAsync()
    {
        await _cacheService.ClearAsync();
        await UpdateMetricsAsync();
    }

    [RelayCommand]
    private async Task ForceGarbageCollectionAsync()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        await UpdateMetricsAsync();
    }
}
