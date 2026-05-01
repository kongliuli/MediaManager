using MediaManager.Core.Enums;

namespace MediaManager.Core.Models;

/// <summary>文档文件，继承自 MediaFile</summary>
public class DocumentFile : MediaFile
{
    /// <summary>
    /// 文档子类型（类型化访问）
    /// </summary>
    public DocumentSubType? TypedSubType
    {
        get
        {
            if (string.IsNullOrEmpty(SubType))
                return null;
            if (Enum.TryParse<DocumentSubType>(SubType, out var result))
                return result;
            return null;
        }
        set
        {
            SubType = value?.ToString();
        }
    }
}
