using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediaManager.Core.Interfaces.Repositories;
using MediaManager.Core.Models;

namespace MediaManager.Api.Controllers;

/// <summary>
/// 播放列表管理 API 控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlaylistsController : ControllerBase
{
    private readonly IPlaylistRepository _playlistRepository;
    private readonly IMediaRepository _mediaRepository;
    private readonly ILogger<PlaylistsController> _logger;

    public PlaylistsController(
        IPlaylistRepository playlistRepository,
        IMediaRepository mediaRepository,
        ILogger<PlaylistsController> logger)
    {
        _playlistRepository = playlistRepository;
        _mediaRepository = mediaRepository;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有播放列表
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllPlaylists()
    {
        var playlists = await _playlistRepository.GetAllAsync();
        var playlistDtos = playlists.Select(p => new PlaylistDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            ItemCount = p.Items?.Count ?? 0,
            TotalDuration = p.Items?.Sum(i => i.MediaFile?.DurationSeconds ?? 0) ?? 0,
            CreatedAt = p.CreatedAt
        });
        return Ok(playlistDtos);
    }

    /// <summary>
    /// 获取播放列表详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPlaylist(Guid id)
    {
        var playlist = await _playlistRepository.GetByIdAsync(id);
        if (playlist == null)
        {
            return NotFound(new { Error = "播放列表不存在" });
        }

        return Ok(new PlaylistDetailDto
        {
            Id = playlist.Id,
            Name = playlist.Name,
            Description = playlist.Description,
            Items = playlist.Items?.OrderBy(i => i.OrderIndex).Select(i => new PlaylistItemDto
            {
                Id = i.Id,
                OrderIndex = i.OrderIndex,
                MediaId = i.MediaFileId,
                MediaFileName = i.MediaFile?.FileName ?? "",
                MediaType = i.MediaFile?.MediaType.ToString() ?? "",
                Duration = i.MediaFile?.DurationSeconds ?? 0
            }).ToList() ?? new List<PlaylistItemDto>(),
            TotalDuration = playlist.Items?.Sum(i => i.MediaFile?.DurationSeconds ?? 0) ?? 0,
            CreatedAt = playlist.CreatedAt
        });
    }

    /// <summary>
    /// 创建新播放列表
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreatePlaylist([FromBody] CreatePlaylistRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { Error = "播放列表名称不能为空" });
        }

        var playlist = new Playlist
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? "",
            CreatedAt = DateTime.UtcNow
        };

        await _playlistRepository.AddAsync(playlist);
        _logger.LogInformation("创建新播放列表: {PlaylistName}", playlist.Name);

        return CreatedAtAction(nameof(GetPlaylist), new { id = playlist.Id }, new PlaylistDto
        {
            Id = playlist.Id,
            Name = playlist.Name,
            Description = playlist.Description,
            ItemCount = 0,
            TotalDuration = 0,
            CreatedAt = playlist.CreatedAt
        });
    }

    /// <summary>
    /// 更新播放列表
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePlaylist(Guid id, [FromBody] UpdatePlaylistRequest request)
    {
        var playlist = await _playlistRepository.GetByIdAsync(id);
        if (playlist == null)
        {
            return NotFound(new { Error = "播放列表不存在" });
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            playlist.Name = request.Name.Trim();
        }
        
        if (request.Description != null)
        {
            playlist.Description = request.Description.Trim();
        }

        await _playlistRepository.UpdateAsync(playlist);
        _logger.LogInformation("更新播放列表: {PlaylistName}", playlist.Name);

        return Ok(new PlaylistDto
        {
            Id = playlist.Id,
            Name = playlist.Name,
            Description = playlist.Description,
            ItemCount = playlist.Items?.Count ?? 0,
            TotalDuration = playlist.Items?.Sum(i => i.MediaFile?.DurationSeconds ?? 0) ?? 0,
            CreatedAt = playlist.CreatedAt
        });
    }

    /// <summary>
    /// 删除播放列表
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlaylist(Guid id)
    {
        var playlist = await _playlistRepository.GetByIdAsync(id);
        if (playlist == null)
        {
            return NotFound(new { Error = "播放列表不存在" });
        }

        await _playlistRepository.DeleteAsync(id);
        _logger.LogInformation("删除播放列表: {PlaylistName}", playlist.Name);

        return Ok(new { Message = "播放列表已删除" });
    }

    /// <summary>
    /// 添加媒体到播放列表
    /// </summary>
    [HttpPost("{id}/items")]
    public async Task<IActionResult> AddItemToPlaylist(Guid id, [FromBody] AddPlaylistItemRequest request)
    {
        var playlist = await _playlistRepository.GetByIdAsync(id);
        if (playlist == null)
        {
            return NotFound(new { Error = "播放列表不存在" });
        }

        var media = await _mediaRepository.GetByIdAsync(request.MediaId);
        if (media == null)
        {
            return NotFound(new { Error = "媒体文件不存在" });
        }

        var items = playlist.Items?.ToList() ?? new List<PlaylistItem>();
        var maxOrder = items.Any() ? items.Max(i => i.OrderIndex) : 0;

        var item = new PlaylistItem
        {
            Id = Guid.NewGuid(),
            PlaylistId = id,
            MediaFileId = request.MediaId,
            OrderIndex = request.OrderIndex ?? (maxOrder + 1)
        };

        await _playlistRepository.AddItemAsync(item);
        _logger.LogInformation("添加媒体 {MediaId} 到播放列表 {PlaylistId}", request.MediaId, id);

        return Ok(new { Message = "已添加到播放列表" });
    }

    /// <summary>
    /// 从播放列表移除媒体
    /// </summary>
    [HttpDelete("{id}/items/{itemId}")]
    public async Task<IActionResult> RemoveItemFromPlaylist(Guid id, Guid itemId)
    {
        await _playlistRepository.RemoveItemAsync(itemId);
        _logger.LogInformation("从播放列表 {PlaylistId} 移除项目 {ItemId}", id, itemId);

        return Ok(new { Message = "已从播放列表移除" });
    }

    /// <summary>
    /// 重新排序播放列表项
    /// </summary>
    [HttpPut("{id}/items/reorder")]
    public async Task<IActionResult> ReorderPlaylistItems(Guid id, [FromBody] ReorderItemsRequest request)
    {
        var playlist = await _playlistRepository.GetByIdAsync(id);
        if (playlist == null)
        {
            return NotFound(new { Error = "播放列表不存在" });
        }

        for (int i = 0; i < request.ItemIds.Count; i++)
        {
            var item = playlist.Items?.FirstOrDefault(x => x.Id == request.ItemIds[i]);
            if (item != null)
            {
                item.OrderIndex = i + 1;
            }
        }

        await _playlistRepository.UpdateAsync(playlist);
        _logger.LogInformation("重新排序播放列表 {PlaylistId}", id);

        return Ok(new { Message = "排序已更新" });
    }

    /// <summary>
    /// 批量添加媒体到播放列表
    /// </summary>
    [HttpPost("{id}/items/batch")]
    public async Task<IActionResult> BatchAddItems(Guid id, [FromBody] BatchAddItemsRequest request)
    {
        var playlist = await _playlistRepository.GetByIdAsync(id);
        if (playlist == null)
        {
            return NotFound(new { Error = "播放列表不存在" });
        }

        var items = playlist.Items?.ToList() ?? new List<PlaylistItem>();
        var maxOrder = items.Any() ? items.Max(i => i.OrderIndex) : 0;

        foreach (var mediaId in request.MediaIds)
        {
            var media = await _mediaRepository.GetByIdAsync(mediaId);
            if (media != null)
            {
                maxOrder++;
                var item = new PlaylistItem
                {
                    Id = Guid.NewGuid(),
                    PlaylistId = id,
                    MediaFileId = mediaId,
                    OrderIndex = maxOrder
                };
                await _playlistRepository.AddItemAsync(item);
            }
        }

        _logger.LogInformation("批量添加 {Count} 个媒体到播放列表 {PlaylistId}", request.MediaIds.Count, id);

        return Ok(new { Message = $"已添加 {request.MediaIds.Count} 个媒体到播放列表" });
    }
}

