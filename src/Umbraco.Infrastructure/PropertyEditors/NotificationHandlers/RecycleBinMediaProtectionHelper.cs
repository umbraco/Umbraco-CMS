using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;

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
        IEnumerable<string> filePathsToRename = filePaths.Select(GetTrashedFilePath);
        mediaFileManager.RemoveSuffixFromMediaFiles(filePathsToRename, Constants.Conventions.Media.TrashedMediaSuffix);
    }

    /// <summary>
    /// Deletes all media files.
    /// </summary>
    /// <param name="deletedMedia">Deleted media entities.</param>
    /// <param name="containedFilePaths">Function to extract file paths from media entities.</param>
    /// <param name="mediaFileManager">The media file manager.</param>
    /// <remarks>
    /// Trashed files have .deleted suffix on disk.
    /// We delete the file at both its suffixed and original path, to account for cases where protection has
    /// been toggled with media items already in the recycle bin.
    /// </remarks>
    public static void DeleteContainedFilesWithProtection(
        IEnumerable<IMedia> deletedMedia,
        Func<IEnumerable<IMedia>, IEnumerable<string>> containedFilePaths,
        MediaFileManager mediaFileManager)
    {
        // Typically all deleted media will have Trashed == true since they come from the recycle bin.
        // However, media can be force-deleted programmatically bypassing the recycle bin, in which
        // case the files won't have the .deleted suffix on disk. We handle both cases here.
        var trashedMedia = deletedMedia.Where(m => m.Trashed).ToList();
        var nonTrashedMedia = deletedMedia.Where(m => !m.Trashed).ToList();

        // A trashed item's file may not have been renamed, so delete it at either path.
        if (trashedMedia.Count > 0)
        {
            var originalPaths = containedFilePaths(trashedMedia).ToList();
            IEnumerable<string> trashedPaths = originalPaths.Select(GetTrashedFilePath);
            mediaFileManager.DeleteMediaFiles(trashedPaths.Concat(originalPaths));
        }

        // Delete non-trashed media files (original paths).
        if (nonTrashedMedia.Count > 0)
        {
            mediaFileManager.DeleteMediaFiles(containedFilePaths(nonTrashedMedia));
        }
    }

    private static string GetTrashedFilePath(string filePath)
        => Path.ChangeExtension(filePath, Constants.Conventions.Media.TrashedMediaSuffix + Path.GetExtension(filePath));
}
