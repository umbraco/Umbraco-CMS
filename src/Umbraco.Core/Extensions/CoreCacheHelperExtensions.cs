// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Cache;

namespace Umbraco.Extensions;

/// <summary>
///     Extension methods for the cache helper
/// </summary>
public static class CoreCacheHelperExtensions
{
    public const string PartialViewCacheKey = "Umbraco.Web.PartialViewCacheKey";

    /// <summary>
    ///     Clears the cache for partial views
    /// </summary>
    /// <param name="appCaches"></param>
    public static void ClearPartialViewCache(this AppCaches appCaches) =>
        appCaches.RuntimeCache.ClearByKey(PartialViewCacheKey);
}
