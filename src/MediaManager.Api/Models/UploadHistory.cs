using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaManager.Api.Models;

/// <summary>
/// 上传历史记录
/// </summary>
[Table("UploadHistories")]
public class UploadHistory
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string OriginalFileName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(2000)]
    public string FilePath { get; set; } = string.Empty;
    
    public long FileSize { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string StorageType { get; set; } = "Local";
    
    public Guid? UserId { get; set; }
    
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsDeleted { get; set; } = false;
    
    public DateTime? DeletedAt { get; set; }
    
    [MaxLength(500)]
    public string? PublicUrl { get; set; }
    
    [MaxLength(2000)]
    public string? ThumbnailUrl { get; set; }
    
    public int? DurationSeconds { get; set; }
    
    public int? Width { get; set; }
    
    public int? Height { get; set; }
}

/// <summary>
/// 上传历史DTO
/// </summary>
public class UploadHistoryDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string StorageType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string? PublicUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int? DurationSeconds { get; set; }
    public string FileSizeFormatted => FormatFileSize(FileSize);
    
    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size = size / 1024;
        }
        return $"{size:0.##} {sizes[order]}";
    }
}
