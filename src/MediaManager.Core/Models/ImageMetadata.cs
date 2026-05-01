namespace MediaManager.Core.Models;

/// <summary>
/// 图像文件扩展元数据
/// </summary>
public class ImageMetadata
{
    /// <summary>
    /// 相机制造商
    /// </summary>
    public string? CameraMake { get; set; }

    /// <summary>
    /// 相机型号
    /// </summary>
    public string? CameraModel { get; set; }

    /// <summary>
    /// 镜头型号
    /// </summary>
    public string? LensModel { get; set; }

    /// <summary>
    /// 焦距（毫米）
    /// </summary>
    public double? FocalLength { get; set; }

    /// <summary>
    /// 光圈值
    /// </summary>
    public double? Aperture { get; set; }

    /// <summary>
    /// 快门速度（秒）
    /// </summary>
    public double? ShutterSpeed { get; set; }

    /// <summary>
    /// ISO 值
    /// </summary>
    public int? ISO { get; set; }

    /// <summary>
    /// 曝光补偿
    /// </summary>
    public double? ExposureCompensation { get; set; }

    /// <summary>
    /// 白平衡
    /// </summary>
    public string? WhiteBalance { get; set; }

    /// <summary>
    /// 闪光灯是否使用
    /// </summary>
    public bool? FlashUsed { get; set; }

    /// <summary>
    /// 拍摄地点纬度
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// 拍摄地点经度
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// 拍摄地点海拔
    /// </summary>
    public double? Altitude { get; set; }

    /// <summary>
    /// 作者/摄影师
    /// </summary>
    public string? Artist { get; set; }

    /// <summary>
    /// 版权信息
    /// </summary>
    public string? Copyright { get; set; }

    /// <summary>
    /// 图像描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 颜色空间
    /// </summary>
    public string? ColorSpace { get; set; }

    /// <summary>
    /// 位深度
    /// </summary>
    public int? BitDepth { get; set; }
}
