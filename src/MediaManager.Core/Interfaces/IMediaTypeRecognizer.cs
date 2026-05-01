using MediaManager.Core.Enums;

namespace MediaManager.Core.Interfaces;

/// <summary>
/// 媒体类型识别接口
/// </summary>
public interface IMediaTypeRecognizer
{
    /// <summary>
    /// 根据文件路径识别媒体主类型
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>识别到的媒体主类型</returns>
    MediaType RecognizeMediaType(string filePath);

    /// <summary>
    /// 根据文件路径识别音频子类型
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>识别到的音频子类型，若无法识别则为null</returns>
    AudioSubType? RecognizeAudioSubType(string filePath);

    /// <summary>
    /// 根据文件路径识别视频子类型
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>识别到的视频子类型，若无法识别则为null</returns>
    VideoSubType? RecognizeVideoSubType(string filePath);

    /// <summary>
    /// 根据文件路径识别图像子类型
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>识别到的图像子类型，若无法识别则为null</returns>
    ImageSubType? RecognizeImageSubType(string filePath);

    /// <summary>
    /// 根据文件路径识别文档子类型
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>识别到的文档子类型，若无法识别则为null</returns>
    DocumentSubType? RecognizeDocumentSubType(string filePath);

    /// <summary>
    /// 检查文件是否支持子类型识别
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>是否支持子类型识别</returns>
    bool SupportsSubTypeRecognition(string filePath);
}
