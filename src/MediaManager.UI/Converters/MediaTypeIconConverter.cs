using MediaManager.Core.Enums;
using System.Globalization;
using System.Windows.Data;
using Binding = System.Windows.Data.Binding;

namespace MediaManager.UI.Converters;

/// <summary>将 MediaType 枚举转换为图标字符（Segoe MDL2 Assets 字体）</summary>
public class MediaTypeIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is MediaType t ? t switch
        {
            MediaType.Audio => "\uE8D6", // 音符图标
            MediaType.Video => "\uE786", // 视频图标
            _               => "\uE8A5"  // 文件图标
        } : "\uE8A5";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
