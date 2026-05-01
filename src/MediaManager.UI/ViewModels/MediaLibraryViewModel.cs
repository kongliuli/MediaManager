using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MediaManager.Core.Enums;
using MediaManager.Core.Interfaces.Repositories;
using MediaManager.Core.Messages;
using MediaManager.Core.Models;
using MediaManager.UI.Views;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace MediaManager.UI.ViewModels;

public partial class MediaLibraryViewModel : ObservableObject, IRecipient<ScanCompletedMessage>
{
    private readonly ILibraryRepository _libraryRepository;
    private readonly IMediaRepository _mediaRepository;

    [ObservableProperty] private ObservableCollection<Library> _libraries = [];
    [ObservableProperty] private Library? _selectedLibrary;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _newLibraryName = string.Empty;
    [ObservableProperty] private string _newLibraryPath = string.Empty;
    [ObservableProperty] private bool _newLibraryIncludeAudio = true;
    [ObservableProperty] private bool _newLibraryIncludeVideo = true;
    [ObservableProperty] private bool _newLibraryIncludeImages = true;
    [ObservableProperty] private bool _newLibraryRecursive = true;
    [ObservableProperty] private Visibility _addPanelVisibility = Visibility.Collapsed;

    public MediaLibraryViewModel(ILibraryRepository libraryRepository, IMediaRepository mediaRepository)
    {
        _libraryRepository = libraryRepository;
        _mediaRepository = mediaRepository;
        WeakReferenceMessenger.Default.Register<ScanCompletedMessage>(this);
        _ = LoadLibrariesAsync();
    }

    public void Receive(ScanCompletedMessage message)
    {
        _ = LoadLibrariesAsync();
    }

    [RelayCommand]
    public async Task LoadLibrariesAsync()
    {
        IsLoading = true;
        try
        {
            var libraries = await _libraryRepository.GetAllAsync();
            Libraries.Clear();
            foreach (var lib in libraries)
            {
                // 加载统计信息
                lib.FileCount = await GetLibraryFileCountAsync(lib.Id);
                Libraries.Add(lib);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task<int> GetLibraryFileCountAsync(int libraryId)
    {
        try
        {
            var query = new Core.DTOs.MediaSearchQuery
            {
                LibraryId = libraryId,
                PageSize = 1
            };
            var result = await _mediaRepository.SearchAsync(query);
            return result.TotalCount;
        }
        catch
        {
            return 0;
        }
    }

    [RelayCommand]
    public void ShowAddPanel()
    {
        AddPanelVisibility = Visibility.Visible;
        NewLibraryName = string.Empty;
        NewLibraryPath = string.Empty;
    }

    [RelayCommand]
    public void HideAddPanel()
    {
        AddPanelVisibility = Visibility.Collapsed;
        NewLibraryName = string.Empty;
        NewLibraryPath = string.Empty;
    }

    [RelayCommand]
    public void BrowsePath()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "选择媒体库扫描目录",
            SelectedPath = NewLibraryPath
        };
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            NewLibraryPath = dialog.SelectedPath;
            if (string.IsNullOrEmpty(NewLibraryName))
            {
                NewLibraryName = new DirectoryInfo(dialog.SelectedPath).Name;
            }
        }
    }

    [RelayCommand]
    public async Task CreateLibraryAsync()
    {
        if (string.IsNullOrWhiteSpace(NewLibraryName))
        {
            MessageBox.Show("请输入媒体库名称", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(NewLibraryPath))
        {
            MessageBox.Show("请选择扫描目录", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsLoading = true;
        try
        {
            var library = new Library
            {
                Name = NewLibraryName.Trim(),
                ScanPath = NewLibraryPath,
                IncludeAudio = NewLibraryIncludeAudio,
                IncludeVideo = NewLibraryIncludeVideo,
                IncludeImages = NewLibraryIncludeImages,
                Recursive = NewLibraryRecursive,
                Status = LibraryStatus.Idle,
                CreatedAt = DateTime.UtcNow,
                LastScannedAt = null
            };

            await _libraryRepository.CreateAsync(library);
            await LoadLibrariesAsync();
            HideAddPanel();

            HandyControl.Controls.Growl.Success($"媒体库 \"{library.Name}\" 创建成功");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"创建失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task DeleteLibraryAsync(Library? library)
    {
        var targetLibrary = library ?? SelectedLibrary;
        if (targetLibrary == null) return;

        var result = MessageBox.Show(
            $"确定要删除媒体库 \"{targetLibrary.Name}\" 吗？\n此操作不会删除关联的媒体文件记录。",
            "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        IsLoading = true;
        try
        {
            await _libraryRepository.DeleteAsync(targetLibrary.Id);
            await LoadLibrariesAsync();
            SelectedLibrary = null;
            HandyControl.Controls.Growl.Success("媒体库已删除");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"删除失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void StartIncrementalScan(Library? library)
    {
        var targetLibrary = library ?? SelectedLibrary;
        if (targetLibrary == null) return;

        // 发送消息到 ScanViewModel 进行增量扫描
        WeakReferenceMessenger.Default.Send(new StartIncrementalScanMessage(targetLibrary));

        // 切换到扫描页面
        WeakReferenceMessenger.Default.Send(new NavigateMessage(typeof(ScanViewModel)));
    }

    [RelayCommand]
    public void ViewLibraryFiles(Library? library)
    {
        var targetLibrary = library ?? SelectedLibrary;
        if (targetLibrary == null) return;

        // 发送消息到 LibraryViewModel 过滤显示该媒体库的文件
        WeakReferenceMessenger.Default.Send(new FilterByLibraryMessage(targetLibrary.Id));
        
        // 切换到媒体文件页面
        WeakReferenceMessenger.Default.Send(new NavigateMessage(typeof(LibraryViewModel)));
    }
}

/// <summary>
/// 消息：开始增量扫描
/// </summary>
public class StartIncrementalScanMessage
{
    public Library Library { get; }

    public StartIncrementalScanMessage(Library library)
    {
        Library = library;
    }
}

/// <summary>
/// 消息：按媒体库过滤
/// </summary>
public class FilterByLibraryMessage
{
    public int LibraryId { get; }

    public FilterByLibraryMessage(int libraryId)
    {
        LibraryId = libraryId;
    }
}
