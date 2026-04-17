using MediaManager.Data.Extensions;
using MediaManager.Services.Extensions;
using MediaManager.UI.Extensions;
using MediaManager.UI.ViewModels;
using MediaManager.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.IO;
using System.Windows;

namespace MediaManager.UI;

/// <summary>
/// 应用程序入口。
/// 构建 DI Host，注册所有服务、仓储和 ViewModel，然后启动主窗口。
/// </summary>
public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var appDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MediaManager");
        Directory.CreateDirectory(appDataDir);

        // 配置 Serilog
        var logPath = Path.Combine(appDataDir, "logs");
        Directory.CreateDirectory(logPath);
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(Path.Combine(logPath, "log.txt"), rollingInterval: RollingInterval.Day)
            .CreateLogger();

        var dbPath = Path.Combine(appDataDir, "media.db");
        var thumbnailDir = Path.Combine(appDataDir, "thumbnails");
        Directory.CreateDirectory(thumbnailDir);

        _host = Host.CreateDefaultBuilder()
            .UseSerilog() // 使用 Serilog
            .ConfigureServices(services =>
            {
                services.AddDataServices(dbPath);
                services.AddMediaServices();
                services.AddUIServices();

                // 注入应用级配置
                services.AddSingleton(new AppConfig { ThumbnailDirectory = thumbnailDir });
            })
            .Build();

        try
        {
            await _host.Services.EnsureDatabaseMigratedAsync();
            await _host.StartAsync();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "应用程序启动失败");
            throw;
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        
        // 关闭 Serilog 日志记录器
        Log.CloseAndFlush();
        
        base.OnExit(e);
    }
}

/// <summary>应用级配置，通过 DI 注入到需要的服务中</summary>
public class AppConfig
{
    public string ThumbnailDirectory { get; set; } = string.Empty;
}
