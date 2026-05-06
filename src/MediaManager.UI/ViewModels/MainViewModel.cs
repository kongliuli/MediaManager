using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MediaManager.UI.DisplayModels;
using Microsoft.Extensions.DependencyInjection;

namespace MediaManager.UI.ViewModels;

public partial class MainViewModel : ObservableObject,
    IRecipient<MediaSelectedMessage>,
    IRecipient<NavigateMessage>
{
    private readonly IServiceProvider _services;

    [ObservableProperty] private ObservableObject? _currentViewModel;
    [ObservableProperty] private bool _isDetailPanelVisible;

    public DetailViewModel DetailViewModel { get; }

    public MainViewModel(IServiceProvider services, DetailViewModel detailViewModel)
    {
        _services = services;
        DetailViewModel = detailViewModel;
        WeakReferenceMessenger.Default.RegisterAll(this);
        NavigateTo<LibraryViewModel>();
    }

    public async void Receive(MediaSelectedMessage message)
    {
        await DetailViewModel.LoadAsync(message.Item);
        IsDetailPanelVisible = true;
    }

    public void Receive(NavigateMessage message)
    {
        CurrentViewModel = (ObservableObject)_services.GetRequiredService(message.ViewModelType);
    }

    [RelayCommand] public void NavigateLibrary()        => NavigateTo<LibraryViewModel>();
    [RelayCommand] public void NavigateMediaLibrary()  => NavigateTo<MediaLibraryViewModel>();
    [RelayCommand] public void NavigateScan()          => NavigateTo<ScanViewModel>();
    [RelayCommand] public void NavigateSearch()        => NavigateTo<SearchViewModel>();
    [RelayCommand] public void NavigatePlaylist()      => NavigateTo<PlaylistViewModel>();
    [RelayCommand] public void NavigateDuplicate()     => NavigateTo<DuplicateViewModel>();
    [RelayCommand] public void NavigateCache()         => NavigateTo<CacheViewModel>();
    [RelayCommand] public void NavigatePerformance()   => NavigateTo<PerformanceViewModel>();
    [RelayCommand] public void NavigateSettings()      => NavigateTo<SettingsViewModel>();
    [RelayCommand] public void CloseDetail()      => IsDetailPanelVisible = false;

    private void NavigateTo<T>() where T : ObservableObject
        => CurrentViewModel = _services.GetRequiredService<T>();
}
