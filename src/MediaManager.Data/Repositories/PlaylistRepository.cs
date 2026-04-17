using MediaManager.Core.Interfaces.Repositories;
using MediaManager.Core.Models;
using MediaManager.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace MediaManager.Data.Repositories;

/// <summary>
/// 播放列表仓储实现。
/// 使用 EF Core 进行数据库操作，支持播放列表的 CRUD 操作和条目管理。
/// </summary>
public class PlaylistRepository(MediaDbContext dbContext) : IPlaylistRepository
{
    public async Task<IReadOnlyList<Playlist>> GetAllAsync(CancellationToken ct = default)
    {
        return await dbContext.Playlists
            .Include(p => p.Items)
            .ThenInclude(pi => pi.MediaFile)
            .ToListAsync(ct);
    }

    public async Task<Playlist?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await dbContext.Playlists
            .Include(p => p.Items)
            .ThenInclude(pi => pi.MediaFile)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Playlist> CreateAsync(Playlist playlist, CancellationToken ct = default)
    {
        await dbContext.Playlists.AddAsync(playlist, ct);
        await dbContext.SaveChangesAsync(ct);
        return playlist;
    }

    public async Task UpdateAsync(Playlist playlist, CancellationToken ct = default)
    {
        dbContext.Playlists.Update(playlist);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var playlist = await GetByIdAsync(id, ct);
        if (playlist != null)
        {
            dbContext.Playlists.Remove(playlist);
            await dbContext.SaveChangesAsync(ct);
        }
    }

    public async Task AddItemAsync(int playlistId, long mediaFileId, CancellationToken ct = default)
    {
        var playlist = await GetByIdAsync(playlistId, ct);
        if (playlist == null)
        {
            throw new InvalidOperationException($"播放列表不存在: {playlistId}");
        }

        var mediaFile = await dbContext.MediaFiles.FindAsync([mediaFileId], ct);
        if (mediaFile == null)
        {
            throw new InvalidOperationException($"媒体文件不存在: {mediaFileId}");
        }

        // 检查是否已经存在
            if (!playlist.Items.Any(pi => pi.MediaFileId == mediaFileId))
            {
                var playlistItem = new PlaylistItem
                {
                    PlaylistId = playlistId,
                    MediaFileId = mediaFileId,
                    SortOrder = playlist.Items.Count + 1
                };

                await dbContext.PlaylistItems.AddAsync(playlistItem, ct);
                await dbContext.SaveChangesAsync(ct);
            }
    }

    public async Task RemoveItemAsync(int playlistId, long mediaFileId, CancellationToken ct = default)
    {
        var playlistItem = await dbContext.PlaylistItems
            .FirstOrDefaultAsync(pi => pi.PlaylistId == playlistId && pi.MediaFileId == mediaFileId, ct);

        if (playlistItem != null)
        {
            dbContext.PlaylistItems.Remove(playlistItem);
            await dbContext.SaveChangesAsync(ct);

            // 更新剩余条目的顺序
            var remainingItems = await dbContext.PlaylistItems
                .Where(pi => pi.PlaylistId == playlistId)
                .OrderBy(pi => pi.SortOrder)
                .ToListAsync(ct);

            for (int i = 0; i < remainingItems.Count; i++)
            {
                remainingItems[i].SortOrder = i + 1;
            }

            await dbContext.SaveChangesAsync(ct);
        }
    }

    public async Task ReorderItemsAsync(int playlistId, IEnumerable<long> orderedMediaIds, CancellationToken ct = default)
    {
        var playlistItems = await dbContext.PlaylistItems
            .Where(pi => pi.PlaylistId == playlistId)
            .ToListAsync(ct);

        var orderedMediaIdList = orderedMediaIds.ToList();
        for (int i = 0; i < orderedMediaIdList.Count; i++)
        {
            var mediaId = orderedMediaIdList[i];
            var playlistItem = playlistItems.FirstOrDefault(pi => pi.MediaFileId == mediaId);
            if (playlistItem != null)
            {
                playlistItem.SortOrder = i + 1;
            }
        }

        await dbContext.SaveChangesAsync(ct);
    }
}