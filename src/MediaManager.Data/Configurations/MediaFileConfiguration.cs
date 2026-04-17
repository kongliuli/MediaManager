using MediaManager.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediaManager.Data.Configurations;

/// <summary>
/// MediaFile TPH 继承配置。
/// AudioFile 和 VideoFile 共用 MediaFiles 表，
/// Discriminator 列为 MediaType（存储枚举名称字符串）。
/// </summary>
public class MediaFileConfiguration : IEntityTypeConfiguration<MediaFile>
{
    public void Configure(EntityTypeBuilder<MediaFile> builder)
    {
        builder.ToTable("MediaFiles");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Path).IsRequired().HasMaxLength(1000);
        builder.Property(m => m.FileName).IsRequired().HasMaxLength(260);
        builder.Property(m => m.Hash).HasMaxLength(64);

        // 唯一索引：同一路径不重复导入
        builder.HasIndex(m => m.Path).IsUnique();
        // 哈希索引：加速重复检测查询
        builder.HasIndex(m => m.Hash);

        // TPH 鉴别器
        builder.HasDiscriminator(m => m.MediaType)
               .HasValue<AudioFile>(Core.Enums.MediaType.Audio)
               .HasValue<VideoFile>(Core.Enums.MediaType.Video);
    }
}
