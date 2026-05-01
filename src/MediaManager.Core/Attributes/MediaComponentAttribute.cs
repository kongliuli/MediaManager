using MediaManager.Core.Enums;

namespace MediaManager.Core.Attributes;

/// <summary>
/// 媒体组件特性，用于标记组件并指定其适用的媒体类型
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class MediaComponentAttribute : Attribute
{
    /// <summary>
    /// 获取或设置组件唯一标识符
    /// </summary>
    public string ComponentId { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置组件名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置组件描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置组件版本
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// 获取或设置支持的主媒体类型集合
    /// </summary>
    public MediaType[] SupportedMediaTypes { get; set; } = [];

    /// <summary>
    /// 获取或设置支持的音频子类型集合
    /// </summary>
    public AudioSubType[] SupportedAudioSubTypes { get; set; } = [];

    /// <summary>
    /// 获取或设置支持的视频子类型集合
    /// </summary>
    public VideoSubType[] SupportedVideoSubTypes { get; set; } = [];

    /// <summary>
    /// 获取或设置支持的图像子类型集合
    /// </summary>
    public ImageSubType[] SupportedImageSubTypes { get; set; } = [];

    /// <summary>
    /// 获取或设置支持的文档子类型集合
    /// </summary>
    public DocumentSubType[] SupportedDocumentSubTypes { get; set; } = [];

    /// <summary>
    /// 获取或设置依赖的组件类型集合
    /// </summary>
    public Type[] Dependencies { get; set; } = [];

    /// <summary>
    /// 获取或设置组件优先级（数值越大优先级越高）
    /// </summary>
    public int Priority { get; set; } = 0;

    /// <summary>
    /// 获取或设置是否为核心组件
    /// </summary>
    public bool IsCoreComponent { get; set; } = false;

    /// <summary>
    /// 初始化 MediaComponentAttribute 类的新实例
    /// </summary>
    public MediaComponentAttribute()
    {
    }

    /// <summary>
    /// 使用指定的组件ID、名称和支持的媒体类型初始化 MediaComponentAttribute 类的新实例
    /// </summary>
    /// <param name="componentId">组件唯一标识符</param>
    /// <param name="name">组件名称</param>
    /// <param name="supportedMediaTypes">支持的主媒体类型</param>
    public MediaComponentAttribute(string componentId, string name, params MediaType[] supportedMediaTypes)
    {
        ComponentId = componentId;
        Name = name;
        SupportedMediaTypes = supportedMediaTypes;
    }
}
