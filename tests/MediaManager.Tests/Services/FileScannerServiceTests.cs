using FluentAssertions;
using MediaManager.Services.Scanning;
using System.IO;
using Xunit;

namespace MediaManager.Tests.Services;

/// <summary>
/// TC-011 ~ TC-020: FileScannerService 测试
/// 包含使用真实目录的集成测试（TC-R01 ~ TC-R05）
/// </summary>
public class FileScannerServiceTests : IDisposable
{
    private readonly FileScannerService _sut = new();
    private readonly string _tempDir;

    // 真实测试目录（微信文件目录，多层结构）
    private const string RealTestDir =
        @"C:\Users\admin.DESKTOP-4R9CSJI\Documents\xwechat_files\wxid_qbctxev48abj21_23fa\msg\file";

    public FileScannerServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"MediaManagerTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    // ── IsSupportedFormat ──────────────────────────────────────────────────

    [Theory] // TC-011
    [InlineData("/music/song.mp3")]
    [InlineData("/music/song.wav")]
    [InlineData("/music/song.flac")]
    [InlineData("/music/song.aac")]
    [InlineData("/music/song.ogg")]
    [InlineData("/music/song.wma")]
    [InlineData("/music/song.m4a")]
    [InlineData("/music/song.opus")]
    public void IsSupportedFormat_AudioExtensions_ReturnsTrue(string path)
    {
        _sut.IsSupportedFormat(path).Should().BeTrue();
    }

    [Theory] // TC-012
    [InlineData("/video/clip.mp4")]
    [InlineData("/video/clip.avi")]
    [InlineData("/video/clip.mov")]
    [InlineData("/video/clip.wmv")]
    [InlineData("/video/clip.flv")]
    [InlineData("/video/clip.mkv")]
    [InlineData("/video/clip.webm")]
    [InlineData("/video/clip.m4v")]
    public void IsSupportedFormat_VideoExtensions_ReturnsTrue(string path)
    {
        _sut.IsSupportedFormat(path).Should().BeTrue();
    }

    [Theory] // TC-013 - 图像文件格式
    [InlineData("/image/photo.jpg")]
    [InlineData("/image/photo.jpeg")]
    [InlineData("/image/photo.png")]
    [InlineData("/image/photo.gif")]
    [InlineData("/image/photo.bmp")]
    [InlineData("/image/photo.tiff")]
    [InlineData("/image/photo.webp")]
    [InlineData("/image/photo.svg")]
    public void IsSupportedFormat_ImageExtensions_ReturnsTrue(string path)
    {
        _sut.IsSupportedFormat(path).Should().BeTrue();
    }

    [Theory] // TC-013
    [InlineData("/docs/readme.txt")]
    [InlineData("/docs/archive.zip")]
    [InlineData("/docs/noextension")]
    public void IsSupportedFormat_UnsupportedExtensions_ReturnsFalse(string path)
    {
        _sut.IsSupportedFormat(path).Should().BeFalse();
    }

    [Theory] // TC-014
    [InlineData("/music/song.MP3")]
    [InlineData("/music/song.Mp3")]
    [InlineData("/video/clip.MP4")]
    [InlineData("/video/clip.MKV")]
    public void IsSupportedFormat_UppercaseExtensions_ReturnsTrue(string path)
    {
        _sut.IsSupportedFormat(path).Should().BeTrue();
    }

    // ── DiscoverFilesAsync（临时目录）─────────────────────────────────────

    [Fact] // TC-017
    public async Task DiscoverFilesAsync_NonExistentDirectory_ReturnsEmpty()
    {
        var results = new List<string>();
        await foreach (var path in _sut.DiscoverFilesAsync("/nonexistent/path/xyz"))
            results.Add(path);

        results.Should().BeEmpty();
    }

    [Fact] // TC-018
    public async Task DiscoverFilesAsync_EmptyDirectory_ReturnsEmpty()
    {
        var results = new List<string>();
        await foreach (var path in _sut.DiscoverFilesAsync(_tempDir))
            results.Add(path);

        results.Should().BeEmpty();
    }

    [Fact] // TC-019
    public async Task DiscoverFilesAsync_DirectoryWithMediaFiles_ReturnsMediaFiles()
    {
        var mp3 = Path.Combine(_tempDir, "song.mp3");
        var mp4 = Path.Combine(_tempDir, "video.mp4");
        var txt = Path.Combine(_tempDir, "readme.txt");
        File.WriteAllText(mp3, "fake");
        File.WriteAllText(mp4, "fake");
        File.WriteAllText(txt, "fake");

        var results = new List<string>();
        await foreach (var path in _sut.DiscoverFilesAsync(_tempDir))
            results.Add(path);

        results.Should().HaveCount(2);
        results.Should().Contain(mp3);
        results.Should().Contain(mp4);
        results.Should().NotContain(txt);
    }

