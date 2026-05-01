namespace MediaManager.Core.Enums;

/// <summary>
/// 扫描步骤枚举
/// </summary>
public enum ScanStep
{
    Initializing,
    DiscoveringFiles,
    CalculatingHash,
    ExtractingMetadata,
    SavingToDatabase,
    GeneratingThumbnail,
    GeneratingWaveform,
    Completed,
    Failed,
    Cancelled
}