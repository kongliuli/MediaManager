using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediaManager.Core.Interfaces.Repositories;
using MediaManager.Core.Models;
using MediaManager.Core.Enums;

namespace MediaManager.Api.Controllers;

/// <summary>
/// 媒体文件 API 控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MediaController : ControllerBase
{
    private readonly IMediaRepository _mediaRepository;
    private readonly ILogger<MediaController> _logger;
    private readonly MediaManager.Core.AppConfig _appConfig;

    public MediaController(
        IMediaRepository mediaRepository,
        ILogger<MediaController> logger,
        MediaManager.Core.AppConfig appConfig)
    {
        _mediaRepository = mediaRepository;
        _logger = logger;
        _appConfig = appConfig;
    }

    /// <summary>
    /// 获取媒体文件列表（支持分页、搜索、筛选）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMedia(
        [FromQuery] string? search = null,
        [FromQuery] MediaType? mediaType = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] SortField? sortBy = null,
        [FromQuery] bool sortDescending = true)
    {
        try
        {
            var query = new MediaManager.Core.DTOs.MediaSearchQuery
            {
                Keyword = search,
                MediaType = mediaType,
                Page = page,
                PageSize = pageSize,
                SortBy = sortBy ?? SortField.DateAdded,
                SortDescending = sortDescending
            };

            var result = await _mediaRepository.SearchAsync(query);
            return Ok(new
            {
                Items = result.Items,
                TotalCount = result.TotalCount,
                Page = page,
                PageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting media files");
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    /// <summary>
    /// 获取指定媒体文件详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMediaById(Guid id)
    {
        try
        {
            var media = await _mediaRepository.GetByIdAsync(id);
            if (media == null)
            {
                return NotFound($"媒体文件不存在: {id}");
            }
            return Ok(media);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting media file {Id}", id);
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    /// <summary>
    /// 删除媒体文件
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMedia(Guid id, [FromQuery] bool deleteFile = false)
    {
        try
        {
            var media = await _mediaRepository.GetByIdAsync(id);
            if (media == null)
            {
                return NotFound($"媒体文件不存在: {id}");
            }

            var filePath = media.Path;
            await _mediaRepository.DeleteAsync(id);

            if (deleteFile && System.IO.File.Exists(filePath))
            {
                try
                {
                    System.IO.File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete file {Path}", filePath);
                }
            }

            return Ok(new { Message = "删除成功" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting media file {Id}", id);
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    /// <summary>
    /// 更新媒体文件信息
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMedia(Guid id, [FromBody] UpdateMediaRequest request)
    {
        try
        {
            var media = await _mediaRepository.GetByIdAsync(id);
            if (media == null)
            {
                return NotFound($"媒体文件不存在: {id}");
            }

            if (request.Rating.HasValue)
            {
                media.Rating = request.Rating.Value;
            }
            if (!string.IsNullOrEmpty(request.Notes))
            {
                media.Notes = request.Notes;
            }

            await _mediaRepository.UpdateAsync(media);
            return Ok(media);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating media file {Id}", id);
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    /// <summary>
    /// 获取缩略图
    /// </summary>
    [HttpGet("{id}/thumbnail")]
    public IActionResult GetThumbnail(Guid id)
    {
        var thumbnailPath = Path.Combine(_appConfig.ThumbnailDirectory, $"{id}.jpg");
        if (!System.IO.File.Exists(thumbnailPath))
        {
            return NotFound("缩略图不存在");
        }

        var fileStream = System.IO.File.OpenRead(thumbnailPath);
        return File(fileStream, "image/jpeg");
    }

    /// <summary>
    /// 获取统计信息
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            var allMedia = (await _mediaRepository.GetAllAsync()).ToList();
            var audioCount = allMedia.Count(m => m.MediaType == MediaType.Audio);
            var videoCount = allMedia.Count(m => m.MediaType == MediaType.Video);
            var imageCount = allMedia.Count(m => m.MediaType == MediaType.Image);
            var totalSize = allMedia.Sum(m => m.FileSize);

            return Ok(new
            {
                TotalCount = allMedia.Count,
                AudioCount = audioCount,
                VideoCount = videoCount,
                ImageCount = imageCount,
                TotalSize = totalSize,
                TotalSizeFormatted = FormatFileSize(totalSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stats");
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    private string FormatFileSize(long bytes)
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

/// <summary>
/// 更新媒体文件请求
/// </summary>
public class UpdateMediaRequest
{
    public int? Rating { get; set; }
    public string? Notes { get; set; }
}
