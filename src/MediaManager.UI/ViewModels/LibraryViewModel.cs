using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MediaManager.Core.DTOs;
using MediaManager.Core.Enums;
using MediaManager.Core.Interfaces.Repositories;
using MediaManager.Core.Messages;
using MediaManager.Core.Models;
using MediaManager.UI.DisplayModels;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Application = System.Windows.Application;
using File = System.IO.File;

namespace MediaManager.UI.ViewModels;

public partial class LibraryViewModel : ObservableObject, 
    IRecipient<FilterByLibraryMessage>,
    IRecipient<ScanCompletedMessage>
{
    private readonly IMediaRepository _repo;
    private readonly ILogger<LibraryViewModel> _logger;

    [ObservableProperty] private ObservableCollection<MediaFileDisplayItem> _items = [];
    [ObservableProperty] private ObservableCollection<MediaGroupItem> _groupedItems = [];
    [ObservableProperty] private MediaFileDisplayItem? _selectedItem;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private SortField _sortBy = SortField.DateAdded;
    
    [ObservableProperty] private int? _filterLibraryId;

    public LibraryViewModel(IMediaRepository repo, ILogger<LibraryViewModel> logger)
    {
        _repo = repo;
        _logger = logger;
        _logger.LogInformation("LibraryViewModel 初始化");
        WeakReferenceMessenger.Default.Register<FilterByLibraryMessage>(this);
        WeakReferenceMessenger.Default.Register<ScanCompletedMessage>(this);
        _ = LoadAsync();
    }

    public void Receive(FilterByLibraryMessage message)
    {
        FilterLibraryId = message.LibraryId;
        _ = LoadAsync();
    }

    public void Receive(ScanCompletedMessage message)
    {
        _logger.LogInformation("收到扫描完成消息，准备刷新数据");
        _ = LoadAsync();
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        _logger.LogInformation("LoadAsync 开始执行 - SearchText: {SearchText}, SortBy: {SortBy}, FilterLibraryId: {FilterLibraryId}", SearchText, SortBy, FilterLibraryId);
        IsLoading = true;
        try
        {
            var result = await _repo.SearchAsync(new MediaSearchQuery
            {
                Keyword = SearchText,
                SortBy = SortBy,
                SortDescending = true,
                PageSize = 200,
                LibraryId = FilterLibraryId
            });

            _logger.LogInformation("SearchAsync 返回 {Count} 条记录, 总数: {Total}", result.Items.Count, result.TotalCount);

            Items.Clear();
            var displayItems = new List<MediaFileDisplayItem>();
            foreach (var file in result.Items)
            {
                var item = new MediaFileDisplayItem(file);
                Items.Add(item);
                displayItems.Add(item);
                LoadThumbnailAsync(item);
            }
            _logger.LogInformation("Items 集合已更新, 当前数量: {Count}", Items.Count);

            // 按类型分组
            GroupByMediaType(displayItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LoadAsync 执行失败");
            throw;
        }
        finally
        {
            IsLoading = false;
            _logger.LogInformation("LoadAsync 执行完成");
        }
    }

    private void GroupByMediaType(List<MediaFileDisplayItem> items)
    {
        GroupedItems.Clear();
        
        var groups = items.GroupBy(item => item.Source.MediaType)
                         .OrderBy(g => g.Key);

        foreach (var group in groups)
        {
            var groupName = GetMediaTypeGroupName(group.Key);
            var groupItem = new MediaGroupItem(group.Key, groupName);
            
            foreach (var item in group)
            {
                groupItem.Items.Add(item);
            }
            
            GroupedItems.Add(groupItem);
        }
    }

    private string GetMediaTypeGroupName(MediaType type)
    {
        return type switch
        {
            MediaType.Audio => "音频",
            MediaType.Video => "视频",
            MediaType.Image => "图片",
            MediaType.Other => "其他",
            _ => "未知"
        };
    }

    partial void OnSelectedItemChanged(MediaFileDisplayItem? value)
    {
        if (value != null)
            WeakReferenceMessenger.Default.Send(new MediaSelectedMessage(value));
    }

    partial void OnSearchTextChanged(string value) => _ = LoadAsync();

    private void LoadThumbnailAsync(MediaFileDisplayItem item)
    {
        _ = Task.Run(() =>
        {
            string? imgPath = item.Source switch
            {
                VideoFile v => v.ThumbnailPath,
                AudioFile a => a.AlbumArtPath,
                ImageFile i => item.Path,
                _ => null
            };

            if (string.IsNullOrEmpty(imgPath) || !File.Exists(imgPath)) return;

            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(imgPath);
                bmp.DecodePixelWidth = 160;
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();

                Application.Current.Dispatcher.InvokeAsync(() => item.Thumbnail = bmp);
            }
            catch { /* 缩略图加载失败静默忽略 */ }
        });
    }
}
