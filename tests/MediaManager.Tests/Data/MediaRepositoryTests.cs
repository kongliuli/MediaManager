using FluentAssertions;
using MediaManager.Core.DTOs;
using MediaManager.Core.Enums;
using Xunit;
using MediaManager.Core.Models;
using MediaManager.Data.Context;
using MediaManager.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MediaManager.Tests.Data;

/// <summary>
/// TC-031 ~ TC-040: MediaRepository 集成测试（使用 EF InMemory）
/// </summary>
public class MediaRepositoryTests : IDisposable
{
    private readonly MediaDbContext _dbContext;
    private readonly MediaRepository _sut;

    public MediaRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<MediaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new MediaDbContext(options);
        _sut = new MediaRepository(_dbContext);
    }

    public void Dispose() => _dbContext.Dispose();

    private static AudioFile MakeAudio(string path = "/music/test.mp3", string hash = "abc123", long size = 1000, double duration = 180) => new()
    {
        Path = path,
        FileName = System.IO.Path.GetFileName(path),
        Hash = hash,
        FileSize = size,
        DurationSeconds = duration,
        DateAdded = DateTime.UtcNow,
        LastModified = DateTime.UtcNow,
        MediaType = MediaType.Audio
    };

    // ── Upsert ─────────────────────────────────────────────────────────────

    [Fact] // TC-031
    public async Task UpsertAsync_NewFile_InsertsRecord()
    {
        var file = MakeAudio("/music/new.mp3");

        await _sut.UpsertAsync(file);

        var found = await _sut.GetByPathAsync("/music/new.mp3");
        found.Should().NotBeNull();
        found!.FileName.Should().Be("new.mp3");
    }

    [Fact] // TC-032
    public async Task UpsertAsync_ExistingPath_UpdatesRecord()
    {
        var file = MakeAudio("/music/existing.mp3");
        await _sut.UpsertAsync(file);

        // 修改 Rating 后再次 Upsert
        file.Rating = 5;
        await _sut.UpsertAsync(file);

        var found = await _sut.GetByPathAsync("/music/existing.mp3");
        found!.Rating.Should().Be(5);
    }

    // ── GetById ────────────────────────────────────────────────────────────

    [Fact] // TC-033
    public async Task GetByIdAsync_ExistingId_ReturnsFile()
    {
        var file = MakeAudio("/music/byid.mp3");
        await _sut.UpsertAsync(file);
        var inserted = await _sut.GetByPathAsync("/music/byid.mp3");

        var result = await _sut.GetByIdAsync(inserted!.Id);

        result.Should().NotBeNull();
        result!.Path.Should().Be("/music/byid.mp3");
    }

    [Fact] // TC-034
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        var result = await _sut.GetByIdAsync(999999);

        result.Should().BeNull();
    }

    // ── GetByPath ──────────────────────────────────────────────────────────

    [Fact] // TC-035
    public async Task GetByPathAsync_ExistingPath_ReturnsFile()
    {
        await _sut.UpsertAsync(MakeAudio("/music/bypath.mp3"));

        var result = await _sut.GetByPathAsync("/music/bypath.mp3");

        result.Should().NotBeNull();
    }

    // ── Delete ─────────────────────────────────────────────────────────────

    [Fact] // TC-036
    public async Task DeleteAsync_ExistingFile_RemovesRecord()
    {
        var file = MakeAudio("/music/delete.mp3");
        await _sut.UpsertAsync(file);
        var inserted = await _sut.GetByPathAsync("/music/delete.mp3");

        await _sut.DeleteAsync(inserted!.Id);

        var found = await _sut.GetByPathAsync("/music/delete.mp3");
        found.Should().BeNull();
    }

    // ── Search ─────────────────────────────────────────────────────────────

    [Fact] // TC-037
    public async Task SearchAsync_KeywordFilter_ReturnsMatchingFiles()
    {
        await _sut.UpsertAsync(MakeAudio("/music/testSong.mp3"));
        await _sut.UpsertAsync(MakeAudio("/music/otherSong.mp3"));

        var query = new MediaSearchQuery { Keyword = "testSong" };
        var result = await _sut.SearchAsync(query);

        result.Items.Should().HaveCount(1);
        result.Items[0].FileName.Should().Be("testSong.mp3");
    }

    [Fact] // TC-038
    public async Task SearchAsync_Pagination_ReturnsCorrectPage()
    {
        for (int i = 1; i <= 5; i++)
            await _sut.UpsertAsync(MakeAudio($"/music/file{i:D2}.mp3"));

        var query = new MediaSearchQuery
        {
            Page = 2,
            PageSize = 2,
            SortBy = SortField.FileName,
            SortDescending = false
        };
        var result = await _sut.SearchAsync(query);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.Items[0].FileName.Should().Be("file03.mp3");
        result.Items[1].FileName.Should().Be("file04.mp3");
    }

    [Fact] // TC-039
    public async Task SearchAsync_SortByDurationDescending_ReturnsSortedResults()
    {
        await _sut.UpsertAsync(MakeAudio("/music/short.mp3", duration: 60));
        await _sut.UpsertAsync(MakeAudio("/music/long.mp3", duration: 300));
        await _sut.UpsertAsync(MakeAudio("/music/medium.mp3", duration: 180));

        var query = new MediaSearchQuery
        {
            SortBy = SortField.Duration,
            SortDescending = true,
            PageSize = 10
        };
        var result = await _sut.SearchAsync(query);

        result.Items[0].DurationSeconds.Should().Be(300);
        result.Items[1].DurationSeconds.Should().Be(180);
        result.Items[2].DurationSeconds.Should().Be(60);
    }

    [Fact] // TC-040
    public async Task GetAllHashesAsync_FilesWithEmptyHash_ExcludesEmptyHashes()
    {
        var withHash = MakeAudio("/music/hashed.mp3", hash: "validhash");
        var withoutHash = MakeAudio("/music/nohash.mp3", hash: "");
        await _sut.UpsertAsync(withHash);
        await _sut.UpsertAsync(withoutHash);

        var hashes = await _sut.GetAllHashesAsync();

        hashes.Should().HaveCount(1);
        hashes[0].Hash.Should().Be("validhash");
    }

    [Fact]
    public async Task SearchAsync_MediaTypeFilter_ReturnsOnlyMatchingType()
    {
        await _sut.UpsertAsync(MakeAudio("/music/audio.mp3"));
        await _dbContext.MediaFiles.AddAsync(new VideoFile
        {
            Path = "/video/clip.mp4",
            FileName = "clip.mp4",
            Hash = "videohash",
            FileSize = 5000,
            DurationSeconds = 120,
            DateAdded = DateTime.UtcNow,
            LastModified = DateTime.UtcNow,
            MediaType = MediaType.Video
        });
        await _dbContext.SaveChangesAsync();

        var query = new MediaSearchQuery { MediaType = MediaType.Audio };
        var result = await _sut.SearchAsync(query);

        result.Items.Should().AllSatisfy(f => f.MediaType.Should().Be(MediaType.Audio));
    }

    [Fact]
    public async Task SearchAsync_DurationRangeFilter_ReturnsFilesInRange()
    {
        await _sut.UpsertAsync(MakeAudio("/music/short.mp3", duration: 30));
        await _sut.UpsertAsync(MakeAudio("/music/medium.mp3", duration: 180));
        await _sut.UpsertAsync(MakeAudio("/music/long.mp3", duration: 600));

        var query = new MediaSearchQuery
        {
            MinDurationSeconds = 60,
            MaxDurationSeconds = 300
        };
        var result = await _sut.SearchAsync(query);

        result.Items.Should().HaveCount(1);
        result.Items[0].DurationSeconds.Should().Be(180);
    }

    [Fact]
    public async Task DeleteRangeAsync_MultipleIds_RemovesAllRecords()
    {
        await _sut.UpsertAsync(MakeAudio("/music/a.mp3"));
        await _sut.UpsertAsync(MakeAudio("/music/b.mp3"));
        await _sut.UpsertAsync(MakeAudio("/music/c.mp3"));

        var all = await _sut.SearchAsync(new MediaSearchQuery { PageSize = 100 });
        var ids = all.Items.Select(f => f.Id).ToList();

        await _sut.DeleteRangeAsync(ids);

        var remaining = await _sut.SearchAsync(new MediaSearchQuery { PageSize = 100 });
        remaining.TotalCount.Should().Be(0);
    }
}
