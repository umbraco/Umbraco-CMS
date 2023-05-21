namespace Umbraco.Cms.Core.IO;

/// <summary>
///     Represents a media file path scheme.
/// </summary>
public interface IMediaPathScheme
{
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
}
