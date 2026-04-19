namespace MediaManager.Core.Interfaces.Services;

/// <summary>
/// 音频波形图生成服务接口。
/// 将音频采样数据渲染为波形图 PNG。
/// </summary>
public interface IWaveformService
{
    Task GenerateAsync(string audioPath, string outputPath, int width = 800, int height = 100, CancellationToken ct = default);
}
