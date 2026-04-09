namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Represents event arguments that contain a list of media files to be deleted.
/// </summary>
public interface IDeletingMediaFilesEventArgs
{
    /// <summary>
    ///     Gets the list of media file paths that should be deleted.
    /// </summary>
    List<string> MediaFilesToDelete { get; }
}
