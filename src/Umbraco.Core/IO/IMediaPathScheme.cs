namespace Umbraco.Cms.Core.IO;

/// <summary>
///     Represents a media file path scheme.
/// </summary>
public interface IMediaPathScheme
{
    /// <summary>
    /// Gets a value indicating whether GUID version 7 is supported.
    /// </summary>
    bool SupportsGuid7 { get; }

    /// <summary>
    ///     Gets a media file path.
    /// </summary>
    /// <param name="fileManager">The media filesystem.</param>
    /// <param name="itemGuid">The (content, media) item unique identifier.</param>
    /// <param name="propertyGuid">The property type unique identifier.</param>
    /// <param name="filename">The file name.</param>
    /// <returns>The filesystem-relative complete file path.</returns>
    string GetFilePath(MediaFileManager fileManager, Guid itemGuid, Guid propertyGuid, string filename);

    /// <summary>
    ///     Gets the directory that can be deleted when the file is deleted.
    /// </summary>
    /// <param name="fileSystem">The media filesystem.</param>
    /// <param name="filepath">The filesystem-relative path of the file.</param>
    /// <returns>The filesystem-relative path of the directory.</returns>
    /// <remarks>
    ///     <para>The directory, and anything below it, will be deleted.</para>
    ///     <para>Can return null (or empty) when no directory should be deleted.</para>
    /// </remarks>
    string? GetDeleteDirectory(MediaFileManager fileSystem, string filepath);

    /// <summary>
    ///     Determines whether a media file path is one this scheme could have produced for the given content and
    ///     property type.
    /// </summary>
    /// <param name="fileManager">The media filesystem.</param>
    /// <param name="filepath">The filesystem-relative path of the file to check.</param>
    /// <param name="itemGuid">The (content, media) item unique identifier the file is expected to belong to.</param>
    /// <param name="propertyGuid">The property type unique identifier the file is expected to belong to.</param>
    /// <returns>
    ///     <c>true</c> if the path could belong to <paramref name="itemGuid" /> and <paramref name="propertyGuid" />;
    ///     otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         Used before deleting or renaming a file referenced by a stored property value, to confirm the path
    ///         belongs to the item being processed rather than one supplied by a client to target another item's file.
    ///     </para>
    /// </remarks>
    // TODO (V19): Remove default implementation.
    bool IsFilePathOwnedBy(MediaFileManager fileManager, string filepath, Guid itemGuid, Guid propertyGuid) => true;
}
