namespace MediaManager.Core.Models;

/// <summary>重复文件组（同一 SHA256 哈希的文件集合）</summary>
public class DuplicateGroup
{
    public string GroupHash { get; set; } = string.Empty;
    public List<MediaFile> Files { get; set; } = [];

    /// <summary>可释放空间（字节）= 文件大小 × (文件数 - 1)</summary>
    public long ReclaimableSize => Files.Count > 1
        ? Files[0].FileSize * (Files.Count - 1)
        : 0;
}
