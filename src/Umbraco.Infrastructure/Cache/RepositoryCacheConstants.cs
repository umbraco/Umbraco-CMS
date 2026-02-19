// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Provides default values for repository caching.
/// </summary>
internal static class RepositoryCacheConstants
{
    /// <summary>
    ///     The default sliding expiration for individual entity cache entries in repositories.
    /// </summary>
    public static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(5);
}
