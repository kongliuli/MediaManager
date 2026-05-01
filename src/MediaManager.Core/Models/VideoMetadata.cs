namespace MediaManager.Core.Models;

/// <summary>
/// 视频文件扩展元数据
/// </summary>
public class VideoMetadata
{
    /// <summary>
    /// 导演
    /// </summary>
    public string? Director { get; set; }

    /// <summary>
    /// 编剧
    /// </summary>
    public string? Writer { get; set; }

    /// <summary>
    /// 演员
    /// </summary>
    public string? Actors { get; set; }

    /// <summary>
    /// 剧情简介
    /// </summary>
    public string? Plot { get; set; }

    /// <summary>
    /// 季号（电视剧）
    /// </summary>
    public int? SeasonNumber { get; set; }

    /// <summary>
    /// 集号（电视剧）
    /// </summary>
    public int? EpisodeNumber { get; set; }

    /// <summary>
    /// 集名
    /// </summary>
    public string? EpisodeName { get; set; }

    /// <summary>
    /// 分级（如 PG-13）
    /// </summary>
    public string? Rating { get; set; }

    /// <summary>
    /// 奖项
    /// </summary>
    public string? Awards { get; set; }

    /// <summary>
    /// 国家/地区
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// 语言
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// 字幕语言
    /// </summary>
    public string? SubtitleLanguages { get; set; }

    /// <summary>
    /// 音频轨道数
    /// </summary>
    public int? AudioTrackCount { get; set; }

    /// <summary>
    /// 容器格式
    /// </summary>
    public string? ContainerFormat { get; set; }

    /// <summary>
    /// 原始发布日期
    /// </summary>
    public DateTime? OriginalReleaseDate { get; set; }
}
