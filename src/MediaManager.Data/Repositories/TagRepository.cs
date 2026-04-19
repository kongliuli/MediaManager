using MediaManager.Core.Interfaces.Repositories;
using MediaManager.Core.Models;
using MediaManager.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace MediaManager.Data.Repositories;

/// <summary>
/// 标签仓储实现。
/// 使用 EF Core 进行数据库操作，支持标签的 CRUD 操作和媒体文件的标签管理。
/// </summary>
public class TagRepository(MediaDbContext dbContext) : ITagRepository
{
    public async Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken ct = default)
    {
        return await dbContext.Tags.ToListAsync(ct);
    }

    public async Task<Tag?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await dbContext.Tags.FindAsync([id], ct);
    }

    public async Task<Tag> CreateAsync(Tag tag, CancellationToken ct = default)
    {
        await dbContext.Tags.AddAsync(tag, ct);
        await dbContext.SaveChangesAsync(ct);
        return tag;
    }

    public async Task UpdateAsync(Tag tag, CancellationToken ct = default)
    {
        dbContext.Tags.Update(tag);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var tag = await GetByIdAsync(id, ct);
        if (tag != null)
        {
            dbContext.Tags.Remove(tag);
            await dbContext.SaveChangesAsync(ct);
        }
    }

    public async Task AddTagToMediaAsync(long mediaFileId, int tagId, CancellationToken ct = default)
    {
        // 检查媒体文件是否存在
        var mediaFile = await dbContext.MediaFiles.FindAsync([mediaFileId], ct);
        if (mediaFile == null)
        {
            throw new InvalidOperationException($"媒体文件不存在: {mediaFileId}");
        }

        // 检查标签是否存在
        var tag = await GetByIdAsync(tagId, ct);
        if (tag == null)
        {
            throw new InvalidOperationException($"标签不存在: {tagId}");
        }

        // 检查是否已经存在关联
        var existingMediaTag = await dbContext.MediaTags
            .FirstOrDefaultAsync(mt => mt.MediaFileId == mediaFileId && mt.TagId == tagId, ct);

        if (existingMediaTag == null)
        {
            var mediaTag = new MediaTag
            {
                MediaFileId = mediaFileId,
                TagId = tagId
            };

            await dbContext.MediaTags.AddAsync(mediaTag, ct);
            await dbContext.SaveChangesAsync(ct);
        }
    }

    public async Task RemoveTagFromMediaAsync(long mediaFileId, int tagId, CancellationToken ct = default)
    {
        var mediaTag = await dbContext.MediaTags
            .FirstOrDefaultAsync(mt => mt.MediaFileId == mediaFileId && mt.TagId == tagId, ct);

        if (mediaTag != null)
        {
            dbContext.MediaTags.Remove(mediaTag);
            await dbContext.SaveChangesAsync(ct);
        }
    }
}