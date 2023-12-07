using System.Collections;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Media;

/// <summary>
///     A resource used for the <see cref="MediaPermissionResource" />.
/// </summary>
public class MediaPermissionResource : IPermissionResource
{
    public static MediaPermissionResource WithKeys(Guid? mediaKey) => mediaKey is null ? Root() : WithKeys(mediaKey.Value.Yield());

    public static MediaPermissionResource WithKeys(Guid mediaKey) => WithKeys(mediaKey.Yield());

    public static MediaPermissionResource WithKeys(IEnumerable<Guid?> mediaKeys)
    {
        bool hasRoot = mediaKeys.Any(x => x is null);
        IEnumerable<Guid> keys = mediaKeys.Where(x => x.HasValue).Select(x => x!.Value);
        return new MediaPermissionResource(keys, hasRoot, false);
    }

    public static MediaPermissionResource WithKeys(IEnumerable<Guid> mediaKeys) =>
        new MediaPermissionResource(mediaKeys, false, false);

    public static MediaPermissionResource Root() =>
        new MediaPermissionResource(Enumerable.Empty<Guid>(), true, false);

    public static MediaPermissionResource RecycleBin() =>
        new MediaPermissionResource(Enumerable.Empty<Guid>(), false, true);

    private MediaPermissionResource(IEnumerable<Guid> mediaKeys, bool checkRoot, bool checkRecycleBin)
    {
        MediaKeys = mediaKeys;
        CheckRoot = checkRoot;
        CheckRecycleBin = checkRecycleBin;
    }

    /// <summary>
    ///     Gets the content keys.
    /// </summary>
    public IEnumerable<Guid> MediaKeys { get; }

    /// <summary>
    ///     Gets whether to check the root.
    /// </summary>
    public bool CheckRoot { get; }

    /// <summary>
    ///     Gets whether to check the recylce bin.
    /// </summary>
    public bool CheckRecycleBin { get; }
}
