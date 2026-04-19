using MediaManager.Core.DTOs;

namespace MediaManager.Core.Messages;

public record ScanCompletedMessage(ScanProgressReport Report);
