using System.ComponentModel;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents configuration settings for cache entry durations.
/// </summary>
/// <remarks>
///     This class defines the time-to-live (TTL) settings for different cache layers
///     including local (in-memory), remote (distributed), and seed caches.
/// </remarks>
public class CacheEntrySettings
{
    /// <summary>
    ///     The default duration for local cache entries (1 day).
    /// </summary>
    internal const string StaticLocalCacheDuration = "1.00:00:00";

    /// <summary>
    ///     The default duration for remote cache entries (365 days).
    /// </summary>
    internal const string StaticRemoteCacheDuration = "365.00:00:00";

    /// <summary>
    ///     The default duration for seed cache entries (365 days).
    /// </summary>
    internal const string StaticSeedCacheDuration = "365.00:00:00";

    /// <summary>
    ///     Gets or sets the duration that items remain in the local (in-memory) cache.
    /// </summary>
    /// <value>
    ///     The local cache duration. Defaults to 1 day.
    /// </value>
    [DefaultValue(StaticLocalCacheDuration)]
    public  TimeSpan LocalCacheDuration { get; set; } = TimeSpan.Parse(StaticLocalCacheDuration);

    /// <summary>
    ///     Gets or sets the duration that items remain in the remote (distributed) cache.
    /// </summary>
    /// <value>
    ///     The remote cache duration. Defaults to 365 days.
    /// </value>
    [DefaultValue(StaticRemoteCacheDuration)]
    public  TimeSpan RemoteCacheDuration { get; set; } = TimeSpan.Parse(StaticRemoteCacheDuration);

    /// <summary>
    ///     Gets or sets the duration that items remain in the seed cache.
    /// </summary>
    /// <value>
    ///     The seed cache duration. Defaults to 365 days.
    /// </value>
    /// <remarks>
    ///     The seed cache is used during application startup to pre-populate the cache
    ///     with frequently accessed content.
    /// </remarks>
    [DefaultValue(StaticSeedCacheDuration)]
    public  TimeSpan SeedCacheDuration { get; set; } = TimeSpan.Parse(StaticSeedCacheDuration);

}
