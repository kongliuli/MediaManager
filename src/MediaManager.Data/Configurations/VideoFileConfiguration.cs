using MediaManager.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediaManager.Data.Configurations;

public class VideoFileConfiguration : IEntityTypeConfiguration<VideoFile>
{
    public void Configure(EntityTypeBuilder<VideoFile> builder)
    {
        builder.Property(v => v.BitRate)
            .HasColumnName("VideoFile_BitRate");

        builder.Property(v => v.ThumbnailPath)
            .HasColumnName("VideoFile_ThumbnailPath");

        builder.Property(v => v.Title)
            .HasColumnName("VideoFile_Title");
    }
}