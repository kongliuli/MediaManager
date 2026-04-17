using MediaManager.Core.Interfaces.Services;
using MediaManager.Data.Context;
using MediaManager.Services.Library;
using MediaManager.Services.Media;
using MediaManager.Services.Metadata;
using MediaManager.Services.Scanning;
using Microsoft.Extensions.DependencyInjection;

namespace MediaManager.Services.Extensions;

/// <summary>
/// Services 层注册扩展。
/// 在 App.xaml.cs 中调用 services.AddMediaServices() 完成注册。
/// </summary>
public static class ServicesExtensions
{
    public static IServiceCollection AddMediaServices(this IServiceCollection services)
    {
        // 扫描
        services.AddSingleton<IFileScannerService, FileScannerService>();
        services.AddTransient<ScanPipelineOrchestrator>();

        // 元数据
        services.AddSingleton<IMetadataService, FfprobeMetadataService>();

        // 媒体处理
        services.AddSingleton<IHashService, HashService>();
        services.AddSingleton<IThumbnailService, ThumbnailService>();
        services.AddSingleton<IWaveformService, WaveformService>();

        // 媒体库
        services.AddScoped<IDuplicateDetectionService, DuplicateDetectionService>();
        services.AddScoped<ISearchService, SearchService>();
        services.AddSingleton<IPlaybackService, PlaybackService>();

        return services;
    }
}
