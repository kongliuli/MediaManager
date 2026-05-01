using MediaManager.Core.Enums;

namespace MediaManager.Core.Interfaces;

/// <summary>
/// 媒体组件基接口，定义所有媒体组件的核心功能
/// </summary>
public interface IMediaComponent
{
    /// <summary>
    /// 获取组件唯一标识符
    /// </summary>
    string ComponentId { get; }

    /// <summary>
    /// 获取组件名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 获取组件描述
    /// </summary>
    string Description { get; }

    /// <summary>
    /// 获取组件版本号
    /// </summary>
    string Version { get; }

    /// <summary>
    /// 获取依赖的组件类型列表
    /// </summary>
    IReadOnlyList<Type> Dependencies { get; }

    /// <summary>
    /// 执行组件功能
    /// </summary>
    /// <param name="context">组件执行上下文</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>执行结果</returns>
    Task<object?> ExecuteAsync(ComponentContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查组件是否适用于指定的媒体类型
    /// </summary>
    /// <param name="mediaType">媒体类型</param>
    /// <param name="subType">媒体子类型（可选）</param>
    /// <returns>是否适用</returns>
    bool IsApplicable(MediaType mediaType, object? subType = null);
}
