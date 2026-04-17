using MediaManager.Core.Interfaces.Services;
using System;
using System.Diagnostics;
using System.IO;

namespace MediaManager.Services.Media;

/// <summary>
/// 视频缩略图生成服务。
/// 使用 ffmpeg 从视频中提取指定时间点的帧，保存为 PNG 图像。
/// </summary>
public class ThumbnailService : IThumbnailService
{
    public async Task GenerateAsync(string videoPath, string outputPath, double? atSecond = null, CancellationToken ct = default)
    {
        // 确保输出目录存在
        var outputDirectory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        // 构建 ffmpeg 命令
        var arguments = new List<string>
        {
            "-i", videoPath,
            "-vframes", "1",
            "-q:v", "2", // 高质量
            "-y" // 覆盖现有文件
        };

        // 如果指定了时间点，添加 -ss 参数
        if (atSecond.HasValue)
        {
            arguments.Insert(1, $"-ss {atSecond.Value}");
        }

        arguments.Add(outputPath);

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = string.Join(" ", arguments),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(processStartInfo);
        if (process == null)
        {
            throw new InvalidOperationException("无法启动 ffmpeg 进程");
        }

        using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cancellationTokenSource.CancelAfter(TimeSpan.FromMinutes(1)); // 1分钟超时

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        var exitTask = Task.Run(() => process.WaitForExit(), cancellationTokenSource.Token);

        var completedTask = await Task.WhenAny(outputTask, errorTask, exitTask);

        if (cancellationTokenSource.Token.IsCancellationRequested)
        {
            process.Kill();
            throw new OperationCanceledException(ct);
        }

        if (process.ExitCode != 0)
        {
            var error = await errorTask;
            throw new Exception($"ffmpeg 执行失败: {error}");
        }

        // 验证输出文件是否生成
        if (!File.Exists(outputPath))
        {
            throw new Exception("缩略图生成失败，输出文件不存在");
        }
    }
}