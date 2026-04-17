using MediaManager.Core.DTOs;
using MediaManager.Core.Interfaces.Repositories;
using MediaManager.Core.Models;
using MediaManager.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace MediaManager.Data.Repositories;

/// <summary>
/// 媒体文件仓储实现。
/// 使用 EF Core 进行数据库操作，支持 CRUD 操作和搜索功能。
/// </summary>
public class MediaRepository(MediaDbContext dbContext) : IMediaRepository
{
    public async Task<MediaFile?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await dbContext.MediaFiles.FindAsync([id], ct);
    }

    public async Task<MediaFile?> GetByPathAsync(string path, CancellationToken ct = default)
    {
        return await dbContext.MediaFiles.FirstOrDefaultAsync(f => f.Path == path, ct);
    }

    public async Task<MediaSearchResult> SearchAsync(MediaSearchQuery query, CancellationToken ct = default)
    {
        var dbQuery = dbContext.MediaFiles.AsQueryable();

        // 应用搜索条件
        if (!string.IsNullOrEmpty(query.Keyword))
        {
            var keyword = $"%{query.Keyword}%";
            dbQuery = dbQuery.Where(f => 
                EF.Functions.Like(f.FileName, keyword)
            );
        }

        if (query.MediaType.HasValue)
        {
            dbQuery = dbQuery.Where(f => f.MediaType == query.MediaType.Value);
        }

        if (query.TagIds != null && query.TagIds.Any())
        {
            dbQuery = dbQuery.Where(f => f.MediaTags.Any(mt => query.TagIds.Contains(mt.TagId)));
        }

        if (query.MinDurationSeconds.HasValue)
        {
            dbQuery = dbQuery.Where(f => f.DurationSeconds >= query.MinDurationSeconds.Value);
        }
        if (query.MaxDurationSeconds.HasValue)
        {
            dbQuery = dbQuery.Where(f => f.DurationSeconds <= query.MaxDurationSeconds.Value);
        }

        if (query.DateAddedFrom.HasValue)
        {
            dbQuery = dbQuery.Where(f => f.DateAdded >= query.DateAddedFrom.Value);
        }
        if (query.DateAddedTo.HasValue)
        {
            dbQuery = dbQuery.Where(f => f.DateAdded <= query.DateAddedTo.Value);
        }

        // 计算总数
        var totalCount = await dbQuery.CountAsync(ct);

        // 应用排序
        dbQuery = query.SortBy switch
        {
            Core.Enums.SortField.FileName => query.SortDescending ? dbQuery.OrderByDescending(f => f.FileName) : dbQuery.OrderBy(f => f.FileName),
            Core.Enums.SortField.Duration => query.SortDescending ? dbQuery.OrderByDescending(f => f.DurationSeconds) : dbQuery.OrderBy(f => f.DurationSeconds),
            Core.Enums.SortField.DateAdded => query.SortDescending ? dbQuery.OrderByDescending(f => f.DateAdded) : dbQuery.OrderBy(f => f.DateAdded),
            Core.Enums.SortField.FileSize => query.SortDescending ? dbQuery.OrderByDescending(f => f.FileSize) : dbQuery.OrderBy(f => f.FileSize),
            _ => dbQuery.OrderBy(f => f.FileName)
        };

        // 应用分页
        var items = await dbQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return new MediaSearchResult
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task UpsertAsync(MediaFile file, CancellationToken ct = default)
    {
        var existingFile = await GetByPathAsync(file.Path, ct);
        if (existingFile != null)
        {
            // 更新现有文件
            dbContext.Entry(existingFile).CurrentValues.SetValues(file);
        }
        else
        {
            // 插入新文件
            await dbContext.MediaFiles.AddAsync(file, ct);
        }
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var file = await GetByIdAsync(id, ct);
        if (file != null)
        {
            dbContext.MediaFiles.Remove(file);
            await dbContext.SaveChangesAsync(ct);
        }
    }

    public async Task DeleteRangeAsync(IEnumerable<long> ids, CancellationToken ct = default)
    {
        var files = await dbContext.MediaFiles.Where(f => ids.Contains(f.Id)).ToListAsync(ct);
        if (files.Any())
        {
            dbContext.MediaFiles.RemoveRange(files);
            await dbContext.SaveChangesAsync(ct);
        }
    }

    public async Task<IReadOnlyList<(string Hash, long Id)>> GetAllHashesAsync(CancellationToken ct = default)
    {
        var result = await dbContext.MediaFiles
            .Where(f => !string.IsNullOrEmpty(f.Hash))
            .Select(f => new { f.Hash, f.Id })
            .ToListAsync(ct);

        return result.Select(f => (f.Hash, f.Id)).ToList();
    }
}