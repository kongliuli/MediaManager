namespace MediaManager.Core.Exceptions;

public class UnsupportedFormatException(string extension)
    : Exception($"File format '{extension}' is not supported.");
