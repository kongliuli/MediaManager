using MediaManager.Core.Models;

namespace MediaManager.Core.Interfaces.Repositories;

/// <summary>标签仓储接口</summary>
public interface ITagRepository
{
    Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken ct = default);
    Task<Tag?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Tag> CreateAsync(Tag tag, CancellationToken ct = default);
    Task UpdateAsync(Tag tag, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);

    Task AddTagToMediaAsync(long mediaFileId, int tagId, CancellationToken ct = default);
    Task RemoveTagFromMediaAsync(long mediaFileId, int tagId, CancellationToken ct = default);
}
