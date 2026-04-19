using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaManager.Core.Interfaces.Services;
using MediaManager.Core.Models;
using MediaManager.UI.DisplayModels;
using System.Collections.ObjectModel;

namespace MediaManager.UI.ViewModels;

public partial class DuplicateViewModel : ObservableObject
{
    private readonly IDuplicateDetectionService _service;

    [ObservableProperty] private ObservableCollection<DuplicateGroup> _groups = [];
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private long _totalReclaimable;

    public DuplicateViewModel(IDuplicateDetectionService service)
    {
        _service = service;
    }

    [RelayCommand]
    public async Task ScanDuplicatesAsync()
    {
        IsLoading = true;
        try
        {
            var result = await _service.FindDuplicatesAsync();
            Groups.Clear();
            foreach (var g in result)
                Groups.Add(g);
            TotalReclaimable = result.Sum(g => g.ReclaimableSize);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
