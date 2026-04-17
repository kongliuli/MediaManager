using MediaManager.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace MediaManager.Data.Context;

/// <summary>
/// EF Core DbContext。
/// 使用 TPH（单表继承）策略存储 AudioFile 和 VideoFile，
/// Discriminator 列为 MediaType 枚举值。
/// 数据库文件路径通过构造函数注入，默认存放在 AppData 目录。
/// </summary>
public class MediaDbContext(DbContextOptions<MediaDbContext> options) : DbContext(options)
{
    public DbSet<MediaFile> MediaFiles => Set<MediaFile>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<MediaTag> MediaTags => Set<MediaTag>();
    public DbSet<Playlist> Playlists => Set<Playlist>();
    public DbSet<PlaylistItem> PlaylistItems => Set<PlaylistItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 应用所有 IEntityTypeConfiguration<T> 配置
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MediaDbContext).Assembly);
    }
}
