using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Infrastructure.PropertyEditors.NotificationHandlers;

/// <summary>
/// Provides helper methods for multiple notification handlers dealing with protection of media files for media in the recycle bin.
/// </summary>
internal static class RecycleBinMediaProtectionHelper
{
    /// <summary>
    /// Renames all file upload property files contained within a collection of media entities that have been moved to the recycle bin.
    /// </summary>
    /// <param name="filePaths">Media file paths.</param>
    /// <param name="mediaFileManager">The media file manager.</param>
    public static void SuffixContainedFiles(IEnumerable<string> filePaths, MediaFileManager mediaFileManager)
        => mediaFileManager.SuffixMediaFiles(filePaths, Constants.Conventions.Media.TrashedMediaSuffix);

    /// <summary>
    /// Renames all file upload property files contained within a collection of media entities that have been restore from the recycle bin.
    /// </summary>
    /// <param name="filePaths">Media file paths.</param>
    /// <param name="mediaFileManager">The media file manager.</param>
    public static void RemoveSuffixFromContainedFiles(IEnumerable<string> filePaths, MediaFileManager mediaFileManager)
    {
        IEnumerable<string> filePathsToRename = filePaths
            .Select(x => Path.ChangeExtension(x, Constants.Conventions.Media.TrashedMediaSuffix + Path.GetExtension(x)));
        mediaFileManager.RemoveSuffixFromMediaFiles(filePathsToRename, Constants.Conventions.Media.TrashedMediaSuffix);
    }
}
