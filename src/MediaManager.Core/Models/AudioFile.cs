using MediaManager.Core.Enums;

namespace MediaManager.Core.Models;

/// <summary>音频文件，继承自 MediaFile</summary>
public class AudioFile : MediaFile
{
    public int BitRate { get; set; }
    public int SampleRate { get; set; }
    public int Channels { get; set; }
    public string Codec { get; set; } = string.Empty;

    // ID3 标签
    public string? Title { get; set; }
    public string? Artist { get; set; }
    public string? Album { get; set; }
    public int? Year { get; set; }
    public string? Genre { get; set; }
    public int? TrackNumber { get; set; }

    /// <summary>内嵌封面图缓存路径</summary>
    public string? AlbumArtPath { get; set; }

    /// <summary>波形图缓存路径</summary>
    public string? WaveformImagePath { get; set; }
}
