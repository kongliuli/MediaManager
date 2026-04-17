using MediaManager.Core.Models;

namespace MediaManager.Core.Interfaces.Services;

/// <summary>
/// 媒体播放服务接口。
/// 调用系统默认程序打开文件，或在资源管理器中定位文件。
/// </summary>
public interface IPlaybackService
{
    void OpenWithDefault(MediaFile file);
    void RevealInExplorer(MediaFile file);
}
