using MediaManager.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediaManager.Data.Configurations;

public class PlaylistConfiguration : IEntityTypeConfiguration<Playlist>
{
    public void Configure(EntityTypeBuilder<Playlist> builder)
    {
        builder.ToTable("Playlists");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
    }
}

public class PlaylistItemConfiguration : IEntityTypeConfiguration<PlaylistItem>
{
    public void Configure(EntityTypeBuilder<PlaylistItem> builder)
    {
        builder.ToTable("PlaylistItems");
        builder.HasKey(pi => pi.Id);

        builder.HasOne(pi => pi.Playlist)
               .WithMany(p => p.Items)
               .HasForeignKey(pi => pi.PlaylistId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pi => pi.MediaFile)
               .WithMany(m => m.PlaylistItems)
               .HasForeignKey(pi => pi.MediaFileId)
               .OnDelete(DeleteBehavior.Cascade);

        // 同一播放列表内排序唯一
        builder.HasIndex(pi => new { pi.PlaylistId, pi.SortOrder });
    }
}
