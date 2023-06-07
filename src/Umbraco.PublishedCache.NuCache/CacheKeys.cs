using System.Runtime.CompilerServices;

namespace Umbraco.Cms.Infrastructure.PublishedCache;

internal static class CacheKeys
{
    public static string PublishedContentChildren(Guid contentUid, bool previewing)
    {
        if (previewing)
        {
            return "NuCache.Content.Children[D::" + contentUid + "]";
        }

        return "NuCache.Content.Children[P::" + contentUid + "]";
    }

    public static string ContentCacheRoots(bool previewing)
    {
        if (previewing)
        {
            return "NuCache.ContentCache.Roots[D:]";
        }

        return "NuCache.ContentCache.Roots[P:]";
    }

    public static string MediaCacheRoots(bool previewing)
    {
        if (previewing)
        {
            return "NuCache.MediaCache.Roots[D:]";
        }

        return "NuCache.MediaCache.Roots[P:]";
    }

    public static string PublishedContentAsPreviewing(Guid contentUid) =>
        "NuCache.Content.AsPreviewing[" + contentUid + "]";

    public static string ProfileName(int userId) => "NuCache.Profile.Name[" + userId + "]";

    public static string PropertyCacheValues(Guid contentUid, string typeAlias, bool previewing)
    {
        if (previewing)
        {
            return "NuCache.Property.CacheValues[D:" + contentUid + ":" + typeAlias + "]";
        }

        return "NuCache.Property.CacheValues[P:" + contentUid + ":" + typeAlias + "]";
    }

    // routes still use int id and not Guid uid, because routable nodes must have
    // a valid ID in the database at that point, whereas content and properties
    // may be virtual (and not in umbracoNode).
    public static string ContentCacheRouteByContent(int id, bool previewing, string? culture)
    {
        if (string.IsNullOrEmpty(culture))
        {
            if (previewing)
            {
                return "NuCache.ContentCache.RouteByContent[D:" + id +"]";
            }

            return "NuCache.ContentCache.RouteByContent[P:" + id + "]";
        }
        else if (previewing)
        {
            return "NuCache.ContentCache.RouteByContent[D:" + id + "-L:" + culture + "]";
        }
        return "NuCache.ContentCache.RouteByContent[P:" + id + "-L:" + culture + "]";
    }

    public static string ContentCacheContentByRoute(string route, bool previewing, string? culture)
    {
        if (string.IsNullOrEmpty(culture))
        {
            if (previewing)
            {
                return "NuCache.ContentCache.ContentByRoute[D:" + route + "]";
            }

            return "NuCache.ContentCache.ContentByRoute[P:" + route + "]";
        }
        else if (previewing)
        {
            return "NuCache.ContentCache.ContentByRoute[D:" + route + "-L:" + culture + "]";
        }

        return "NuCache.ContentCache.ContentByRoute[P:" + route + "-L:" + culture + "]";
    }
}
