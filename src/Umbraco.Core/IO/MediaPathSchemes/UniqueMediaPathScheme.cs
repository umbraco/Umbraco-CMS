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
        // Shortening of the Guid to 8 chars risks collisions with GUIDs, which are expected to be very rare.
        // However with GUID 7, the chance is much higher, as those created in a short period of time could have
        // the same first 8 characters.
        // Such GUIDs would have been created via Guid.CreateVersion7() rather than Guid.NewGuid().
        // We should detect that, throw, and recommend creation of standard GUIDs or the use of a custom IMediaPathScheme instead.
        if (itemGuid.Version == 7 || propertyGuid.Version == 7)
        {
            throw new ArgumentException(
                "The UniqueMediaPathScheme cannot be used with version 7 GUIDs due to an increased risk of collisions in the generated file paths. " +
                "Please use version 4 GUIDs created via Guid.NewGuid() or implement and register a different IMediaPathScheme.");
        }

        Guid combinedGuid = GuidUtils.Combine(itemGuid, propertyGuid);
        var directory = GuidUtils.ToBase32String(combinedGuid, DirectoryLength);

        return Path.Combine(directory, filename).Replace('\\', '/');
    }

    /// <inheritdoc />
    public string? GetDeleteDirectory(MediaFileManager fileManager, string filepath)
    {
        string? directory = Path.GetDirectoryName(filepath);

        if (string.IsNullOrEmpty(directory))
        {
            return null;
        }

        IEnumerable<string> files = fileManager.FileSystem.GetFiles(directory);

        if (files.Any())
        {
            return null;
        }

        return directory;
    }
}
