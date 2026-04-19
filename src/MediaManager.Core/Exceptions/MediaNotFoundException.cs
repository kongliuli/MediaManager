namespace MediaManager.Core.Exceptions;

public class MediaNotFoundException(long id) : Exception($"Media file with ID {id} was not found.");
