namespace MediaManager.Core.Models;

/// <summary>其他文件类型，继承自 MediaFile</summary>
public class OtherFile : MediaFile
{
    /// <summary>文件扩展名（不含点）</summary>
    public string Extension { get; set; } = string.Empty;
}