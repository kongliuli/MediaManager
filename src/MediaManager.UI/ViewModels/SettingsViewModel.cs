using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaManager.Core;

namespace MediaManager.UI.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly AppConfig _config;

    [ObservableProperty] private string _thumbnailDirectory;
    [ObservableProperty] private bool _scanRecursive = true;
    [ObservableProperty] private int _thumbnailSize = 160;

    public SettingsViewModel(AppConfig config)
    {
        _config = config;
        _thumbnailDirectory = config.ThumbnailDirectory;
    }

    [RelayCommand]
    public void BrowseThumbnailDirectory()
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "选择缩略图存储目录",
            SelectedPath = ThumbnailDirectory
        };
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            ThumbnailDirectory = dialog.SelectedPath;
            _config.ThumbnailDirectory = dialog.SelectedPath;
        }
    }

    [RelayCommand]
    public void Save()
    {
        _config.ThumbnailDirectory = ThumbnailDirectory;
        HandyControl.Controls.Growl.Success("设置已保存");
    }
}
