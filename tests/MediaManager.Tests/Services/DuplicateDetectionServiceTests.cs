using FluentAssertions;
using MediaManager.Core.Interfaces.Repositories;
using MediaManager.Core.Models;
using Xunit;
using MediaManager.Services.Library;
using Moq;

namespace MediaManager.Tests.Services;

/// <summary>
/// TC-021 ~ TC-025: DuplicateDetectionService 测试
/// </summary>
public class DuplicateDetectionServiceTests
{
    private readonly Mock<IMediaRepository> _repoMock = new();
    private readonly DuplicateDetectionService _sut;

    public DuplicateDetectionServiceTests()
    {
        _sut = new DuplicateDetectionService(_repoMock.Object);
    }

    private static AudioFile MakeFile(long id, string hash, long size = 1000) => new()
    {
        Id = id,
        Hash = hash,
        FileSize = size,
        Path = $"/fake/{id}.mp3",
        FileName = $"{id}.mp3"
    };

    [Fact] // TC-021
    public async Task FindDuplicatesAsync_AllUniqueHashes_ReturnsEmpty()
    {
        _repoMock.Setup(r => r.GetAllHashesAsync(default))
            .ReturnsAsync([("hash1", 1L), ("hash2", 2L), ("hash3", 3L)]);

        var result = await _sut.FindDuplicatesAsync();

        result.Should().BeEmpty();
    }

    [Fact] // TC-022
    public async Task FindDuplicatesAsync_OneDuplicatePair_ReturnsOneGroup()
    {
        _repoMock.Setup(r => r.GetAllHashesAsync(default))
            .ReturnsAsync([("sameHash", 1L), ("sameHash", 2L)]);

        _repoMock.Setup(r => r.GetByIdAsync(1L, default)).ReturnsAsync(MakeFile(1, "sameHash"));
        _repoMock.Setup(r => r.GetByIdAsync(2L, default)).ReturnsAsync(MakeFile(2, "sameHash"));

        var result = await _sut.FindDuplicatesAsync();

        result.Should().HaveCount(1);
        result[0].GroupHash.Should().Be("sameHash");
        result[0].Files.Should().HaveCount(2);
    }

    [Fact] // TC-023
    public async Task FindDuplicatesAsync_MultipleGroups_ReturnsAllGroups()
    {
        _repoMock.Setup(r => r.GetAllHashesAsync(default))
            .ReturnsAsync([
                ("hashA", 1L), ("hashA", 2L),
                ("hashB", 3L), ("hashB", 4L),
                ("hashC", 5L), ("hashC", 6L)
            ]);

        foreach (var (id, hash) in new[] { (1L, "hashA"), (2L, "hashA"), (3L, "hashB"), (4L, "hashB"), (5L, "hashC"), (6L, "hashC") })
        {
            var captured = id;
            var capturedHash = hash;
            _repoMock.Setup(r => r.GetByIdAsync(captured, default)).ReturnsAsync(MakeFile(captured, capturedHash));
        }

        var result = await _sut.FindDuplicatesAsync();

        result.Should().HaveCount(3);
    }

    [Fact] // TC-024
    public async Task FindDuplicatesAsync_FileNotFoundInRepo_GroupExcluded()
    {
        // 两个文件同哈希，但 GetByIdAsync 对其中一个返回 null
        _repoMock.Setup(r => r.GetAllHashesAsync(default))
            .ReturnsAsync([("sameHash", 1L), ("sameHash", 2L)]);

        _repoMock.Setup(r => r.GetByIdAsync(1L, default)).ReturnsAsync(MakeFile(1, "sameHash"));
        _repoMock.Setup(r => r.GetByIdAsync(2L, default)).ReturnsAsync((MediaFile?)null);

        var result = await _sut.FindDuplicatesAsync();

        // 只有1个文件被找到，不足2个，组不应加入结果
        result.Should().BeEmpty();
    }

    [Fact] // TC-025
    public async Task FindDuplicatesAsync_EmptyRepository_ReturnsEmpty()
    {
        _repoMock.Setup(r => r.GetAllHashesAsync(default))
            .ReturnsAsync([]);

        var result = await _sut.FindDuplicatesAsync();

        result.Should().BeEmpty();
    }
}
