using MediaManager.Core.Enums;

namespace MediaManager.Core.Models;

/// <summary>
/// 组件执行上下文，包含组件执行所需的所有信息和资源
/// </summary>
public class ComponentContext
{
    /// <summary>
    /// 获取或设置当前处理的媒体文件集合
    /// </summary>
    public IReadOnlyList<MediaFile> MediaFiles { get; set; } = [];

    /// <summary>
    /// 获取或设置当前媒体类型
    /// </summary>
    public MediaType? CurrentMediaType { get; set; }

    /// <summary>
    /// 获取或设置当前媒体子类型
    /// </summary>
    public object? CurrentMediaSubType { get; set; }

    /// <summary>
    /// 获取或设置上下文参数字典
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();

    /// <summary>
    /// 获取或设置操作结果存储
    /// </summary>
    public Dictionary<string, object> Results { get; set; } = new();

    /// <summary>
    /// 获取或设置服务提供程序
    /// </summary>
    public IServiceProvider? ServiceProvider { get; set; }

    /// <summary>
    /// 获取或设置操作开始时间
    /// </summary>
    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 获取或设置操作ID
    /// </summary>
    public string OperationId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 获取或设置父组件上下文（用于嵌套组件调用）
    /// </summary>
    public ComponentContext? ParentContext { get; set; }

    /// <summary>
    /// 初始化 ComponentContext 类的新实例
    /// </summary>
    public ComponentContext()
    {
    }

    /// <summary>
    /// 使用指定的媒体文件初始化 ComponentContext 类的新实例
    /// </summary>
    /// <param name="mediaFiles">媒体文件集合</param>
    public ComponentContext(IEnumerable<MediaFile> mediaFiles)
    {
        MediaFiles = mediaFiles.ToList();
    }

    /// <summary>
    /// 获取指定名称的参数值
    /// </summary>
    /// <typeparam name="T">参数类型</typeparam>
    /// <param name="key">参数名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>参数值</returns>
    public T? GetParameter<T>(string key, T? defaultValue = default)
    {
        if (Parameters.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return defaultValue;
    }

    /// <summary>
    /// 设置指定名称的参数值
    /// </summary>
    /// <param name="key">参数名称</param>
    /// <param name="value">参数值</param>
    public void SetParameter(string key, object value)
    {
        Parameters[key] = value;
    }

    /// <summary>
    /// 获取指定名称的结果值
    /// </summary>
    /// <typeparam name="T">结果类型</typeparam>
    /// <param name="key">结果名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>结果值</returns>
    public T? GetResult<T>(string key, T? defaultValue = default)
    {
        if (Results.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return defaultValue;
    }

    /// <summary>
    /// 设置指定名称的结果值
    /// </summary>
    /// <param name="key">结果名称</param>
    /// <param name="value">结果值</param>
    public void SetResult(string key, object value)
    {
        Results[key] = value;
    }

    /// <summary>
    /// 创建子上下文
    /// </summary>
    /// <returns>子上下文</returns>
    public ComponentContext CreateChildContext()
    {
        return new ComponentContext
        {
            MediaFiles = MediaFiles,
            CurrentMediaType = CurrentMediaType,
            CurrentMediaSubType = CurrentMediaSubType,
            Parameters = new Dictionary<string, object>(Parameters),
            Results = new Dictionary<string, object>(Results),
            ServiceProvider = ServiceProvider,
            OperationId = $"{OperationId}-{Guid.NewGuid():N}",
            ParentContext = this
        };
    }
}
