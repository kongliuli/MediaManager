namespace MediaManager.Core.Models;

/// <summary>
/// 文档文件扩展元数据
/// </summary>
public class DocumentMetadata
{
    /// <summary>
    /// 标题
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 作者
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// 主题
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// 关键词
    /// </summary>
    public string? Keywords { get; set; }

    /// <summary>
    /// 评论/说明
    /// </summary>
    public string? Comments { get; set; }

    /// <summary>
    /// 创建应用程序
    /// </summary>
    public string? CreatorApplication { get; set; }

    /// <summary>
    /// 页码数
    /// </summary>
    public int? PageCount { get; set; }

    /// <summary>
    /// 字数
    /// </summary>
    public int? WordCount { get; set; }

    /// <summary>
    /// 字符数
    /// </summary>
    public int? CharacterCount { get; set; }

    /// <summary>
    /// 行数
    /// </summary>
    public int? LineCount { get; set; }

    /// <summary>
    /// 语言
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// 创建日期
    /// </summary>
    public DateTime? CreatedDate { get; set; }

    /// <summary>
    /// 最后修改日期
    /// </summary>
    public DateTime? ModifiedDate { get; set; }

    /// <summary>
    /// 最后打印日期
    /// </summary>
    public DateTime? LastPrintedDate { get; set; }

    /// <summary>
    /// 公司/组织
    /// </summary>
    public string? Company { get; set; }

    /// <summary>
    /// 版本号
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// 模板名称
    /// </summary>
    public string? TemplateName { get; set; }
}
