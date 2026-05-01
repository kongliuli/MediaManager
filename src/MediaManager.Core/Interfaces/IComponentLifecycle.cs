namespace MediaManager.Core.Interfaces;

/// <summary>
/// 组件生命周期管理接口，定义组件的初始化、启用、禁用和销毁操作
/// </summary>
public interface IComponentLifecycle
{
    /// <summary>
    /// 组件初始化，在组件首次加载时调用
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>初始化是否成功</returns>
    Task<bool> InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 组件启用，在组件被激活时调用
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>启用是否成功</returns>
    Task<bool> EnableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 组件禁用，在组件被暂停时调用
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>禁用是否成功</returns>
    Task<bool> DisableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 组件销毁，在组件卸载时调用，用于清理资源
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>销毁是否成功</returns>
    Task<bool> DestroyAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取组件当前状态
    /// </summary>
    ComponentState State { get; }
}

/// <summary>
/// 组件状态枚举
/// </summary>
public enum ComponentState
{
    /// <summary>
    /// 未初始化
    /// </summary>
    Uninitialized,

    /// <summary>
    /// 初始化中
    /// </summary>
    Initializing,

    /// <summary>
    /// 已初始化但未启用
    /// </summary>
    Initialized,

    /// <summary>
    /// 启用中
    /// </summary>
    Enabling,

    /// <summary>
    /// 已启用
    /// </summary>
    Enabled,

    /// <summary>
    /// 禁用中
    /// </summary>
    Disabling,

    /// <summary>
    /// 已禁用
    /// </summary>
    Disabled,

    /// <summary>
    /// 销毁中
    /// </summary>
    Destroying,

    /// <summary>
    /// 已销毁
    /// </summary>
    Destroyed,

    /// <summary>
    /// 错误状态
    /// </summary>
    Error
}
