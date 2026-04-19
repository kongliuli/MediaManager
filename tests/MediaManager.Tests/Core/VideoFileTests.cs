using FluentAssertions;
using MediaManager.Core.Models;
using Xunit;

namespace MediaManager.Tests.Core;

/// <summary>
/// TC-005 ~ TC-006: VideoFile.Resolution 计算属性测试
/// </summary>
public class VideoFileTests
{
    [Fact] // TC-005
    public void Resolution_NormalDimensions_ReturnsFormattedString()
    {
        var video = new VideoFile { Width = 1920, Height = 1080 };

        video.Resolution.Should().Be("1920×1080");
    }

    [Fact] // TC-006
    public void Resolution_ZeroDimensions_ReturnsZeroString()
    {
        var video = new VideoFile { Width = 0, Height = 0 };

        video.Resolution.Should().Be("0×0");
    }
}
