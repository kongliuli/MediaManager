using FluentAssertions;
using MediaManager.Core.DTOs;
using MediaManager.Core.Enums;
using Xunit;

namespace MediaManager.Tests.Core;

/// <summary>
/// TC-007 ~ TC-010: ScanProgressReport.ProgressPercent 计算属性测试
/// </summary>
public class ScanProgressReportTests
{
    [Fact] // TC-007
    public void ProgressPercent_HalfProcessed_Returns50()
    {
        var report = new ScanProgressReport
        {
            TotalDiscovered = 100,
            Processed = 50
        };

        report.ProgressPercent.Should().Be(50.0);
    }

    [Fact] // TC-008
    public void ProgressPercent_TotalIsZero_ReturnsZero()
    {
        var report = new ScanProgressReport
        {
            TotalDiscovered = 0,
            Processed = 0
        };

        report.ProgressPercent.Should().Be(0.0);
    }

    [Fact] // TC-009
    public void ProgressPercent_AllProcessed_Returns100()
    {
        var report = new ScanProgressReport
        {
            TotalDiscovered = 10,
            Processed = 10
        };

        report.ProgressPercent.Should().Be(100.0);
    }

    [Fact] // TC-010
    public void ProgressPercent_ProcessedExceedsTotal_CapsAt100()
    {
        var report = new ScanProgressReport
        {
            TotalDiscovered = 10,
            Processed = 15
        };

        report.ProgressPercent.Should().Be(100.0);
    }

    [Fact]
    public void DefaultStatus_IsIdle()
    {
        var report = new ScanProgressReport();

        report.Status.Should().Be(ScanStatus.Idle);
    }
}
