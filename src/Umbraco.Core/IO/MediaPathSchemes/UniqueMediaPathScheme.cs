namespace Umbraco.Cms.Core.IO.MediaPathSchemes;

/// <summary>
///     Implements a unique directory media path scheme.
/// </summary>
/// <remarks>
///     <para>This scheme provides deterministic short paths, with potential collisions.</para>
/// </remarks>
public class UniqueMediaPathScheme : IMediaPathScheme
{
    private const int DirectoryLength = 8;

    /// <inheritdoc />
    public string GetFilePath(MediaFileManager fileManager, Guid itemGuid, Guid propertyGuid, string filename)
    {
        Guid combinedGuid = GuidUtils.Combine(itemGuid, propertyGuid);
        var directory = GuidUtils.ToBase32String(combinedGuid, DirectoryLength);

        return Path.Combine(directory, filename).Replace('\\', '/');
    }

    /// <inheritdoc />
    /// <remarks>
    ///     <para>
    ///         Returning null so that <see cref="MediaFileSystem.DeleteMediaFiles" /> does *not*
    ///         delete any directory. This is because the above shortening of the Guid to 8 chars
    ///         means we're increasing the risk of collision, and we don't want to delete files
    ///         belonging to other media items.
    ///     </para>
    ///     <para>
    ///         And, at the moment, we cannot delete directory "only if it is empty" because of
    ///         race conditions. We'd need to implement locks in <see cref="MediaFileSystem" /> for
    ///         this.
    ///     </para>
    /// </remarks>
    public string? GetDeleteDirectory(MediaFileManager fileManager, string filepath) => null;
}
