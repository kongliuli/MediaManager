using MediaManager.Core;
using MediaManager.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;

namespace MediaManager.Services.Media;

/// <summary>
/// FFmpeg 定位服务。
/// 负责查找系统中的 ffmpeg 和 ffprobe 可执行文件。
/// 查找顺序：配置路径 → 应用程序目录 → 系统 PATH。
/// </summary>
public class FfmpegLocator : IFfmpegLocator
{
    private readonly ILogger<FfmpegLocator> _logger;
    private readonly AppConfig _config;

    public string? FfmpegPath { get; private set; }
    public string? FfprobePath { get; private set; }
    public bool IsFfmpegAvailable => !string.IsNullOrEmpty(FfmpegPath);
    public bool IsFfprobeAvailable => !string.IsNullOrEmpty(FfprobePath);

    public FfmpegLocator(ILogger<FfmpegLocator> logger, AppConfig config)
    {
        _logger = logger;
        _config = config;
        Locate();
    }

    private void Locate()
    {
        FfmpegPath = FindExecutable("ffmpeg", _config.FfmpegPath);
        FfprobePath = FindExecutable("ffprobe", _config.FfprobePath);

        _logger.LogInformation("FFmpeg 定位结果:");
        _logger.LogInformation("  ffmpeg: {FfmpegStatus}", IsFfmpegAvailable ? FfmpegPath : "未找到");
        _logger.LogInformation("  ffprobe: {FfprobeStatus}", IsFfprobeAvailable ? FfprobePath : "未找到");

        if (!IsFfmpegAvailable)
        {
            _logger.LogWarning("FFmpeg 不可用，部分功能（如视频缩略图生成）将受限");
        }
        if (!IsFfprobeAvailable)
        {
            _logger.LogWarning("FFprobe 不可用，媒体元数据提取功能将受限");
        }
    }

    private string? FindExecutable(string name, string? configuredPath)
    {
        // 1. 优先使用配置的路径
        if (!string.IsNullOrWhiteSpace(configuredPath) && File.Exists(configuredPath))
        {
            return configuredPath;
        }

        // 2. 检查应用程序目录
        var appDir = AppDomain.CurrentDomain.BaseDirectory;
        var appPath = Path.Combine(appDir, $"{name}.exe");
        if (File.Exists(appPath))
        {
            return appPath;
        }

        // 3. 检查 tools/ffmpeg 目录
        var toolsPath = Path.Combine(appDir, "..", "..", "..", "..", "tools", "ffmpeg", $"{name}.exe");
        var resolvedToolsPath = Path.GetFullPath(toolsPath);
        if (File.Exists(resolvedToolsPath))
        {
            return resolvedToolsPath;
        }

        // 4. 在系统 PATH 中查找
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "where.exe",
                Arguments = name,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processStartInfo);
            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                {
                    var paths = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var path in paths)
                    {
                        var trimmedPath = path.Trim();
                        if (File.Exists(trimmedPath))
                        {
                            return trimmedPath;
                        }
                    }
                }
            }
        }
        catch
        {
            // 静默忽略
        }

        return null;
    }
}

/// <summary>
/// FFmpeg 定位服务接口
/// </summary>
public interface IFfmpegLocator
{
    string? FfmpegPath { get; }
    string? FfprobePath { get; }
    bool IsFfmpegAvailable { get; }
    bool IsFfprobeAvailable { get; }
}