    [Fact] // TC-020
    public async Task DiscoverFilesAsync_CancelledToken_ThrowsOperationCancelled()
    {
        for (int i = 0; i < 5; i++)
            File.WriteAllText(Path.Combine(_tempDir, $"song{i}.mp3"), "fake");

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () =>
        {
            await foreach (var _ in _sut.DiscoverFilesAsync(_tempDir, ct: cts.Token)) { }
        };

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task DiscoverFilesAsync_RecursiveSubdirectory_FindsNestedFiles()
    {
        var subDir = Path.Combine(_tempDir, "sub");
        Directory.CreateDirectory(subDir);
        var nested = Path.Combine(subDir, "nested.mp3");
        File.WriteAllText(nested, "fake");

        var results = new List<string>();
        await foreach (var path in _sut.DiscoverFilesAsync(_tempDir, recursive: true))
            results.Add(path);

        results.Should().Contain(nested);
    }

    [Fact]
    public async Task DiscoverFilesAsync_NonRecursive_DoesNotFindNestedFiles()
    {
        var subDir = Path.Combine(_tempDir, "sub");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(subDir, "nested.mp3"), "fake");

        var results = new List<string>();
        await foreach (var path in _sut.DiscoverFilesAsync(_tempDir, recursive: false))
            results.Add(path);

        results.Should().BeEmpty();
    }

    // ── 真实目录集成测试（TC-R01 ~ TC-R05）──────────────────────────────

    [Fact] // TC-R01
    public async Task RealDir_RecursiveScan_FindsMediaFiles()
    {
        if (!Directory.Exists(RealTestDir)) return; // 目录不存在则跳过

        var results = new List<string>();
        await foreach (var path in _sut.DiscoverFilesAsync(RealTestDir, recursive: true))
            results.Add(path);

        results.Should().NotBeEmpty("真实目录中应包含媒体文件");
    }

    [Fact] // TC-R02
    public async Task RealDir_RecursiveScan_FindsMp3Files()
    {
        if (!Directory.Exists(RealTestDir)) return;

        var results = new List<string>();
        await foreach (var path in _sut.DiscoverFilesAsync(RealTestDir, recursive: true))
            results.Add(path);

        var mp3Files = results.Where(p => p.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)).ToList();
        mp3Files.Should().NotBeEmpty("目录中应有 .mp3 文件");
    }

    [Fact] // TC-R03
    public async Task RealDir_RecursiveScan_FindsVideoFiles()
    {
        if (!Directory.Exists(RealTestDir)) return;

        var results = new List<string>();
        await foreach (var path in _sut.DiscoverFilesAsync(RealTestDir, recursive: true))
            results.Add(path);

        var videoFiles = results.Where(p =>
            p.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
            p.EndsWith(".mov", StringComparison.OrdinalIgnoreCase)).ToList();

        videoFiles.Should().NotBeEmpty("目录中应有 .mp4 或 .mov 文件");
    }

    [Fact] // TC-R04
    public async Task RealDir_NonRecursiveScan_FindsNoFilesInRoot()
    {
        if (!Directory.Exists(RealTestDir)) return;

        // 根目录本身不含媒体文件，只有子目录
        var results = new List<string>();
        await foreach (var path in _sut.DiscoverFilesAsync(RealTestDir, recursive: false))
            results.Add(path);

        results.Should().BeEmpty("根目录下无直接媒体文件，文件都在月份子目录中");
    }

    [Fact] // TC-R05
    public async Task RealDir_RecursiveScan_ExcludesNonMediaFiles()
    {
        if (!Directory.Exists(RealTestDir)) return;

        var results = new List<string>();
        await foreach (var path in _sut.DiscoverFilesAsync(RealTestDir, recursive: true))
            results.Add(path);

        // 所有返回的文件都应是支持的格式
        results.Should().AllSatisfy(p =>
            _sut.IsSupportedFormat(p).Should().BeTrue($"{p} 应为支持的媒体格式"));

        // 不应包含 .pdf, .docx 等非媒体文件（图像文件 .jpg, .png 现在是支持的格式）
        results.Should().NotContain(p =>
            p.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ||
            p.EndsWith(".docx", StringComparison.OrdinalIgnoreCase));
    }
}
