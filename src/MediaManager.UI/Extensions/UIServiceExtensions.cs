using MediaManager.UI.ViewModels;
using MediaManager.UI.Views;
using Microsoft.Extensions.DependencyInjection;

namespace MediaManager.UI.Extensions;

/// <summary>
/// UI 层服务注册扩展。
/// 注册所有 ViewModel 和 View（Transient，每次导航创建新实例）。
/// </summary>
public static class UIServiceExtensions
{
    public static IServiceCollection AddUIServices(this IServiceCollection services)
    {
        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<DetailViewModel>();   // MainViewModel 持有引用，必须 Singleton
        services.AddTransient<LibraryViewModel>();
        services.AddSingleton<ScanViewModel>();
        services.AddTransient<SearchViewModel>();
        services.AddTransient<PlaylistViewModel>();
        services.AddTransient<DuplicateViewModel>();
        services.AddTransient<CacheViewModel>();
        services.AddTransient<PerformanceViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<MediaLibraryViewModel>();

        // Views
        services.AddTransient<MainWindow>();
        services.AddTransient<LibraryView>();
        services.AddTransient<ScanView>();
        services.AddTransient<SearchView>();
        services.AddTransient<DetailView>();
        services.AddTransient<PlaylistView>();
        services.AddTransient<DuplicateView>();
        services.AddTransient<CacheView>();
        services.AddTransient<PerformanceView>();
        services.AddTransient<SettingsView>();
        services.AddTransient<MediaLibraryView>();

        return services;
    }
}
