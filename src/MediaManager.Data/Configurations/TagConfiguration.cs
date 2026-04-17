using MediaManager.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediaManager.Data.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Color).HasMaxLength(20).HasDefaultValue("#607D8B");
        builder.HasIndex(t => t.Name).IsUnique();
    }
}

public class MediaTagConfiguration : IEntityTypeConfiguration<MediaTag>
{
    public void Configure(EntityTypeBuilder<MediaTag> builder)
    {
        builder.ToTable("MediaTags");
        builder.HasKey(mt => new { mt.MediaFileId, mt.TagId });

        builder.HasOne(mt => mt.MediaFile)
               .WithMany(m => m.MediaTags)
               .HasForeignKey(mt => mt.MediaFileId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mt => mt.Tag)
               .WithMany(t => t.MediaTags)
               .HasForeignKey(mt => mt.TagId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