/// <summary>
/// 播放列表DTO
/// </summary>
public class PlaylistDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public int TotalDuration { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 播放列表详情DTO
/// </summary>
public class PlaylistDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<PlaylistItemDto> Items { get; set; } = new();
    public int TotalDuration { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 播放列表项DTO
/// </summary>
public class PlaylistItemDto
{
    public Guid Id { get; set; }
    public int OrderIndex { get; set; }
    public Guid MediaId { get; set; }
    public string MediaFileName { get; set; } = string.Empty;
    public string MediaType { get; set; } = string.Empty;
    public int Duration { get; set; }
}

/// <summary>
/// 创建播放列表请求
/// </summary>
public class CreatePlaylistRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>
/// 更新播放列表请求
/// </summary>
public class UpdatePlaylistRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// 添加播放列表项请求
/// </summary>
public class AddPlaylistItemRequest
{
    public Guid MediaId { get; set; }
    public int? OrderIndex { get; set; }
}

/// <summary>
/// 重新排序请求
/// </summary>
public class ReorderItemsRequest
{
    public List<Guid> ItemIds { get; set; } = new();
}

/// <summary>
/// 批量添加项请求
/// </summary>
public class BatchAddItemsRequest
{
    public List<Guid> MediaIds { get; set; } = new();
}
