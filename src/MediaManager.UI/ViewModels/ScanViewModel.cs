using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MediaManager.Core.DTOs;
using MediaManager.Core.Enums;
using MediaManager.Core.Interfaces.Services;
using MediaManager.Core.Models;
using MediaManager.Services.Scanning;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace MediaManager.UI.ViewModels;

public partial class ScanViewModel : ObservableObject, IRecipient<StartIncrementalScanMessage>
{
    private readonly ScanPipelineOrchestrator _orchestrator;
    private readonly IDuplicateDetectionService _duplicateDetectionService;

    [ObservableProperty] 
    private string _scanDirectory = string.Empty;

    partial void OnScanDirectoryChanged(string? oldValue, string newValue)
    {
        StartScanCommand.NotifyCanExecuteChanged();
    }
    
    [ObservableProperty] private ScanProgressReport _progress = new();
    [ObservableProperty] private ObservableCollection<ScanLogMessage> _logMessages = [];
    [ObservableProperty] private bool _isScanning;
    
    // 扫描选项
    [ObservableProperty] private bool _includeAudio = true;
    [ObservableProperty] private bool _includeVideo = true;
    [ObservableProperty] private bool _includeImages = false;
    [ObservableProperty] private bool _recursive = true;

    // 当前关联的媒体库（用于增量扫描）
    [ObservableProperty] private Library? _currentLibrary;
    
    // 是否为增量扫描模式
    [ObservableProperty] private bool _isIncrementalScan;

    partial void OnIncludeAudioChanged(bool value) => StartScanCommand.NotifyCanExecuteChanged();
    partial void OnIncludeVideoChanged(bool value) => StartScanCommand.NotifyCanExecuteChanged();
    partial void OnIncludeImagesChanged(bool value) => StartScanCommand.NotifyCanExecuteChanged();

    // 日志筛选属性
    [ObservableProperty] private LogMessageType? _selectedMessageType;
    [ObservableProperty] private MediaType? _selectedMediaType;
    [ObservableProperty] private string _selectedFileExtension = string.Empty;
    
    // 筛选后的日志视图
    public ICollectionView FilteredLogView { get; }

    // 可用的筛选选项
    public LogMessageType[] AvailableMessageTypes { get; } = (LogMessageType[])Enum.GetValues(typeof(LogMessageType));
    public MediaType[] AvailableMediaTypes { get; } = (MediaType[])Enum.GetValues(typeof(MediaType));
    
    // 已发现的文件扩展名列表（用于筛选）
    [ObservableProperty] private ObservableCollection<string> _discoveredExtensions = [];
    
    // 重复文件检测相关
    [ObservableProperty] private bool _showDuplicates;
    [ObservableProperty] private bool _isDetectingDuplicates;
    [ObservableProperty] private ObservableCollection<DuplicateGroup> _duplicateGroups = [];
    [ObservableProperty] private long _totalReclaimableSpace;

    private CancellationTokenSource? _cts;

    public ScanViewModel(ScanPipelineOrchestrator orchestrator, IDuplicateDetectionService duplicateDetectionService)
    {
        _orchestrator = orchestrator;
        _duplicateDetectionService = duplicateDetectionService;
        FilteredLogView = CollectionViewSource.GetDefaultView(LogMessages);
        FilteredLogView.Filter = FilterLogMessages;
        
        // 订阅增量扫描消息
        WeakReferenceMessenger.Default.Register(this);
    }

    public void Receive(StartIncrementalScanMessage message)
    {
        // 设置增量扫描模式
        IsIncrementalScan = true;
        CurrentLibrary = message.Library;
        
        // 使用媒体库的扫描路径和选项
        ScanDirectory = message.Library.ScanPath;
        IncludeAudio = message.Library.IncludeAudio;
        IncludeVideo = message.Library.IncludeVideo;
        IncludeImages = message.Library.IncludeImages;
        Recursive = message.Library.Recursive;
        
        // 自动开始扫描
        _ = StartScanAsync();
    }

    partial void OnSelectedMessageTypeChanged(LogMessageType? value) => FilteredLogView.Refresh();
    partial void OnSelectedMediaTypeChanged(MediaType? value) => FilteredLogView.Refresh();
    partial void OnSelectedFileExtensionChanged(string value) => FilteredLogView.Refresh();

