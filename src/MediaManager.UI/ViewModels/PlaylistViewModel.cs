using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaManager.Core.Interfaces.Repositories;
using MediaManager.Core.Models;
using System.Collections.ObjectModel;

namespace MediaManager.UI.ViewModels;

public partial class PlaylistViewModel : ObservableObject
{
    private readonly IPlaylistRepository _repo;

    [ObservableProperty] private ObservableCollection<Playlist> _playlists = [];
    [ObservableProperty] private Playlist? _selectedPlaylist;
    [ObservableProperty] private string _newPlaylistName = string.Empty;

    public PlaylistViewModel(IPlaylistRepository repo)
    {
        _repo = repo;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        var all = await _repo.GetAllAsync();
        Playlists.Clear();
        foreach (var p in all) Playlists.Add(p);
    }

    [RelayCommand]
    public async Task CreatePlaylistAsync()
    {
        if (string.IsNullOrWhiteSpace(NewPlaylistName)) return;
        var playlist = await _repo.CreateAsync(new Playlist { Name = NewPlaylistName });
        Playlists.Add(playlist);
        NewPlaylistName = string.Empty;
    }

    [RelayCommand]
    public async Task DeletePlaylistAsync(Playlist playlist)
    {
        await _repo.DeleteAsync(playlist.Id);
        Playlists.Remove(playlist);
        if (SelectedPlaylist == playlist) SelectedPlaylist = null;
    }
}
