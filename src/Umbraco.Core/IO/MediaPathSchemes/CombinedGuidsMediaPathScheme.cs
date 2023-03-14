namespace Umbraco.Cms.Core.IO.MediaPathSchemes;

/// <summary>
///     Implements a combined-guids media path scheme.
/// </summary>
/// <remarks>
///     <para>Path is "{combinedGuid}/{filename}" where combinedGuid is a combination of itemGuid and propertyGuid.</para>
/// </remarks>
public class CombinedGuidsMediaPathScheme : IMediaPathScheme
{
    /// <inheritdoc />
    public string GetFilePath(MediaFileManager fileManager, Guid itemGuid, Guid propertyGuid, string filename)
    {
        // assumes that cuid and puid keys can be trusted - and that a single property type
        // for a single content cannot store two different files with the same name
        Guid combinedGuid = GuidUtils.Combine(itemGuid, propertyGuid);
        var directory =
            HexEncoder.Encode(
                combinedGuid.ToByteArray() /*'/', 2, 4*/); // could use ext to fragment path eg 12/e4/f2/...
        return Path.Combine(directory, filename).Replace('\\', '/');
    }

    /// <inheritdoc />
    public string GetDeleteDirectory(MediaFileManager fileSystem, string filepath) => Path.GetDirectoryName(filepath)!;
}