    private bool FilterLogMessages(object item)
    {
        if (item is not ScanLogMessage log)
            return false;

        // 按消息类型筛选
        if (SelectedMessageType.HasValue && log.Type != SelectedMessageType.Value)
            return false;

        // 按媒体类型筛选
        if (SelectedMediaType.HasValue && log.MediaType != SelectedMediaType.Value)
            return false;

        // 按文件扩展名筛选
        if (!string.IsNullOrWhiteSpace(SelectedFileExtension) && 
            !string.Equals(log.FileExtension, SelectedFileExtension, StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }

    [RelayCommand]
    public void ClearFilters()
    {
        SelectedMessageType = null;
        SelectedMediaType = null;
        SelectedFileExtension = string.Empty;
    }

    [RelayCommand]
    public void BrowseDirectory()
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "选择要扫描的媒体目录",
            UseDescriptionForTitle = true
        };
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            ScanDirectory = dialog.SelectedPath;
    }

    [RelayCommand(CanExecute = nameof(CanScan))]
    public async Task StartScanAsync()
    {
        _cts = new CancellationTokenSource();
        IsScanning = true;
        LogMessages.Clear();
        DiscoveredExtensions.Clear();

        var progressHandler = new Progress<ScanProgressReport>(report =>
        {
            Progress = report;
        });

        IProgress<ScanLogMessage> logHandler = new Progress<ScanLogMessage>(message =>
            {
                System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    LogMessages.Add(message);
                    
                    // 收集已发现的文件扩展名（用于筛选）
                    if (!string.IsNullOrWhiteSpace(message.FileExtension) && 
                        !DiscoveredExtensions.Contains(message.FileExtension))
                    {
                        DiscoveredExtensions.Add(message.FileExtension);
                    }
                });
            });

        try
        {
            var thumbnailDir = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MediaManager", "thumbnails");

            // 如果是增量扫描且有关联的媒体库，传递库ID
            int? libraryId = IsIncrementalScan && CurrentLibrary != null ? CurrentLibrary.Id : null;

            await _orchestrator.RunAsync(ScanDirectory, thumbnailDir, IncludeAudio, IncludeVideo, 
                                         IncludeImages, Recursive, progressHandler, logHandler, 
                                         _cts.Token, libraryId);
        }
        catch (OperationCanceledException)
        {
            logHandler.Report(new ScanLogMessage { Type = LogMessageType.Info, Message = "扫描已取消" });
        }
        catch (Exception ex)
        {
            logHandler.Report(new ScanLogMessage { Type = LogMessageType.Error, Message = $"扫描失败: {ex.Message}" });
        }
        finally
        {
            IsScanning = false;
        }
    }

    /// <summary>
    /// 从媒体库开始增量扫描
    /// </summary>
    public async Task StartIncrementalScanAsync(Library library)
    {
        CurrentLibrary = library;
        IsIncrementalScan = true;
        ScanDirectory = library.ScanPath;
        IncludeAudio = library.IncludeAudio;
        IncludeVideo = library.IncludeVideo;
        IncludeImages = library.IncludeImages;
        Recursive = library.Recursive;
        
        await StartScanAsync();
    }

    [RelayCommand(CanExecute = nameof(IsScanning))]
    public void CancelScan() => _cts?.Cancel();

    [RelayCommand]
    public async Task DetectDuplicatesAsync()
    {
        IsDetectingDuplicates = true;
        DuplicateGroups.Clear();
        TotalReclaimableSpace = 0;
        
        try
        {
            var duplicates = await _duplicateDetectionService.FindDuplicatesAsync();
            
            foreach (var group in duplicates)
            {
                DuplicateGroups.Add(group);
                TotalReclaimableSpace += group.ReclaimableSize;
            }
            
            if (DuplicateGroups.Count > 0)
            {
                ShowDuplicates = true;
                LogMessages.Add(new ScanLogMessage 
                { 
                    Type = LogMessageType.Info, 
                    Message = $"发现 {DuplicateGroups.Count} 组重复文件，可回收空间: {FormatFileSize(TotalReclaimableSpace)}" 
                });
            }
            else
            {
                LogMessages.Add(new ScanLogMessage 
                { 
                    Type = LogMessageType.Success, 
                    Message = "未发现重复文件" 
                });
            }
        }
        catch (Exception ex)
        {
            LogMessages.Add(new ScanLogMessage 
            { 
                Type = LogMessageType.Error, 
                Message = $"重复文件检测失败: {ex.Message}" 
            });
        }
        finally
        {
            IsDetectingDuplicates = false;
        }
    }

    [RelayCommand]
    public void ToggleShowDuplicates()
    {
        ShowDuplicates = !ShowDuplicates;
    }

    private string FormatFileSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F2} KB";
        if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F2} MB";
        return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
    }

    private bool CanScan() => !IsScanning && !string.IsNullOrWhiteSpace(ScanDirectory) && 
                              (IncludeAudio || IncludeVideo || IncludeImages);
}
