namespace Umbraco.Cms.Api.Management.Security.Authorization.Media;

/// <summary>
///     A resource used for the <see cref="MediaPermissionResource" />.
/// </summary>
public class MediaPermissionResource : IPermissionResource
{

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaPermissionResource" /> class.
    /// </summary>
    /// <param name="mediaKey">The key of the media item.</param>
    public MediaPermissionResource(Guid mediaKey)
    {
        MediaKeys = new List<Guid> { mediaKey };
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaPermissionResource" /> class.
    /// </summary>
    /// <param name="mediaKeys">The keys of the user items.</param>
    public MediaPermissionResource(IEnumerable<Guid> mediaKeys)
    {
        MediaKeys = mediaKeys;
    }

    /// <summary>
    ///     Gets the content keys.
    /// </summary>
    public IEnumerable<Guid> MediaKeys { get; }

}
