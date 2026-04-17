using MediaManager.Core.Interfaces.Services;
using MediaManager.Core.Models;
using System.Diagnostics;
using System.IO;

namespace MediaManager.Services.Library;

/// <summary>
/// 媒体播放服务。
/// 调用系统默认程序打开文件，或在资源管理器中定位文件。
/// </summary>
public class PlaybackService : IPlaybackService
{
    public void OpenWithDefault(MediaFile file)
    {
        if (string.IsNullOrEmpty(file.Path) || !File.Exists(file.Path))
        {
            throw new FileNotFoundException($"文件不存在: {file.Path}");
        }

        // 使用系统默认程序打开文件
        Process.Start(new ProcessStartInfo
        {
            FileName = file.Path,
            UseShellExecute = true
        });
    }

    public void RevealInExplorer(MediaFile file)
    {
        if (string.IsNullOrEmpty(file.Path) || !File.Exists(file.Path))
        {
            throw new FileNotFoundException($"文件不存在: {file.Path}");
        }

        // 在资源管理器中定位文件
        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"/select,\"{file.Path}\"",
            UseShellExecute = true
        });
    }
}