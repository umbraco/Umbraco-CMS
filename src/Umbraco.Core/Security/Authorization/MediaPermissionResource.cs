using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security.Authorization;

/// <summary>
///     A resource used for the <see cref="MediaPermissionHandler" />.
/// </summary>
public class MediaPermissionResource : IPermissionResource
{
    /// <summary>
    ///     Creates a <see cref="MediaPermissionResource" /> with the specified key.
    /// </summary>
    /// <param name="mediaKey">The key of the media or null if root.</param>
    /// <returns>An instance of <see cref="MediaPermissionResource" />.</returns>
    public static MediaPermissionResource WithKeys(Guid? mediaKey) =>
        mediaKey is null
            ? Root()
            : WithKeys(mediaKey.Value.Yield());

    /// <summary>
    ///     Creates a <see cref="MediaPermissionResource" /> with the specified key.
    /// </summary>
    /// <param name="mediaKey">The key of the media.</param>
    /// <returns>An instance of <see cref="MediaPermissionResource" />.</returns>
    public static MediaPermissionResource WithKeys(Guid mediaKey) => WithKeys(mediaKey.Yield());

    /// <summary>
    ///     Creates a <see cref="MediaPermissionResource" /> with the specified keys.
    /// </summary>
    /// <param name="mediaKeys">The keys of the medias or null if root.</param>
    /// <returns>An instance of <see cref="MediaPermissionResource" />.</returns>
    public static MediaPermissionResource WithKeys(IEnumerable<Guid?> mediaKeys)
    {
        IEnumerable<Guid?> mediaKeysList = mediaKeys.ToList();
        var hasRoot = mediaKeysList.Any(x => x is null);
        IEnumerable<Guid> keys = mediaKeysList.Where(x => x.HasValue).Select(x => x!.Value);
        return new MediaPermissionResource(keys, hasRoot, false);
    }

    /// <summary>
    ///     Creates a <see cref="MediaPermissionResource" /> with the specified keys.
    /// </summary>
    /// <param name="mediaKeys">The keys of the medias.</param>
    /// <returns>An instance of <see cref="MediaPermissionResource" />.</returns>
    public static MediaPermissionResource WithKeys(IEnumerable<Guid> mediaKeys) =>
        new(mediaKeys, false, false);

    /// <summary>
    ///     Creates a <see cref="MediaPermissionResource" /> with the root.
    /// </summary>
    /// <returns>An instance of <see cref="MediaPermissionResource" />.</returns>
    public static MediaPermissionResource Root() =>
        new(Enumerable.Empty<Guid>(), true, false);

    /// <summary>
    ///     Creates a <see cref="MediaPermissionResource" /> with the recycle bin.
    /// </summary>
    /// <returns>An instance of <see cref="MediaPermissionResource" />.</returns>
    public static MediaPermissionResource RecycleBin() =>
        new(Enumerable.Empty<Guid>(), false, true);

    private MediaPermissionResource(IEnumerable<Guid> mediaKeys, bool checkRoot, bool checkRecycleBin)
    {
        MediaKeys = mediaKeys;
        CheckRoot = checkRoot;
        CheckRecycleBin = checkRecycleBin;
    }

    /// <summary>
    ///     Gets the media keys.
    /// </summary>
    public IEnumerable<Guid> MediaKeys { get; }

    /// <summary>
    ///     Gets a value indicating whether to check the root.
    /// </summary>
    public bool CheckRoot { get; }

    /// <summary>
    ///     Gets a value indicating whether to check the recycle bin.
    /// </summary>
    public bool CheckRecycleBin { get; }
}
