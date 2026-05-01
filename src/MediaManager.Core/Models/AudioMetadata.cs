namespace MediaManager.Core.Models;

/// <summary>
/// 音频文件扩展元数据
/// </summary>
public class AudioMetadata
{
    /// <summary>
    /// ISRC 编码（国际标准录音代码）
    /// </summary>
    public string? ISRC { get; set; }

    /// <summary>
    /// 唱片公司
    /// </summary>
    public string? RecordLabel { get; set; }

    /// <summary>
    /// 版权信息
    /// </summary>
    public string? Copyright { get; set; }

    /// <summary>
    /// 作曲者
    /// </summary>
    public string? Composer { get; set; }

    /// <summary>
    /// 编曲者
    /// </summary>
    public string? Arranger { get; set; }

    /// <summary>
    /// 表演者
    /// </summary>
    public string? Performer { get; set; }

    /// <summary>
    /// 碟片编号
    /// </summary>
    public int? DiscNumber { get; set; }

    /// <summary>
    /// 总碟片数
    /// </summary>
    public int? TotalDiscs { get; set; }

    /// <summary>
    /// BPM（每分钟节拍数）
    /// </summary>
    public double? BPM { get; set; }

    /// <summary>
    /// 调式（如 C 大调、A 小调）
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// 语言
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// 歌词
    /// </summary>
    public string? Lyrics { get; set; }

    /// <summary>
    /// 评论
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// 编码工具/编码器
    /// </summary>
    public string? Encoder { get; set; }

    /// <summary>
    /// 原始发布日期
    /// </summary>
    public DateTime? OriginalReleaseDate { get; set; }
}
