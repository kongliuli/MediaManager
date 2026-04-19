using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MediaManager.Core.DTOs;
using MediaManager.Core.Enums;
using MediaManager.Core.Interfaces.Repositories;
using MediaManager.UI.DisplayModels;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace MediaManager.UI.ViewModels;

public partial class SearchViewModel : ObservableObject
{
    private readonly IMediaRepository _repo;

    [ObservableProperty] private string _keyword = string.Empty;
    [ObservableProperty] private MediaType? _mediaTypeFilter;
    [ObservableProperty] private double? _minDuration;
    [ObservableProperty] private double? _maxDuration;
    [ObservableProperty] private ObservableCollection<MediaFileDisplayItem> _results = [];
    [ObservableProperty] private MediaFileDisplayItem? _selectedItem;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private int _totalCount;

    public SearchViewModel(IMediaRepository repo)
    {
        _repo = repo;
    }

    [RelayCommand]
    public async Task SearchAsync()
    {
        IsLoading = true;
        try
        {
            var query = new MediaSearchQuery
            {
                Keyword = string.IsNullOrWhiteSpace(Keyword) ? null : Keyword,
                MediaType = MediaTypeFilter,
                MinDurationSeconds = MinDuration,
                MaxDurationSeconds = MaxDuration,
                SortBy = SortField.DateAdded,
                SortDescending = true,
                PageSize = 100
            };

            var result = await _repo.SearchAsync(query);
            TotalCount = result.TotalCount;
            Results.Clear();
            foreach (var f in result.Items)
                Results.Add(new MediaFileDisplayItem(f));
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSelectedItemChanged(MediaFileDisplayItem? value)
    {
        if (value != null)
            WeakReferenceMessenger.Default.Send(new MediaSelectedMessage(value));
    }
}
