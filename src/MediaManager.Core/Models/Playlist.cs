namespace MediaManager.Core.Models;

/// <summary>播放列表</summary>
public class Playlist
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<PlaylistItem> Items { get; set; } = [];
}
