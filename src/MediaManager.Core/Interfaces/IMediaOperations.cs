using MediaManager.Core.Models;

namespace MediaManager.Core.Interfaces;

/// <summary>
/// 主页面媒体操作契约，定义主页面可以执行的媒体管理操作
/// </summary>
public interface IMediaOperations
{
    /// <summary>
    /// 批量选择媒体文件
    /// </summary>
    /// <param name="mediaIds">媒体文件ID集合</param>
    void SelectMedia(IEnumerable<long> mediaIds);

    /// <summary>
    /// 取消选择所有媒体文件
    /// </summary>
    void DeselectAll();

    /// <summary>
    /// 批量删除媒体文件
    /// </summary>
    /// <param name="mediaIds">媒体文件ID集合</param>
    /// <param name="permanent">是否永久删除</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>删除结果</returns>
    Task<bool> DeleteMediaAsync(IEnumerable<long> mediaIds, bool permanent = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量移动媒体文件到指定媒体库
    /// </summary>
    /// <param name="mediaIds">媒体文件ID集合</param>
    /// <param name="targetLibraryId">目标媒体库ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>移动结果</returns>
    Task<bool> MoveMediaAsync(IEnumerable<long> mediaIds, int targetLibraryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量添加标签到媒体文件
    /// </summary>
    /// <param name="mediaIds">媒体文件ID集合</param>
    /// <param name="tagNames">标签名称集合</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>添加结果</returns>
    Task<bool> AddTagsAsync(IEnumerable<long> mediaIds, IEnumerable<string> tagNames, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量从媒体文件移除标签
    /// </summary>
    /// <param name="mediaIds">媒体文件ID集合</param>
    /// <param name="tagNames">标签名称集合</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>移除结果</returns>
    Task<bool> RemoveTagsAsync(IEnumerable<long> mediaIds, IEnumerable<string> tagNames, CancellationToken cancellationToken = default);

    /// <summary>
    /// 导出媒体文件元数据
    /// </summary>
    /// <param name="mediaIds">媒体文件ID集合</param>
    /// <param name="exportPath">导出路径</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>导出结果</returns>
    Task<bool> ExportMetadataAsync(IEnumerable<long> mediaIds, string exportPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// 执行操作链
    /// </summary>
    /// <param name="operationChain">操作链</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>执行结果</returns>
    Task<bool> ExecuteOperationChainAsync(OperationChain operationChain, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取当前选中的媒体文件
    /// </summary>
    /// <returns>选中的媒体文件列表</returns>
    IReadOnlyList<MediaFile> GetSelectedMedia();

    /// <summary>
    /// 刷新媒体列表
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>刷新是否成功</returns>
    Task<bool> RefreshMediaListAsync(CancellationToken cancellationToken = default);
}
