using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediaManager.Core.Interfaces.Repositories;
using MediaManager.Core.Models;

namespace MediaManager.Api.Controllers;

/// <summary>
/// 标签管理 API 控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TagsController : ControllerBase
{
    private readonly ITagRepository _tagRepository;
    private readonly IMediaRepository _mediaRepository;
    private readonly ILogger<TagsController> _logger;

    public TagsController(
        ITagRepository tagRepository,
        IMediaRepository mediaRepository,
        ILogger<TagsController> logger)
    {
        _tagRepository = tagRepository;
        _mediaRepository = mediaRepository;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有标签
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllTags()
    {
        var tags = await _tagRepository.GetAllAsync();
        var tagDtos = tags.Select(t => new TagDto
        {
            Id = t.Id,
            Name = t.Name,
            MediaCount = t.MediaTags?.Count ?? 0
        });
        return Ok(tagDtos);
    }

    /// <summary>
    /// 获取标签详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTag(Guid id)
    {
        var tag = await _tagRepository.GetByIdAsync(id);
        if (tag == null)
        {
            return NotFound(new { Error = "标签不存在" });
        }

        return Ok(new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            MediaCount = tag.MediaTags?.Count ?? 0
        });
    }

    /// <summary>
    /// 创建新标签
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateTag([FromBody] CreateTagRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { Error = "标签名称不能为空" });
        }

        var existingTag = await _tagRepository.GetByNameAsync(request.Name);
        if (existingTag != null)
        {
            return BadRequest(new { Error = "标签已存在" });
        }

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim()
        };

        await _tagRepository.AddAsync(tag);
        _logger.LogInformation("创建新标签: {TagName}", tag.Name);

        return CreatedAtAction(nameof(GetTag), new { id = tag.Id }, new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            MediaCount = 0
        });
    }

    /// <summary>
    /// 更新标签
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTag(Guid id, [FromBody] UpdateTagRequest request)
    {
        var tag = await _tagRepository.GetByIdAsync(id);
        if (tag == null)
        {
            return NotFound(new { Error = "标签不存在" });
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { Error = "标签名称不能为空" });
        }

        var existingTag = await _tagRepository.GetByNameAsync(request.Name);
        if (existingTag != null && existingTag.Id != id)
        {
            return BadRequest(new { Error = "标签名称已被使用" });
        }

        tag.Name = request.Name.Trim();
        await _tagRepository.UpdateAsync(tag);
        _logger.LogInformation("更新标签: {TagName}", tag.Name);

        return Ok(new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            MediaCount = tag.MediaTags?.Count ?? 0
        });
    }

    /// <summary>
    /// 删除标签
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTag(Guid id)
    {
        var tag = await _tagRepository.GetByIdAsync(id);
        if (tag == null)
        {
            return NotFound(new { Error = "标签不存在" });
        }

        await _tagRepository.DeleteAsync(id);
        _logger.LogInformation("删除标签: {TagName}", tag.Name);

        return Ok(new { Message = "标签已删除" });
    }

    /// <summary>
    /// 为媒体文件添加标签
    /// </summary>
    [HttpPost("media/{mediaId}/tags/{tagId}")]
    public async Task<IActionResult> AddTagToMedia(Guid mediaId, Guid tagId)
    {
        var media = await _mediaRepository.GetByIdAsync(mediaId);
        if (media == null)
        {
            return NotFound(new { Error = "媒体文件不存在" });
        }

        var tag = await _tagRepository.GetByIdAsync(tagId);
        if (tag == null)
        {
            return NotFound(new { Error = "标签不存在" });
        }

        await _tagRepository.AddTagToMediaAsync(mediaId, tagId);
        _logger.LogInformation("为媒体文件 {MediaId} 添加标签 {TagId}", mediaId, tagId);

        return Ok(new { Message = "标签已添加" });
    }

    /// <summary>
    /// 从媒体文件移除标签
    /// </summary>
    [HttpDelete("media/{mediaId}/tags/{tagId}")]
    public async Task<IActionResult> RemoveTagFromMedia(Guid mediaId, Guid tagId)
    {
        await _tagRepository.RemoveTagFromMediaAsync(mediaId, tagId);
        _logger.LogInformation("从媒体文件 {MediaId} 移除标签 {TagId}", mediaId, tagId);

        return Ok(new { Message = "标签已移除" });
    }

    /// <summary>
    /// 获取媒体文件的所有标签
    /// </summary>
    [HttpGet("media/{mediaId}")]
    public async Task<IActionResult> GetMediaTags(Guid mediaId)
    {
        var tags = await _tagRepository.GetTagsForMediaAsync(mediaId);
        var tagDtos = tags.Select(t => new TagDto
        {
            Id = t.Id,
            Name = t.Name,
            MediaCount = 0
        });
        return Ok(tagDtos);
    }

    /// <summary>
    /// 获取标签下的所有媒体文件
    /// </summary>
    [HttpGet("{id}/media")]
    public async Task<IActionResult> GetMediaByTag(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var tag = await _tagRepository.GetByIdAsync(id);
        if (tag == null)
        {
            return NotFound(new { Error = "标签不存在" });
        }

        var mediaFiles = await _tagRepository.GetMediaByTagAsync(id);
        var pagedMedia = mediaFiles.Skip((page - 1) * pageSize).Take(pageSize);

        return Ok(new
        {
            Tag = new TagDto { Id = tag.Id, Name = tag.Name },
            MediaFiles = pagedMedia.Select(m => new MediaSummaryDto
            {
                Id = m.Id,
                FileName = m.FileName,
                Path = m.Path,
                MediaType = m.MediaType.ToString(),
                FileSize = m.FileSize,
                DurationSeconds = m.DurationSeconds
            }),
            TotalCount = mediaFiles.Count(),
            Page = page,
            PageSize = pageSize
        });
    }
}

/// <summary>
/// 标签DTO
/// </summary>
public class TagDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MediaCount { get; set; }
}

/// <summary>
/// 创建标签请求
/// </summary>
public class CreateTagRequest
{
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// 更新标签请求
/// </summary>
public class UpdateTagRequest
{
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// 媒体摘要DTO
/// </summary>
public class MediaSummaryDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string MediaType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int DurationSeconds { get; set; }
}
