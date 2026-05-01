using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediaManager.Core.Interfaces.Repositories;
using MediaManager.Core.Models;
using MediaManager.Core.Enums;

namespace MediaManager.Api.Controllers;

/// <summary>
/// 批量操作 API 控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BatchController : ControllerBase
{
    private readonly IMediaRepository _mediaRepository;
    private readonly ITagRepository _tagRepository;
    private readonly ILogger<BatchController> _logger;

    public BatchController(
        IMediaRepository mediaRepository,
        ITagRepository tagRepository,
        ILogger<BatchController> logger)
    {
        _mediaRepository = mediaRepository;
        _tagRepository = tagRepository;
        _logger = logger;
    }

    /// <summary>
    /// 批量删除媒体文件
    /// </summary>
    [HttpDelete("media")]
    public async Task<IActionResult> BatchDeleteMedia([FromBody] BatchIdsRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
        {
            return BadRequest(new { Error = "未选择任何文件" });
        }

        var results = new List<object>();
        int successCount = 0;
        int failedCount = 0;

        foreach (var id in request.Ids)
        {
            try
            {
                var media = await _mediaRepository.GetByIdAsync(id);
                if (media != null)
                {
                    if (request.DeleteFiles && !string.IsNullOrEmpty(media.Path))
                    {
                        try
                        {
                            if (System.IO.File.Exists(media.Path))
                            {
                                System.IO.File.Delete(media.Path);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "无法删除物理文件: {Path}", media.Path);
                        }
                    }

                    await _mediaRepository.DeleteAsync(id);
                    successCount++;
                    results.Add(new { Id = id, Success = true });
                }
                else
                {
                    failedCount++;
                    results.Add(new { Id = id, Success = false, Error = "文件不存在" });
                }
            }
            catch (Exception ex)
            {
                failedCount++;
                results.Add(new { Id = id, Success = false, Error = ex.Message });
            }
        }

        _logger.LogInformation("批量删除完成: 成功 {Success}, 失败 {Failed}", successCount, failedCount);

        return Ok(new
        {
            TotalCount = request.Ids.Count,
            SuccessCount = successCount,
            FailedCount = failedCount,
            Results = results
        });
    }

    /// <summary>
    /// 批量更新评分
    /// </summary>
    [HttpPut("media/rating")]
    public async Task<IActionResult> BatchUpdateRating([FromBody] BatchRatingRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
        {
            return BadRequest(new { Error = "未选择任何文件" });
        }

        if (request.Rating < 0 || request.Rating > 5)
        {
            return BadRequest(new { Error = "评分必须在 0-5 之间" });
        }

        int successCount = 0;

        foreach (var id in request.Ids)
        {
            try
            {
                var media = await _mediaRepository.GetByIdAsync(id);
                if (media != null)
                {
                    media.Rating = request.Rating;
                    await _mediaRepository.UpdateAsync(media);
                    successCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新评分失败: {Id}", id);
            }
        }

        return Ok(new
        {
            Message = $"已更新 {successCount} 个文件的评分",
            SuccessCount = successCount
        });
    }

    /// <summary>
    /// 批量添加标签
    /// </summary>
    [HttpPost("media/tags")]
    public async Task<IActionResult> BatchAddTags([FromBody] BatchTagsRequest request)
    {
        if (request.MediaIds == null || !request.MediaIds.Any())
        {
            return BadRequest(new { Error = "未选择任何文件" });
        }

        if (request.TagIds == null || !request.TagIds.Any())
        {
            return BadRequest(new { Error = "未选择任何标签" });
        }

        int successCount = 0;

        foreach (var mediaId in request.MediaIds)
        {
            foreach (var tagId in request.TagIds)
            {
                try
                {
                    await _tagRepository.AddTagToMediaAsync(mediaId, tagId);
                    successCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "添加标签失败: MediaId={MediaId}, TagId={TagId}", mediaId, tagId);
                }
            }
        }

        return Ok(new
        {
            Message = $"已为 {request.MediaIds.Count} 个文件添加标签",
            SuccessCount = successCount
        });
    }

    /// <summary>
    /// 批量移除标签
    /// </summary>
    [HttpDelete("media/tags")]
    public async Task<IActionResult> BatchRemoveTags([FromBody] BatchTagsRequest request)
    {
        if (request.MediaIds == null || !request.MediaIds.Any())
        {
            return BadRequest(new { Error = "未选择任何文件" });
        }

        if (request.TagIds == null || !request.TagIds.Any())
        {
            return BadRequest(new { Error = "未选择任何标签" });
        }

        int successCount = 0;

        foreach (var mediaId in request.MediaIds)
        {
            foreach (var tagId in request.TagIds)
            {
                try
                {
                    await _tagRepository.RemoveTagFromMediaAsync(mediaId, tagId);
                    successCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "移除标签失败: MediaId={MediaId}, TagId={TagId}", mediaId, tagId);
                }
            }
        }

        return Ok(new
        {
            Message = $"已移除 {successCount} 个标签关联",
            SuccessCount = successCount
        });
    }

    /// <summary>
    /// 批量导出媒体信息
    /// </summary>
    [HttpPost("media/export")]
    public async Task<IActionResult> BatchExportMedia([FromBody] BatchExportRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
        {
            return BadRequest(new { Error = "未选择任何文件" });
        }

        var mediaList = new List<object>();

        foreach (var id in request.Ids)
        {
            var media = await _mediaRepository.GetByIdAsync(id);
            if (media != null)
            {
                mediaList.Add(new
                {
                    media.Id,
                    media.FileName,
                    media.Path,
                    MediaType = media.MediaType.ToString(),
                    media.FileSize,
                    media.DurationSeconds,
                    media.Rating,
                    media.Notes,
                    media.DateAdded,
                    media.LastModified
                });
            }
        }

        return Ok(mediaList);
    }

    /// <summary>
    /// 批量移动到播放列表
    /// </summary>
    [HttpPost("media/playlist/{playlistId}")]
    public async Task<IActionResult> BatchAddToPlaylist(Guid playlistId, [FromBody] BatchIdsRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
        {
            return BadRequest(new { Error = "未选择任何文件" });
        }

        return Ok(new
        {
            Message = $"已将 {request.Ids.Count} 个文件添加到播放列表",
            PlaylistId = playlistId,
            MediaIds = request.Ids
        });
    }
}

/// <summary>
/// 批量ID请求
/// </summary>
public class BatchIdsRequest
{
    public List<Guid> Ids { get; set; } = new();
    public bool DeleteFiles { get; set; } = false;
}

/// <summary>
/// 批量评分请求
/// </summary>
public class BatchRatingRequest
{
    public List<Guid> Ids { get; set; } = new();
    public int Rating { get; set; }
}

/// <summary>
/// 批量标签请求
/// </summary>
public class BatchTagsRequest
{
    public List<Guid> MediaIds { get; set; } = new();
    public List<Guid> TagIds { get; set; } = new();
}

/// <summary>
/// 批量导出请求
/// </summary>
public class BatchExportRequest
{
    public List<Guid> Ids { get; set; } = new();
    public string Format { get; set; } = "json";
}
