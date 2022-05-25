using System.Runtime.CompilerServices;

namespace Umbraco.Cms.Infrastructure.PublishedCache;

internal static class CacheKeys
{
    public static string PublishedContentChildren(Guid contentUid, bool previewing) =>
        "NuCache.Content.Children[" + DraftOrPub(previewing) + ":" + contentUid + "]";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string DraftOrPub(bool previewing) => previewing ? "D:" : "P:";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string LangId(string? culture)
        => string.IsNullOrEmpty(culture) ? string.Empty : "-L:" + culture;

    public static string ContentCacheRoots(bool previewing) =>
        "NuCache.ContentCache.Roots[" + DraftOrPub(previewing) + "]";

    public static string MediaCacheRoots(bool previewing) => "NuCache.MediaCache.Roots[" + DraftOrPub(previewing) + "]";

    public static string PublishedContentAsPreviewing(Guid contentUid) =>
        "NuCache.Content.AsPreviewing[" + contentUid + "]";

    public static string ProfileName(int userId) => "NuCache.Profile.Name[" + userId + "]";

    public static string PropertyCacheValues(Guid contentUid, string typeAlias, bool previewing) =>
        "NuCache.Property.CacheValues[" + DraftOrPub(previewing) + contentUid + ":" + typeAlias + "]";

    // routes still use int id and not Guid uid, because routable nodes must have
    // a valid ID in the database at that point, whereas content and properties
    // may be virtual (and not in umbracoNode).
    public static string ContentCacheRouteByContent(int id, bool previewing, string? culture) =>
        "NuCache.ContentCache.RouteByContent[" + DraftOrPub(previewing) + id + LangId(culture) + "]";

    public static string ContentCacheContentByRoute(string route, bool previewing, string? culture) =>
        "NuCache.ContentCache.ContentByRoute[" + DraftOrPub(previewing) + route + LangId(culture) + "]";
}
