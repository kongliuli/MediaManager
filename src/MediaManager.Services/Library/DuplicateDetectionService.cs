using MediaManager.Core.Interfaces.Repositories;
using MediaManager.Core.Interfaces.Services;
using MediaManager.Core.Models;

namespace MediaManager.Services.Library;

/// <summary>
/// 重复文件检测服务。
/// 基于 SHA256 哈希识别内容完全相同的文件，将它们分组并计算可回收空间。
/// </summary>
public class DuplicateDetectionService(IMediaRepository mediaRepository) : IDuplicateDetectionService
{
    public async Task<IReadOnlyList<DuplicateGroup>> FindDuplicatesAsync(CancellationToken ct = default)
    {
        // 获取所有文件的哈希值和 ID
        var hashes = await mediaRepository.GetAllHashesAsync(ct);

        // 按哈希值分组，过滤出包含多个文件的组
        var duplicateHashGroups = hashes
            .GroupBy(h => h.Hash)
            .Where(g => g.Count() > 1)
            .ToList();

        var duplicateGroups = new List<DuplicateGroup>();

        // 为每个哈希组获取完整的媒体文件信息
        foreach (var hashGroup in duplicateHashGroups)
        {
            var group = new DuplicateGroup
            {
                GroupHash = hashGroup.Key,
                Files = new List<MediaFile>()
            };

            // 获取每个文件的完整信息
            foreach (var (hash, id) in hashGroup)
            {
                var file = await mediaRepository.GetByIdAsync(id, ct);
                if (file != null)
                {
                    group.Files.Add(file);
                }
            }

            // 确保组中至少有两个文件
            if (group.Files.Count > 1)
            {
                duplicateGroups.Add(group);
            }
        }

        return duplicateGroups;
    }
}