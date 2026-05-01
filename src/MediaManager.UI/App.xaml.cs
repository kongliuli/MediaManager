using MediaManager.Data.Extensions;
using MediaManager.Services.Extensions;
using MediaManager.UI.Extensions;
using MediaManager.UI.ViewModels;
using MediaManager.UI.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.IO;
using System.Windows;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

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

        // 配置 Serilog - 多级别日志记录
        var appDataLogPath = Path.Combine(appDataDir, "logs");
        Directory.CreateDirectory(appDataLogPath);
        
        // 获取应用程序运行目录（bin目录）
        var binLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        Directory.CreateDirectory(binLogPath);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            // 所有级别日志到 AppData
            .WriteTo.File(Path.Combine(appDataLogPath, "log.txt"), rollingInterval: RollingInterval.Day)
            // 信息级别及以上到 bin/logs/info.txt
            .WriteTo.File(Path.Combine(binLogPath, "info.txt"), 
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                rollingInterval: RollingInterval.Day)
            // 警告级别及以上到 bin/logs/warn.txt
            .WriteTo.File(Path.Combine(binLogPath, "warn.txt"), 
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning,
                rollingInterval: RollingInterval.Day)
            // 错误级别及以上到 bin/logs/error.txt
            .WriteTo.File(Path.Combine(binLogPath, "error.txt"), 
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error,
                rollingInterval: RollingInterval.Day)
            .CreateLogger();

        // 获取应用程序实际运行目录（使用程序集位置）
        var appDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) 
                     ?? AppDomain.CurrentDomain.BaseDirectory;
        
        // 从配置文件读取数据库路径
        var config = new ConfigurationBuilder()
            .SetBasePath(appDir)
            .AddJsonFile("appsettings.json")
            .Build();
        
        // 尝试多种方式确定数据库路径
        var dbPath = string.Empty;
        var dbRelativePath = config["DatabasePath"] ?? "../../../../../src/media.db";
        
        // 方法1：从配置文件解析路径
        var resolvedPath = Path.GetFullPath(Path.Combine(appDir, dbRelativePath));
        if (File.Exists(resolvedPath))
        {
            dbPath = resolvedPath;
        }
        // 方法2：从项目根目录定位
        else
        {
            var projectRoot = Path.GetFullPath(Path.Combine(appDir, "../../../../../"));
            var projectDbPath = Path.Combine(projectRoot, "src", "media.db");
            if (File.Exists(projectDbPath))
            {
                dbPath = projectDbPath;
            }
        }
        
        // 如果仍未找到，使用固定路径（调试用）
        if (string.IsNullOrEmpty(dbPath))
        {
            dbPath = @"D:\Code\VibeWorkspace\worktrees\MediaManager_260417\src\media.db";
        }
        
        // 确保目录存在
        var dbDirectory = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory))
        {
            Directory.CreateDirectory(dbDirectory);
        }
        
        // 调试日志：输出数据库路径
        Log.Information("数据库路径配置:");
        Log.Information("  AppDir: {AppDir}", appDir);
        Log.Information("  相对路径: {RelativePath}", dbRelativePath);
        Log.Information("  解析路径: {ResolvedPath}", resolvedPath);
        Log.Information("  最终路径: {FinalPath}", dbPath);
        Log.Information("  文件存在: {Exists}", File.Exists(dbPath));
        
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
                services.AddSingleton(new MediaManager.Core.AppConfig 
                { 
                    ThumbnailDirectory = thumbnailDir,
                    FfmpegPath = Path.GetFullPath(Path.Combine(appDir, config["FfmpegPath"] ?? "")),
                    FfprobePath = Path.GetFullPath(Path.Combine(appDir, config["FfprobePath"] ?? ""))
                });
            })
            .Build();

        try
        {
            await _host.Services.EnsureDatabaseMigratedAsync();
            await _host.StartAsync();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            MainWindow = mainWindow;   // 让 WPF 知道主窗口是谁
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "应用程序启动失败");
            MessageBox.Show($"启动失败：{ex.Message}\n\n{ex.InnerException?.Message}",
                "MediaManager", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
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


