namespace MediaManager.Core.Models;

/// <summary>标签</summary>
public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>标签颜色（十六进制，如 "#7C3AED"）</summary>
    public string Color { get; set; } = "#607D8B";

    public ICollection<MediaTag> MediaTags { get; set; } = [];
}
