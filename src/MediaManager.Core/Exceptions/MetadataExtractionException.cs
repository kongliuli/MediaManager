namespace MediaManager.Core.Exceptions;

public class MetadataExtractionException(string filePath, Exception inner)
    : Exception($"Failed to extract metadata from '{filePath}'.", inner);
