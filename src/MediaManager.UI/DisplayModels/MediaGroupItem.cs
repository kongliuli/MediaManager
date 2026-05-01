using CommunityToolkit.Mvvm.ComponentModel;
using MediaManager.Core.Enums;
using MediaManager.UI.ViewModels;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace MediaManager.UI.DisplayModels;

public partial class MediaGroupItem : ObservableObject
{
    [ObservableProperty] private MediaType _mediaType;
    [ObservableProperty] private string _groupName;
    [ObservableProperty] private ObservableCollection<MediaFileDisplayItem> _items = [];
    [ObservableProperty] private bool _isExpanded = true;

    public int ItemCount => Items.Count;

    public MediaGroupItem(MediaType mediaType, string groupName)
    {
        MediaType = mediaType;
        GroupName = groupName;
        Items.CollectionChanged += Items_CollectionChanged;
    }

    private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(ItemCount));
    }
}