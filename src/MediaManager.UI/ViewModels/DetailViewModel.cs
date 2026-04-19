using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaManager.Core.DTOs;
using MediaManager.Core.Interfaces.Repositories;
using MediaManager.Core.Interfaces.Services;
using MediaManager.Core.Models;
using MediaManager.UI.DisplayModels;
using NAudio.Wave;
using System.IO;
using System.Windows.Media.Imaging;
using File = System.IO.File;
using Path = System.IO.Path;

namespace MediaManager.UI.ViewModels;

public partial class DetailViewModel : ObservableObject, IDisposable
{
    private readonly IMediaRepository _repo;
    private readonly IHashService _hashService;
    private readonly IMetadataService _metadataService;
    private WaveOutEvent? _waveOut;
    private AudioFileReader? _audioReader;

    [ObservableProperty] private MediaFileDisplayItem? _currentItem;
    [ObservableProperty] private bool _isAudio;
    [ObservableProperty] private bool _isVideo;
    [ObservableProperty] private bool _isImage;
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private double _playbackPosition;
    [ObservableProperty] private double _playbackDuration;
    [ObservableProperty] private BitmapSource? _previewImage;
    [ObservableProperty] private bool _isRecalculating;

    public DetailViewModel(IMediaRepository repo, IHashService hashService, IMetadataService metadataService)
    {
        _repo = repo;
        _hashService = hashService;
        _metadataService = metadataService;
    }

    public async Task LoadAsync(MediaFileDisplayItem item)
    {
        StopPlayback();
        CurrentItem = item;

        IsAudio = item.Source is AudioFile;
        IsVideo = item.Source is VideoFile;
        IsImage = false;
        PreviewImage = null;

        // 加载预览图
        if (item.Source is VideoFile v && !string.IsNullOrEmpty(v.ThumbnailPath))
            await LoadImageAsync(v.ThumbnailPath);
        else if (item.Source is AudioFile a && !string.IsNullOrEmpty(a.AlbumArtPath))
            await LoadImageAsync(a.AlbumArtPath);

        // 检查是否为图片文件
        var ext = Path.GetExtension(item.Path).ToLowerInvariant();
        if (ext is ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp")
        {
            IsImage = true;
            await LoadImageAsync(item.Path);
        }
    }

    private async Task LoadImageAsync(string path)
    {
        if (!File.Exists(path)) return;
        
        try
        {
            var bmp = await Task.Run(() =>
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path);
                bitmap.DecodePixelWidth = 600;
                bitmap.DecodePixelHeight = 450;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            });
            
            PreviewImage = bmp;
        }
        catch (Exception ex)
        {
            // 静默忽略，记录日志
            System.Diagnostics.Debug.WriteLine($"图片加载失败: {ex.Message}");
        }
    }

    [RelayCommand]
    public void TogglePlayback()
    {
        if (IsPlaying)
            PausePlayback();
        else
            StartPlayback();
    }

    [RelayCommand]
    public void StopPlayback()
    {
        _waveOut?.Stop();
        _waveOut?.Dispose();
        _audioReader?.Dispose();
        _waveOut = null;
        _audioReader = null;
        IsPlaying = false;
        PlaybackPosition = 0;
    }

    private void StartPlayback()
    {
        if (CurrentItem?.Source is not AudioFile || !File.Exists(CurrentItem.Path)) return;
        try
        {
            if (_waveOut == null)
            {
                _audioReader = new AudioFileReader(CurrentItem.Path);
                PlaybackDuration = _audioReader.TotalTime.TotalSeconds;
                _waveOut = new WaveOutEvent();
                _waveOut.Init(_audioReader);
                _waveOut.PlaybackStopped += (_, _) =>
                {
                    IsPlaying = false;
                    PlaybackPosition = 0;
                };
            }
            _waveOut.Play();
            IsPlaying = true;
            _ = TrackPositionAsync();
        }
        catch { /* 播放失败静默忽略 */ }
    }

    private void PausePlayback()
    {
        _waveOut?.Pause();
        IsPlaying = false;
    }

    private async Task TrackPositionAsync()
    {
        while (_waveOut?.PlaybackState == PlaybackState.Playing)
        {
            PlaybackPosition = _audioReader?.CurrentTime.TotalSeconds ?? 0;
            await Task.Delay(500);
        }
    }

    [RelayCommand]
    public async Task DeleteFileAsync()
    {
        if (CurrentItem == null) return;
        StopPlayback();
        await _repo.DeleteAsync(CurrentItem.Source.Id);
        CurrentItem = null;
    }

    [RelayCommand]
    public async Task RecalculateAsync()
    {
        if (CurrentItem == null) return;
        
        IsRecalculating = true;
        try
        {
            var mediaFile = CurrentItem.Source;
            var filePath = mediaFile.Path;

            if (!File.Exists(filePath))
            {
                System.Windows.MessageBox.Show("文件不存在！", "错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            // 重新计算哈希
            var newHash = await _hashService.ComputeAsync(filePath);
            mediaFile.Hash = newHash;

            // 更新文件大小和修改时间
            var fileInfo = new FileInfo(filePath);
            mediaFile.FileSize = fileInfo.Length;
            mediaFile.LastModified = fileInfo.LastWriteTimeUtc;

            // 尝试重新提取元数据
            try
            {
                var metadata = await _metadataService.ExtractAsync(filePath);
                if (metadata != null)
                {
                    mediaFile.DurationSeconds = metadata.DurationSeconds;
                    mediaFile.Width = metadata.Width;
                    mediaFile.Height = metadata.Height;

                    if (mediaFile is VideoFile video)
                    {
                        video.FrameRate = metadata.FrameRate;
                        video.VideoCodec = metadata.VideoCodec ?? string.Empty;
                        video.AudioCodec = metadata.AudioCodec ?? string.Empty;
                        video.BitRate = metadata.VideoBitRate;
                        video.Title = metadata.Title;
                    }
                    else if (mediaFile is AudioFile audio)
                    {
                        audio.BitRate = metadata.BitRate;
                        audio.SampleRate = metadata.SampleRate;
                        audio.Channels = metadata.Channels;
                        audio.Codec = metadata.AudioCodec ?? string.Empty;
                        audio.Title = metadata.Title;
                        audio.Artist = metadata.Artist;
                        audio.Album = metadata.Album;
                        audio.Year = metadata.Year;
                        audio.Genre = metadata.Genre;
                    }
                    else if (mediaFile is ImageFile image)
                    {
                        // 从文件扩展名获取格式
                        image.Format = System.IO.Path.GetExtension(filePath).TrimStart('.').ToUpper();
                    }
                }
            }
            catch (Exception ex)
            {
                // 元数据提取失败，继续保存其他信息
                System.Diagnostics.Debug.WriteLine($"元数据提取失败: {ex.Message}");
            }

            // 保存到数据库
            await _repo.UpsertAsync(mediaFile);

            // 重新加载预览
            await LoadAsync(CurrentItem);

            System.Windows.MessageBox.Show("重新计算完成！", "成功", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"重新计算失败: {ex.Message}", "错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            IsRecalculating = false;
        }
    }

    public void Dispose()
    {
        StopPlayback();
    }
}
