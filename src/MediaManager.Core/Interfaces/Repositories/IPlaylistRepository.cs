using MediaManager.Core.Models;

namespace MediaManager.Core.Interfaces.Repositories;

/// <summary>播放列表仓储接口</summary>
public interface IPlaylistRepository
{
    Task<IReadOnlyList<Playlist>> GetAllAsync(CancellationToken ct = default);
    Task<Playlist?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Playlist> CreateAsync(Playlist playlist, CancellationToken ct = default);
    Task UpdateAsync(Playlist playlist, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);

    Task AddItemAsync(int playlistId, long mediaFileId, CancellationToken ct = default);
    Task RemoveItemAsync(int playlistId, long mediaFileId, CancellationToken ct = default);
    Task ReorderItemsAsync(int playlistId, IEnumerable<long> orderedMediaIds, CancellationToken ct = default);
}
