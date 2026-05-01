using MediaManager.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediaManager.Data.Configurations;

/// <summary>
/// MediaFile TPH 继承配置。
/// AudioFile、VideoFile、ImageFile、DocumentFile、OtherFile 及其扩展类共用 MediaFiles 表，
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
        builder.Property(m => m.SubType).HasMaxLength(100);

        // 唯一索引：同一路径不重复导入
        builder.HasIndex(m => m.Path).IsUnique();
        // 哈希索引：加速重复检测查询
        builder.HasIndex(m => m.Hash);

        // TPH 鉴别器配置 - 支持所有类型和扩展类型
        var discriminatorBuilder = builder.HasDiscriminator(m => m.MediaType)
               .HasValue<AudioFile>(Core.Enums.MediaType.Audio)
               .HasValue<VideoFile>(Core.Enums.MediaType.Video)
               .HasValue<ImageFile>(Core.Enums.MediaType.Image)
               .HasValue<DocumentFile>(Core.Enums.MediaType.Other)
               .HasValue<OtherFile>(Core.Enums.MediaType.Other);

        // 扩展类型映射（使用相同的 MediaType，但可能需要额外的逻辑来区分）
        // EF Core 不允许多级继承在同一个鉴别器上使用相同的值，
        // 但我们可以通过实体名称或其他方式来处理扩展类型
    }
}
