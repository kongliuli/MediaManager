using FluentAssertions;
using MediaManager.Core.Models;
using Xunit;

namespace MediaManager.Tests.Core;

/// <summary>
/// TC-001 ~ TC-004: DuplicateGroup.ReclaimableSize 计算属性测试
/// </summary>
public class DuplicateGroupTests
{
    private static AudioFile MakeFile(long fileSize) => new AudioFile
    {
        Path = $"/fake/{Guid.NewGuid()}.mp3",
        FileName = "test.mp3",
        FileSize = fileSize,
        Hash = "abc123"
    };

    [Fact] // TC-001
    public void ReclaimableSize_SingleFile_ReturnsZero()
    {
        var group = new DuplicateGroup
        {
            GroupHash = "abc",
            Files = [MakeFile(1000)]
        };

        group.ReclaimableSize.Should().Be(0);
    }

    [Fact] // TC-002
    public void ReclaimableSize_TwoFiles_ReturnsOneCopy()
    {
        var group = new DuplicateGroup
        {
            GroupHash = "abc",
            Files = [MakeFile(1000), MakeFile(1000)]
        };

        group.ReclaimableSize.Should().Be(1000);
    }

    [Fact] // TC-003
    public void ReclaimableSize_ThreeFiles_ReturnsTwoCopies()
    {
        var group = new DuplicateGroup
        {
            GroupHash = "abc",
            Files = [MakeFile(500), MakeFile(500), MakeFile(500)]
        };

        group.ReclaimableSize.Should().Be(1000);
    }

    [Fact] // TC-004
    public void ReclaimableSize_EmptyFiles_ReturnsZero()
    {
        var group = new DuplicateGroup
        {
            GroupHash = "abc",
            Files = []
        };

        group.ReclaimableSize.Should().Be(0);
    }
}
