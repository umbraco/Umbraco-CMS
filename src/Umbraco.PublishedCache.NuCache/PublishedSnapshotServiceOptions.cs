using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Infrastructure.PublishedCache;

/// <summary>
///     Options class for configuring the <see cref="IPublishedSnapshotService" />
/// </summary>
public class PublishedSnapshotServiceOptions
{
    // disabled: prevents the published snapshot from updating and exposing changes
    //           or even creating a new published snapshot to see changes, uses old cache = bad
    //
    //// indicates that the snapshot cache should reuse the application request cache
    //// otherwise a new cache object would be created for the snapshot specifically,
    //// which is the default - web boot manager uses this to optimize facades
    // public bool PublishedSnapshotCacheIsApplicationRequestCache;

    /// <summary>
    ///     If true this disables the persisted local cache files for content and media
    /// </summary>
    /// <remarks>
    ///     By default this is false which means umbraco will use locally persisted cache files for reading in all published
    ///     content and media on application startup.
    ///     The reason for this is to improve startup times because the alternative to populating the published content and
    ///     media on application startup is to read
    ///     these values from the database. In scenarios where sites are relatively small (below a few thousand nodes) reading
    ///     the content/media from the database to populate
    ///     the in memory cache isn't that slow and is only marginally slower than reading from the locally persisted cache
    ///     files.
    /// </remarks>
    public bool IgnoreLocalDb { get; set; }
}
