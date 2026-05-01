using MediaManager.UI.DisplayModels;

namespace MediaManager.UI.ViewModels;

/// <summary>选中媒体文件时广播，DetailViewModel 和 MainViewModel 订阅</summary>
public record MediaSelectedMessage(MediaFileDisplayItem Item);

/// <summary>导航到指定 ViewModel 类型</summary>
public record NavigateMessage(Type ViewModelType);
